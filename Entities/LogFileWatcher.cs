using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FallGuysStats {
    public class LogLine {
        public TimeSpan Time { get; } = TimeSpan.Zero;
        public DateTime Date { get; set; } = DateTime.MinValue;
        public string Line { get; set; }
        public bool IsValid { get; set; }
        public long Offset { get; set; }

        public LogLine(string line, long offset) {
            this.Offset = offset;
            this.Line = line;
            bool isValidSemiColon = line.IndexOf(':') == 2 && line.IndexOf(':', 3) == 5 && line.IndexOf(':', 6) == 12;
            bool isValidDot = line.IndexOf('.') == 2 && line.IndexOf('.', 3) == 5 && line.IndexOf(':', 6) == 12;
            this.IsValid = isValidSemiColon || isValidDot;
            if (this.IsValid) {
                this.Time = TimeSpan.ParseExact(line.Substring(0, 12), isValidSemiColon ? "hh\\:mm\\:ss\\.fff" : "hh\\.mm\\.ss\\.fff", null);
            }
        }

        public override string ToString() {
            return $"{this.Time}: {this.Line} ({this.Offset})";
        }
    }
    public class LogRound {
        public bool CurrentlyInParty;
        public bool PrivateLobby;
        public bool FindingPosition;
        public bool IsFinal;
        public bool HasIsFinal;
        public string CurrentPlayerID;
        public int Duration;

        public static int GetInfo { get; set; }
        public static bool IsInfoEmpty { get; set; }
        public static int SavedCount { get; set; }
        public static bool IsPlaying { get; set; }
        public static bool IsSpectating { get; set; }
        public static bool IsLastPlayed { get; set; }

        public static DateTime End { get; set; } = DateTime.MinValue;

        public RoundInfo Info;
    }
    public class LogFileWatcher {
        private const int UpdateDelay = 500;

        private string filePath;
        private string prevFilePath;
        private List<LogLine> lines = new List<LogLine>();
        private bool running;
        private bool stop;
        private Thread watcher, parser;
        public Stats StatsForm { get; set; }
        private string selectedShowId;
        //private readonly object balanceLock = new object();

        public event Action<List<RoundInfo>> OnParsedLogLines;
        public event Action<List<RoundInfo>> OnParsedLogLinesCurrent;
        public event Action<DateTime> OnNewLogFileDate;
        public event Action<string> OnError;

        public void Start(string logDirectory, string fileName) {
            if (this.running) { return; }

            this.filePath = Path.Combine(logDirectory, fileName);
            this.prevFilePath = Path.Combine(logDirectory, Path.GetFileNameWithoutExtension(fileName) + "-prev.log");
            this.stop = false;
            this.watcher = new Thread(ReadLogFile) { IsBackground = true };
            this.watcher.Start();
            this.parser = new Thread(ParseLines) { IsBackground = true };
            this.parser.Start();
        }

        public async Task Stop() {
            this.stop = true;
            while (this.running || this.watcher == null || this.watcher.ThreadState == ThreadState.Unstarted) {
                await Task.Delay(50);
            }
            this.lines = new List<LogLine>();
            await Task.Factory.StartNew(() => this.watcher?.Join());
            await Task.Factory.StartNew(() => this.parser?.Join());
        }

        private void ReadLogFile() {
            this.running = true;
            List<LogLine> tempLines = new List<LogLine>();
            DateTime lastDate = DateTime.MinValue;
            bool completed = false;
            string currentFilePath = prevFilePath;
            long offset = 0;
            while (!this.stop) {
                try {
                    if (File.Exists(currentFilePath)) {
                        using (FileStream fs = new FileStream(currentFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                            tempLines.Clear();

                            if (fs.Length > offset) {
                                fs.Seek(offset, SeekOrigin.Begin);

                                LineReader sr = new LineReader(fs);
                                string line;
                                DateTime currentDate = lastDate;
                                while ((line = sr.ReadLine()) != null) {
                                    LogLine logLine = new LogLine(line, sr.Position);

                                    if (logLine.IsValid) {
                                        int index;
                                        if ((index = line.IndexOf("[GlobalGameStateClient].PreStart called at ")) > 0) {
                                            currentDate = DateTime.SpecifyKind(DateTime.Parse(line.Substring(index + 43, 19)), DateTimeKind.Utc);
                                            this.OnNewLogFileDate?.Invoke(currentDate);
                                        }

                                        if (currentDate != DateTime.MinValue) {
                                            if (currentDate.TimeOfDay.TotalSeconds - logLine.Time.TotalSeconds > 60000) {
                                                currentDate = currentDate.AddDays(1);
                                            }
                                            currentDate = currentDate.AddSeconds(logLine.Time.TotalSeconds - currentDate.TimeOfDay.TotalSeconds);
                                            logLine.Date = currentDate;
                                        }

                                        if (line.IndexOf(" == [CompletedEpisodeDto] ==") > 0) {
                                            StringBuilder sb = new StringBuilder(line);
                                            sb.AppendLine();
                                            while ((line = sr.ReadLine()) != null) {
                                                LogLine temp = new LogLine(line, fs.Position);
                                                if (temp.IsValid) {
                                                    logLine.Line = sb.ToString();
                                                    logLine.Offset = sr.Position;
                                                    tempLines.Add(logLine);
                                                    tempLines.Add(temp);
                                                    break;
                                                } else if (!string.IsNullOrEmpty(line)) {
                                                    sb.AppendLine(line);
                                                }
                                            }
                                        } else {
                                            tempLines.Add(logLine);
                                        }
                                    } else if (logLine.Line.IndexOf("Client address: ", StringComparison.OrdinalIgnoreCase) > 0) {
                                        tempLines.Add(logLine);
                                    }
                                }
                            } else if (offset > fs.Length) {
                                offset = 0;
                            }
                        }
                    }

                    if (tempLines.Count > 0) {
                        List<RoundInfo> round = new List<RoundInfo>();
                        LogRound logRound = new LogRound();
                        List<LogLine> currentLines = new List<LogLine>();

                        for (int i = 0; i < tempLines.Count; i++) {
                            LogLine line = tempLines[i];
                            currentLines.Add(line);
                            if (this.ParseLine(line, round, logRound)) {
                                lastDate = line.Date;
                                offset = line.Offset;
                                lock (this.lines) {
                                    this.lines.AddRange(currentLines);
                                    currentLines.Clear();
                                }
                            } else if (line.Line.IndexOf("[StateMatchmaking] Begin", StringComparison.OrdinalIgnoreCase) > 0 ||
                                  line.Line.IndexOf("[GameStateMachine] Replacing FGClient.StateMainMenu with FGClient.StatePrivateLobby", StringComparison.OrdinalIgnoreCase) > 0) {
                                offset = i > 0 ? tempLines[i - 1].Offset : offset;
                                lastDate = line.Date;
                            }
                        }

                        this.OnParsedLogLinesCurrent?.Invoke(round);
                    }

                    if (!completed) {
                        completed = true;
                        offset = 0;
                        currentFilePath = filePath;
                    }
                } catch (Exception ex) {
                    this.OnError?.Invoke(ex.ToString());
                }
                Thread.Sleep(UpdateDelay);
            }
            this.running = false;
        }

        private void ParseLines() {
            List<RoundInfo> round = new List<RoundInfo>();
            List<RoundInfo> allStats = new List<RoundInfo>();
            LogRound logRound = new LogRound();

            while (!this.stop) {
                try {
                    lock (this.lines) {
                        for (int i = 0; i < this.lines.Count; i++) {
                            LogLine line = this.lines[i];
                            if (this.ParseLine(line, round, logRound)) {
                                if (logRound.Info != null && !Stats.InShow && Stats.AbruptShowEnd) {
                                    Stats.AbruptShowEnd = false;
                                    DateTime showStart = DateTime.MinValue;
                                    DateTime showEnd = logRound.Info.End;
                                    for (int j = 0; j < round.Count; j++) {
                                        if (j == 0) {
                                            showStart = round[j].Start;
                                        }
                                        round[j].ShowStart = showStart;
                                        round[j].PrivateLobby = true;
                                        round[j].Playing = false;
                                        if (j < (round.Count - 1)) {
                                            round[j].Qualified = true;
                                            if (round[j].Position == 0) {
                                                round[j].Position = round[j + 1].Players;
                                            }
                                        } else if (round[j].Position == 0) {
                                            round[j].Position = round[j].Players;
                                        }
                                        round[j].VerifyName();
                                        round[j].ShowEnd = showEnd;
                                    }
                                }
                                allStats.AddRange(round);
                            }
                        }

                        if (allStats.Count > 0) {
                            this.OnParsedLogLines?.Invoke(allStats);
                            allStats.Clear();
                        }

                        this.lines.Clear();
                    }
                } catch (Exception ex) {
                    this.OnError?.Invoke(ex.ToString());
                }
                Thread.Sleep(UpdateDelay);
            }
        }

        private readonly Dictionary<string, string> _sceneNameReplacer = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "FallGuy_FollowTheLeader_UNPACKED", "FallGuy_FollowTheLeader" } };

        private bool GetIsRealLastRound(string roundName) {
            return (roundName.IndexOf("ound_jinxed", StringComparison.OrdinalIgnoreCase) > 0
                    && roundName.IndexOf("_non_final", StringComparison.OrdinalIgnoreCase) == -1)

                    || (roundName.IndexOf("ound_fall_ball", StringComparison.OrdinalIgnoreCase) > 0
                        && roundName.IndexOf("_non_final", StringComparison.OrdinalIgnoreCase) == -1
                        && roundName.IndexOf("_cup_only", StringComparison.OrdinalIgnoreCase) == -1)

                    || (roundName.IndexOf("ound_1v1_volleyfall", StringComparison.OrdinalIgnoreCase) > 0
                        && roundName.IndexOf("_final", StringComparison.OrdinalIgnoreCase) > 0)

                    || (roundName.IndexOf("ound_pixelperfect", StringComparison.OrdinalIgnoreCase) > 0
                        && roundName.Substring(roundName.Length - 6).ToLower() == "_final")

                    || roundName.EndsWith("_timeattack_final", StringComparison.OrdinalIgnoreCase)

                    || roundName.EndsWith("_xtreme_party_final", StringComparison.OrdinalIgnoreCase);
        }

        private bool GetIsModeException(string sceneName) {
            return sceneName.IndexOf("ound_lava_event_only_slime_climb", StringComparison.OrdinalIgnoreCase) > 0
                   || sceneName.IndexOf("ound_kraken_attack_only_finals", StringComparison.OrdinalIgnoreCase) > 0
                   || sceneName.IndexOf("ound_blastball_only_finals", StringComparison.OrdinalIgnoreCase) > 0
                   || sceneName.IndexOf("ound_floor_fall_only_finals", StringComparison.OrdinalIgnoreCase) > 0
                   || sceneName.IndexOf("ound_hexsnake_only_finals", StringComparison.OrdinalIgnoreCase) > 0
                   || sceneName.IndexOf("ound_jump_showdown_only_finals", StringComparison.OrdinalIgnoreCase) > 0
                   || sceneName.IndexOf("ound_hexaring_only_finals", StringComparison.OrdinalIgnoreCase) > 0
                   || sceneName.IndexOf("ound_tunnel_final_only_finals", StringComparison.OrdinalIgnoreCase) > 0
                   || sceneName.IndexOf("ound_floor_fall_event_only", StringComparison.OrdinalIgnoreCase) > 0
                   || sceneName.IndexOf("ound_floor_fall_event_only_low_grav", StringComparison.OrdinalIgnoreCase) > 0
                   || sceneName.IndexOf("ound_floor_fall_event_walnut", StringComparison.OrdinalIgnoreCase) > 0
                   || sceneName.IndexOf("ound_hexaring_event_walnut", StringComparison.OrdinalIgnoreCase) > 0
                   || sceneName.IndexOf("ound_hexsnake_event_walnut", StringComparison.OrdinalIgnoreCase) > 0
                   || sceneName.IndexOf("ound_blastball_arenasurvival_blast_ball_trials", StringComparison.OrdinalIgnoreCase) > 0
                   || sceneName.IndexOf("ound_robotrampage_arena_2_ss2_show1", StringComparison.OrdinalIgnoreCase) > 0;
        }

        private bool GetIsFinalException(string roundName) {
            return ((roundName.IndexOf("ound_lava_event_only_slime_climb", StringComparison.OrdinalIgnoreCase) > 0
                     || roundName.IndexOf("ound_kraken_attack_only_finals", StringComparison.OrdinalIgnoreCase) > 0
                     || roundName.IndexOf("ound_blastball_only_finals", StringComparison.OrdinalIgnoreCase) > 0
                     || roundName.IndexOf("ound_floor_fall_only_finals", StringComparison.OrdinalIgnoreCase) > 0
                     || roundName.IndexOf("ound_hexsnake_only_finals", StringComparison.OrdinalIgnoreCase) > 0
                     || roundName.IndexOf("ound_jump_showdown_only_finals", StringComparison.OrdinalIgnoreCase) > 0
                     || roundName.IndexOf("ound_hexaring_only_finals", StringComparison.OrdinalIgnoreCase) > 0
                     || roundName.IndexOf("ound_tunnel_final_only_finals", StringComparison.OrdinalIgnoreCase) > 0
                     || roundName.IndexOf("ound_floor_fall_event_only", StringComparison.OrdinalIgnoreCase) > 0
                     || roundName.IndexOf("ound_floor_fall_event_only_low_grav", StringComparison.OrdinalIgnoreCase) > 0
                     || roundName.IndexOf("ound_floor_fall_event_walnut", StringComparison.OrdinalIgnoreCase) > 0
                     || roundName.IndexOf("ound_hexaring_event_walnut", StringComparison.OrdinalIgnoreCase) > 0
                     || roundName.IndexOf("ound_hexsnake_event_walnut", StringComparison.OrdinalIgnoreCase) > 0)
                        && roundName.Substring(roundName.Length - 6).ToLower() == "_final")

                     || (roundName.IndexOf("ound_blastball_arenasurvival_blast_ball_trials", StringComparison.OrdinalIgnoreCase) > 0
                         && roundName.Substring(roundName.Length - 3).ToLower() == "_fn")

                     || (roundName.IndexOf("ound_robotrampage_arena_2_ss2_show1", StringComparison.OrdinalIgnoreCase) > 0
                         && roundName.Substring(roundName.Length - 3) == "_03");
        }

        private bool GetIsTeamException(string roundName) {
            return roundName.IndexOf("ound_1v1_volleyfall", StringComparison.OrdinalIgnoreCase) > 0
                   && (roundName.IndexOf("_duos", StringComparison.OrdinalIgnoreCase) > 0
                   || roundName.IndexOf("_squads", StringComparison.OrdinalIgnoreCase) > 0);
        }

        private bool ParseLine(LogLine line, List<RoundInfo> round, LogRound logRound) {
            int index;
            if (line.Line.IndexOf("[StateMainMenu] Loading scene MainMenu", StringComparison.OrdinalIgnoreCase) > 0) {
                LogRound.IsPlaying = false;
                LogRound.IsLastPlayed = false;
                if (logRound.Info != null) {
                    if (logRound.Info.End == DateTime.MinValue) {
                        logRound.Info.End = line.Date;
                    }
                    logRound.Info.Playing = false;
                    if (!LogRound.IsSpectating) {
                        Stats.AbruptShowEnd = true;
                        logRound.FindingPosition = false;
                        Stats.InShow = false;
                        LogRound.GetInfo = 0;
                        return true;
                    }
                }
                logRound.FindingPosition = false;
                Stats.InShow = false;
                LogRound.GetInfo = 0;
                return false;
            } else if (line.Line.IndexOf("[GameSession] Changing state from Playing to GameOver", StringComparison.OrdinalIgnoreCase) > 0
                  || line.Line.IndexOf("Changing local player state to: SpectatingEliminated", StringComparison.OrdinalIgnoreCase) > 0
                  || line.Line.IndexOf("[GlobalGameStateClient] SwitchToDisconnectingState", StringComparison.OrdinalIgnoreCase) > 0
                  || line.Line.IndexOf("[GameStateMachine] Replacing FGClient.StatePrivateLobby with FGClient.StateMainMenu", StringComparison.OrdinalIgnoreCase) > 0) {
                bool isRoundPlaying = false;
                bool isStillInShow = true;
                if (line.Line.IndexOf("Changing local player state to: SpectatingEliminated", StringComparison.OrdinalIgnoreCase) > 0) { LogRound.IsSpectating = true; isRoundPlaying = true; }
                if (line.Line.IndexOf("[GlobalGameStateClient] SwitchToDisconnectingState", StringComparison.OrdinalIgnoreCase) > 0) { isStillInShow = false; }
                if (line.Line.IndexOf("[GameStateMachine] Replacing FGClient.StatePrivateLobby with FGClient.StateMainMenu", StringComparison.OrdinalIgnoreCase) > 0) { logRound.PrivateLobby = false; isStillInShow = false; }
                LogRound.IsPlaying = isRoundPlaying;
                if (logRound.Info != null) {
                    if (logRound.Info.End == DateTime.MinValue) {
                        logRound.Info.End = line.Date;
                    }
                    logRound.Info.Playing = isRoundPlaying;
                } else if (!LogRound.IsInfoEmpty) {
                    LogRound.IsInfoEmpty = true;
                    LogRound.End = DateTime.UtcNow;
                }
                Stats.InShow = isStillInShow;
                if (!Stats.InShow) {
                    LogRound.IsLastPlayed = false;
                    logRound.FindingPosition = false;
                    LogRound.GetInfo = 0;
                    if (logRound.Info != null && !LogRound.IsSpectating) {
                        Stats.AbruptShowEnd = true;
                        return true;
                    }
                } else {
                    LogRound.GetInfo = 2;
                }
                return false;
            } else if (logRound.Info != null && LogRound.GetInfo == 5 && logRound.FindingPosition && (index = line.Line.IndexOf("[ClientGameSession] NumPlayersAchievingObjective=")) > 0) {
                int position = int.Parse(line.Line.Substring(index + 49));
                if (position > 0) {
                    logRound.FindingPosition = false;
                    logRound.Info.Position = position;
                }
                return false;
            } else if (LogRound.GetInfo == 1 && (index = line.Line.IndexOf("[HandleSuccessfulLogin] Selected show is", StringComparison.OrdinalIgnoreCase)) > 0) {
                this.selectedShowId = line.Line.Substring(line.Line.Length - (line.Line.Length - index - 41));
                if (this.StatsForm.CurrentSettings.AutoChangeProfile) {
                    this.StatsForm.SetLinkedProfile(this.selectedShowId, logRound.PrivateLobby);
                }
                LogRound.GetInfo = 2;
                return false;
            } else if (LogRound.GetInfo >= 2 && line.Line.IndexOf("Client address: ", StringComparison.OrdinalIgnoreCase) > 0) {
                index = line.Line.IndexOf("RTT: ");
                if (index > 0) {
                    int msIndex = line.Line.IndexOf("ms", index);
                    Stats.ServerPing = int.Parse(line.Line.Substring(index + 5, msIndex - index - 5));
                }
                return false;
            } else if ((LogRound.GetInfo == 2 || LogRound.IsSpectating) && (index = line.Line.IndexOf("[StateGameLoading] Loading game level scene", StringComparison.OrdinalIgnoreCase)) > 0) {
                LogRound.IsPlaying = false;
                LogRound.IsLastPlayed = false;
                LogRound.IsInfoEmpty = false;
                logRound.Info = new RoundInfo { ShowNameId = this.selectedShowId };
                int index2 = line.Line.IndexOf(' ', index + 44);
                if (index2 < 0) { index2 = line.Line.Length; }

                logRound.Info.SceneName = line.Line.Substring(index + 44, index2 - index - 44);
                if (_sceneNameReplacer.TryGetValue(logRound.Info.SceneName, out string newName)) {
                    logRound.Info.SceneName = newName;
                }
                logRound.FindingPosition = false;
                round.Add(logRound.Info);
                LogRound.GetInfo = 3;
                return false;
            } else if (logRound.Info != null && LogRound.GetInfo == 3 && (index = line.Line.IndexOf("[StateGameLoading] Finished loading game level", StringComparison.OrdinalIgnoreCase)) > 0) {
                int index2 = line.Line.IndexOf(". ", index + 62);
                if (index2 < 0) { index2 = line.Line.Length; }
                logRound.Info.Name = line.Line.Substring(index + 62, index2 - index - 62);

                bool isRealLastRound = GetIsRealLastRound(logRound.Info.Name);
                bool isModeException = GetIsModeException(logRound.Info.Name);
                bool isFinalException = GetIsFinalException(logRound.Info.Name);
                bool isTeamException = GetIsTeamException(logRound.Info.Name);

                logRound.Info.Round = !LogRound.IsSpectating ? round.Count : LogRound.SavedCount + round.Count;
                logRound.Info.Start = line.Date;
                logRound.Info.InParty = logRound.CurrentlyInParty;
                logRound.Info.PrivateLobby = logRound.PrivateLobby;
                logRound.Info.GameDuration = logRound.Duration;

                if (isRealLastRound) {
                    logRound.Info.IsFinal = true;
                } else if (isModeException) {
                    logRound.Info.IsFinal = isFinalException;
                } else {
                    logRound.Info.IsFinal = logRound.IsFinal || (!logRound.HasIsFinal && LevelStats.SceneToRound.TryGetValue(logRound.Info.SceneName, out string roundName) && LevelStats.ALL.TryGetValue(roundName, out LevelStats stats) && stats.IsFinal);
                }
                logRound.Info.IsTeam = isTeamException;
                LogRound.GetInfo = 4;
                return false;
            } else if (line.Line.IndexOf("[StateMatchmaking] Begin", StringComparison.OrdinalIgnoreCase) > 0 ||
              line.Line.IndexOf("[GameStateMachine] Replacing FGClient.StateMainMenu with FGClient.StatePrivateLobby", StringComparison.OrdinalIgnoreCase) > 0) {
                LogRound.IsPlaying = false;
                LogRound.IsSpectating = false;
                LogRound.IsInfoEmpty = true;
                logRound.PrivateLobby = line.Line.IndexOf("StatePrivateLobby") > 0;
                logRound.CurrentlyInParty = !logRound.PrivateLobby && (line.Line.IndexOf("solo", StringComparison.OrdinalIgnoreCase) == -1);
                if (logRound.Info != null) {
                    if (logRound.Info.End == DateTime.MinValue) {
                        logRound.Info.End = line.Date;
                    }
                    logRound.Info.Playing = false;
                }
                logRound.FindingPosition = false;
                Stats.InShow = true;
                round.Clear();
                logRound.Info = null;
                LogRound.GetInfo = 1;
                return false;
            } else if ((index = line.Line.IndexOf("NetworkGameOptions: durationInSeconds=", StringComparison.OrdinalIgnoreCase)) > 0) { // legacy code // It seems to have been deleted from the log file now.
                int nextIndex = line.Line.IndexOf(" ", index + 38);
                logRound.Duration = int.Parse(line.Line.Substring(index + 38, nextIndex - index - 38));
                index = line.Line.IndexOf("isFinalRound=", StringComparison.OrdinalIgnoreCase);
                logRound.HasIsFinal = index > 0;
                index = line.Line.IndexOf("isFinalRound=True", StringComparison.OrdinalIgnoreCase);
                logRound.IsFinal = index > 0;
                return false;
            } else if (logRound.Info != null && LogRound.GetInfo == 4) {
                if (line.Line.IndexOf("[GameSession] Changing state from Countdown to Playing", StringComparison.OrdinalIgnoreCase) > 0) {
                    LogRound.IsPlaying = true;
                    logRound.Info.Start = line.Date;
                    logRound.Info.Playing = true;
                    LogRound.GetInfo = 5;
                    return false;
                } else if (line.Line.IndexOf("[ClientGameManager] Finalising spawn", StringComparison.OrdinalIgnoreCase) > 0 || line.Line.IndexOf("[ClientGameManager] Added player ", StringComparison.OrdinalIgnoreCase) > 0) {
                    logRound.Info.Players++;
                    return false;
                } else if (line.Line.IndexOf("[CameraDirector] Adding Spectator target", StringComparison.OrdinalIgnoreCase) > 0) {
                    if (line.Line.IndexOf("ps4", StringComparison.OrdinalIgnoreCase) > 0) {
                        logRound.Info.PlayersPs4++;
                    } else if (line.Line.IndexOf("ps5", StringComparison.OrdinalIgnoreCase) > 0) {
                        logRound.Info.PlayersPs5++;
                    } else if (line.Line.IndexOf("xb1", StringComparison.OrdinalIgnoreCase) > 0) {
                        logRound.Info.PlayersXb1++;
                    } else if (line.Line.IndexOf("xsx", StringComparison.OrdinalIgnoreCase) > 0) {
                        logRound.Info.PlayersXsx++;
                    } else if (line.Line.IndexOf("switch", StringComparison.OrdinalIgnoreCase) > 0) {
                        logRound.Info.PlayersSw++;
                    } else if (line.Line.IndexOf("win", StringComparison.OrdinalIgnoreCase) > 0) {
                        logRound.Info.PlayersPc++;
                    } else if (line.Line.IndexOf("bots", StringComparison.OrdinalIgnoreCase) > 0) {
                        logRound.Info.PlayersBots++;
                    } else {
                        logRound.Info.PlayersEtc++;
                    }
                    return false;
                } else if ((index = line.Line.IndexOf("[ClientGameManager] Handling bootstrap for local player FallGuy [", StringComparison.OrdinalIgnoreCase)) > 0) {
                    int prevIndex = line.Line.IndexOf(']', index + 65);
                    logRound.CurrentPlayerID = line.Line.Substring(index + 65, prevIndex - index - 65);
                    return false;
                }
                return false;
            } else if (logRound.Info != null && LogRound.GetInfo == 5 && line.Line.IndexOf($"[ClientGameManager] Handling unspawn for player FallGuy [{logRound.CurrentPlayerID}]", StringComparison.OrdinalIgnoreCase) > 0) {
                logRound.Info.Finish = logRound.Info.End == DateTime.MinValue ? line.Date : logRound.Info.End;
                logRound.FindingPosition = true;
                return false;
            } else if (line.Line.IndexOf(" == [CompletedEpisodeDto] ==", StringComparison.OrdinalIgnoreCase) > 0) {
                if (logRound.Info == null) { return false; }

                LogRound.GetInfo = 0;
                LogRound.IsSpectating = true;
                LogRound.IsLastPlayed = true;

                RoundInfo roundInfo = null;
                StringReader sr = new StringReader(line.Line);
                string detail;
                bool foundRound = false;
                int maxRound = 0;
                DateTime showStart = DateTime.MinValue;
                while ((detail = sr.ReadLine()) != null) {
                    if (detail.IndexOf("[Round ", StringComparison.OrdinalIgnoreCase) == 0) {
                        foundRound = true;
                        int roundNum = (int)detail[7] - 0x30 + 1;
                        string roundName = detail.Substring(11, detail.Length - 12);

                        if (roundNum - 1 < round.Count) {
                            if (roundNum > maxRound) {
                                maxRound = roundNum;
                            }

                            roundInfo = round[roundNum - 1];
                            if (string.IsNullOrEmpty(roundInfo.Name) || !roundInfo.Name.Equals(roundName, StringComparison.OrdinalIgnoreCase)) {
                                return false;
                            }
                            roundInfo.VerifyName();

                            if (roundNum == 1) {
                                showStart = roundInfo.Start;
                            }
                            roundInfo.ShowStart = showStart;
                            roundInfo.Playing = false;
                        } else {
                            return false;
                        }

                        if (roundInfo.End == DateTime.MinValue) {
                            roundInfo.End = line.Date;
                        }
                        if (roundInfo.Start == DateTime.MinValue) {
                            roundInfo.Start = roundInfo.End;
                        }
                        if (!roundInfo.Finish.HasValue) {
                            roundInfo.Finish = roundInfo.End;
                        }
                    } else if (foundRound) {
                        if (detail.IndexOf("> Qualified: ", StringComparison.OrdinalIgnoreCase) == 0) {
                            char qualified = detail[13];
                            roundInfo.Qualified = qualified == 'T';
                            roundInfo.Finish = roundInfo.Qualified ? roundInfo.Finish : null;
                        } else if (detail.IndexOf("> Position: ", StringComparison.OrdinalIgnoreCase) == 0) {
                            roundInfo.Position = int.Parse(detail.Substring(12));
                        } else if (detail.IndexOf("> Team Score: ", StringComparison.OrdinalIgnoreCase) == 0) {
                            roundInfo.Score = int.Parse(detail.Substring(14));
                        } else if (detail.IndexOf("> Kudos: ", StringComparison.OrdinalIgnoreCase) == 0) {
                            roundInfo.Kudos += int.Parse(detail.Substring(9));
                        } else if (detail.IndexOf("> Bonus Tier: ", StringComparison.OrdinalIgnoreCase) == 0 && detail.Length == 15) {
                            char tier = detail[14];
                            roundInfo.Tier = (int)tier - 0x30 + 1;
                        } else if (detail.IndexOf("> Bonus Kudos: ", StringComparison.OrdinalIgnoreCase) == 0) {
                            roundInfo.Kudos += int.Parse(detail.Substring(15));
                        }
                    }
                }

                if (round.Count > maxRound) {
                    return false;
                }

                DateTime showEnd = logRound.Info.End;
                for (int i = 0; i < round.Count; i++) {
                    round[i].ShowEnd = showEnd;
                }

                if (logRound.Info.Qualified) {
                    logRound.Info.Position = 1;
                    logRound.Info.Crown = true;
                }

                LogRound.SavedCount = logRound.Info.Round;
                return true;
            }
            return false;
        }
    }
}