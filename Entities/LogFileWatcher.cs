﻿using System;
using System.Collections.Generic;
#if Debug
using System.Diagnostics;
#endif
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
            bool isValidSemiColon = line.IndexOf(":") == 2 && line.IndexOf(":", 3) == 5 && line.IndexOf(":", 6) == 12;
            bool isValidDot = line.IndexOf(".") == 2 && line.IndexOf(".", 3) == 5 && line.IndexOf(":", 6) == 12;
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
        public bool GetServerPing;
        public bool CountingPlayers;
        public bool GetCurrentPlayerID;
        public bool FindingPosition;
        public bool IsFinal;
        public bool HasIsFinal;
        public string CurrentPlayerID;
        public int Duration;

        public static List<string> succeededPlayerIds = new List<string>();

        public static int SavedRoundCount { get; set; }
        public static int NumPlayersSucceeded { get; set; }
        public static bool IsLastRoundRunning { get; set; }
        public static bool IsLastPlayedRoundStillPlaying { get; set; }
        public static bool IsShowCompletedOrEnded { get; set; }

        public static DateTime LastRoundStart { get; set; } = DateTime.MinValue;
        public static DateTime? LastPlayedRoundStart { get; set; } = null;
        public static DateTime? LastPlayedRoundEnd { get; set; } = null;

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

        private bool autoChangeProfile;
        private string selectedShowId;
        private string sessionId;

        public event Action<List<RoundInfo>> OnParsedLogLines;
        public event Action<List<RoundInfo>> OnParsedLogLinesCurrent;
        public event Action<DateTime> OnNewLogFileDate;
        public event Action<string> OnError;

        public void SetAutoChangeProfile(bool option) {
            this.autoChangeProfile = option;
        }

        public void Start(string logDirectory, string fileName) {
            if (this.running) { return; }

#if Debug
            Debug.WriteLine("LogFileWatcher is running!");
#endif
            this.filePath = Path.Combine(logDirectory, fileName);
            this.prevFilePath = Path.Combine(logDirectory, Path.GetFileNameWithoutExtension(fileName) + "-prev.log");
            this.stop = false;
            this.watcher = new Thread(this.ReadLogFile) { IsBackground = true };
            this.watcher.Start();
            this.parser = new Thread(this.ParseLines) { IsBackground = true };
            this.parser.Start();
        }

        public async Task Stop() {
            this.stop = true;
#if Debug
            while (this.running || this.watcher == null || this.watcher.ThreadState == System.Threading.ThreadState.Unstarted) {
#else
            while (this.running || this.watcher == null || this.watcher.ThreadState == ThreadState.Unstarted) {
#endif
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
                                LogRound.SavedRoundCount = 0;
                                //LogRound.LastPlayedRoundStart = null;
                                //LogRound.LastPlayedRoundEnd = null;
                                lastDate = line.Date;
                                offset = line.Offset;
                                lock (this.lines) {
                                    this.lines.AddRange(currentLines);
                                    currentLines.Clear();
                                }
                            } else if (line.Line.IndexOf("[StateMatchmaking] Begin", StringComparison.OrdinalIgnoreCase) > 0 || line.Line.IndexOf("[GameStateMachine] Replacing FGClient.StatePrivateLobby with FGClient.StateConnectToGame", StringComparison.OrdinalIgnoreCase) > 0
                                       || line.Line.IndexOf("[StateMainMenu] Loading scene MainMenu", StringComparison.OrdinalIgnoreCase) > 0
                                       || line.Line.IndexOf("[GameStateMachine] Replacing FGClient.StatePrivateLobby with FGClient.StateMainMenu", StringComparison.OrdinalIgnoreCase) > 0
                                       || line.Line.IndexOf("[GameStateMachine] Replacing FGClient.StateReloadingToMainMenu with FGClient.StateMainMenu", StringComparison.OrdinalIgnoreCase) > 0
                                       || line.Line.IndexOf("[GlobalGameStateClient] SwitchToDisconnectingState", StringComparison.OrdinalIgnoreCase) > 0
                                       || line.Line.IndexOf("The remote sent a disconnect request", StringComparison.OrdinalIgnoreCase) > 0
                                       || line.Line.IndexOf("[ClientGlobalGameState] Client has been disconnected", StringComparison.OrdinalIgnoreCase) > 0) {
                                offset = i > 0 ? tempLines[i - 1].Offset : offset;
                                lastDate = line.Date;
                            } else if (line.Line.IndexOf("[HandleSuccessfulLogin] Selected show is", StringComparison.OrdinalIgnoreCase) > 0) {
                                if (this.autoChangeProfile && !LogRound.IsShowCompletedOrEnded) {
                                    this.StatsForm.SetLinkedProfile(this.selectedShowId, logRound.PrivateLobby);
                                }
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
                    this.OnError?.Invoke(ex.Message);
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
                    this.OnError?.Invoke(ex.Message);
                }
                Thread.Sleep(UpdateDelay);
            }
        }

        private readonly Dictionary<string, string> _sceneNameReplacer = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "FallGuy_FollowTheLeader_UNPACKED", "FallGuy_FollowTheLeader" } };

        private bool GetIsRealLastRound(string roundName) {
            return (roundName.IndexOf("ound_jinxed", StringComparison.OrdinalIgnoreCase) != -1
                    && roundName.IndexOf("_non_final", StringComparison.OrdinalIgnoreCase) == -1)

                    || (roundName.IndexOf("ound_fall_ball", StringComparison.OrdinalIgnoreCase) != -1
                        && roundName.IndexOf("_non_final", StringComparison.OrdinalIgnoreCase) == -1
                        && roundName.IndexOf("_cup_only", StringComparison.OrdinalIgnoreCase) == -1)

                    || (roundName.IndexOf("ound_1v1_volleyfall", StringComparison.OrdinalIgnoreCase) != -1
                        && roundName.IndexOf("_final", StringComparison.OrdinalIgnoreCase) != -1)

                    || (roundName.IndexOf("ound_pixelperfect", StringComparison.OrdinalIgnoreCase) != -1
                        && roundName.Substring(roundName.Length - 6).ToLower() == "_final")

                    || roundName.EndsWith("_timeattack_final", StringComparison.OrdinalIgnoreCase)

                    || roundName.EndsWith("_xtreme_party_final", StringComparison.OrdinalIgnoreCase);
        }

        private bool GetIsModeException(string sceneName) {
            return sceneName.IndexOf("ound_lava_event_only_slime_climb", StringComparison.OrdinalIgnoreCase) != -1
                   || sceneName.IndexOf("ound_kraken_attack_only_finals", StringComparison.OrdinalIgnoreCase) != -1
                   || sceneName.IndexOf("ound_blastball_only_finals", StringComparison.OrdinalIgnoreCase) != -1
                   || sceneName.IndexOf("ound_floor_fall_only_finals", StringComparison.OrdinalIgnoreCase) != -1
                   || sceneName.IndexOf("ound_hexsnake_only_finals", StringComparison.OrdinalIgnoreCase) != -1
                   || sceneName.IndexOf("ound_jump_showdown_only_finals", StringComparison.OrdinalIgnoreCase) != -1
                   || sceneName.IndexOf("ound_hexaring_only_finals", StringComparison.OrdinalIgnoreCase) != -1
                   || sceneName.IndexOf("ound_tunnel_final_only_finals", StringComparison.OrdinalIgnoreCase) != -1
                   || sceneName.IndexOf("ound_drumtop_event_only", StringComparison.OrdinalIgnoreCase) != -1
                   || sceneName.IndexOf("ound_floor_fall_event_only", StringComparison.OrdinalIgnoreCase) != -1
                   || sceneName.IndexOf("ound_floor_fall_event_only_low_grav", StringComparison.OrdinalIgnoreCase) != -1
                   || sceneName.IndexOf("ound_floor_fall_event_walnut", StringComparison.OrdinalIgnoreCase) != -1
                   || sceneName.IndexOf("ound_hexaring_event_walnut", StringComparison.OrdinalIgnoreCase) != -1
                   || sceneName.IndexOf("ound_hexsnake_event_walnut", StringComparison.OrdinalIgnoreCase) != -1
                   || sceneName.IndexOf("ound_blastball_arenasurvival_blast_ball_trials", StringComparison.OrdinalIgnoreCase) != -1
                   || sceneName.IndexOf("ound_robotrampage_arena_2_ss2_show1", StringComparison.OrdinalIgnoreCase) != -1;
        }

        private bool GetIsFinalException(string roundName) {
            return ((roundName.IndexOf("ound_lava_event_only_slime_climb", StringComparison.OrdinalIgnoreCase) != -1
                     || roundName.IndexOf("ound_kraken_attack_only_finals", StringComparison.OrdinalIgnoreCase) != -1
                     || roundName.IndexOf("ound_blastball_only_finals", StringComparison.OrdinalIgnoreCase) != -1
                     || roundName.IndexOf("ound_floor_fall_only_finals", StringComparison.OrdinalIgnoreCase) != -1
                     || roundName.IndexOf("ound_hexsnake_only_finals", StringComparison.OrdinalIgnoreCase) != -1
                     || roundName.IndexOf("ound_jump_showdown_only_finals", StringComparison.OrdinalIgnoreCase) != -1
                     || roundName.IndexOf("ound_hexaring_only_finals", StringComparison.OrdinalIgnoreCase) != -1
                     || roundName.IndexOf("ound_tunnel_final_only_finals", StringComparison.OrdinalIgnoreCase) != -1
                     || roundName.IndexOf("ound_drumtop_event_only", StringComparison.OrdinalIgnoreCase) != -1
                     || roundName.IndexOf("ound_floor_fall_event_only", StringComparison.OrdinalIgnoreCase) != -1
                     || roundName.IndexOf("ound_floor_fall_event_only_low_grav", StringComparison.OrdinalIgnoreCase) != -1
                     || roundName.IndexOf("ound_floor_fall_event_walnut", StringComparison.OrdinalIgnoreCase) != -1
                     || roundName.IndexOf("ound_hexaring_event_walnut", StringComparison.OrdinalIgnoreCase) != -1
                     || roundName.IndexOf("ound_hexsnake_event_walnut", StringComparison.OrdinalIgnoreCase) != -1)
                        && roundName.Substring(roundName.Length - 6).ToLower() == "_final")

                     || (roundName.IndexOf("ound_blastball_arenasurvival_blast_ball_trials", StringComparison.OrdinalIgnoreCase) != -1
                         && roundName.Substring(roundName.Length - 3).ToLower() == "_fn")

                     || (roundName.IndexOf("ound_robotrampage_arena_2_ss2_show1", StringComparison.OrdinalIgnoreCase) != -1
                         && roundName.Substring(roundName.Length - 3) == "_03");
        }

        private bool GetIsTeamException(string roundName) {
            return roundName.IndexOf("ound_1v1_volleyfall", StringComparison.OrdinalIgnoreCase) != -1
                   && (roundName.IndexOf("_duos", StringComparison.OrdinalIgnoreCase) != -1
                   || roundName.IndexOf("_squads", StringComparison.OrdinalIgnoreCase) != -1);
        }

        private bool ParseLine(LogLine line, List<RoundInfo> round, LogRound logRound) {
            int index;
            if (line.Line.IndexOf("[StateMatchmaking] Begin", StringComparison.OrdinalIgnoreCase) > 0 || line.Line.IndexOf("[GameStateMachine] Replacing FGClient.StatePrivateLobby with FGClient.StateConnectToGame", StringComparison.OrdinalIgnoreCase) > 0) {
                if (logRound.Info != null) {
                    if (logRound.Info.End == DateTime.MinValue) {
                        logRound.Info.End = line.Date;
                    }
                    logRound.Info.Playing = false;
                }
                logRound.Info = null;

                LogRound.IsShowCompletedOrEnded = false;

                logRound.PrivateLobby = line.Line.IndexOf("StatePrivateLobby", StringComparison.OrdinalIgnoreCase) > 0;
                logRound.CurrentlyInParty = !logRound.PrivateLobby && (line.Line.IndexOf("solo", StringComparison.OrdinalIgnoreCase) == -1);
                logRound.GetServerPing = true;
                logRound.CountingPlayers = false;
                logRound.GetCurrentPlayerID = false;
                logRound.FindingPosition = false;

                round.Clear();
            } else if (logRound.Info == null && !LogRound.IsShowCompletedOrEnded && (index = line.Line.IndexOf("[HandleSuccessfulLogin] Selected show is", StringComparison.OrdinalIgnoreCase)) > 0) {
                this.selectedShowId = line.Line.Substring(line.Line.Length - (line.Line.Length - index - 41));
            } else if (logRound.Info == null && !LogRound.IsShowCompletedOrEnded && (index = line.Line.IndexOf("[HandleSuccessfulLogin] Session: ", StringComparison.OrdinalIgnoreCase)) > 0) {
                //Store SessionID to prevent duplicates
                this.sessionId = line.Line.Substring(index + 33);
            } else if (logRound.GetServerPing && line.Line.IndexOf("Client address: ", StringComparison.OrdinalIgnoreCase) > 0) {
                index = line.Line.IndexOf("RTT: ");
                if (index > 0) {
                    logRound.GetServerPing = false;
                    int msIndex = line.Line.IndexOf("ms", index);
                    Stats.LastServerPing = int.Parse(line.Line.Substring(index + 5, msIndex - index - 5));
                }
            } else if ((index = line.Line.IndexOf("[StateGameLoading] Loading game level scene", StringComparison.OrdinalIgnoreCase)) > 0) {
                logRound.Info = new RoundInfo { ShowNameId = this.selectedShowId, SessionId = this.sessionId };
                int index2 = line.Line.IndexOf(" ", index + 44);
                if (index2 < 0) { index2 = line.Line.Length; }

                logRound.Info.SceneName = line.Line.Substring(index + 44, index2 - index - 44);
                if (_sceneNameReplacer.TryGetValue(logRound.Info.SceneName, out string newName)) {
                    logRound.Info.SceneName = newName;
                }

                logRound.FindingPosition = false;

                round.Add(logRound.Info);
            } else if (logRound.Info != null && (index = line.Line.IndexOf("[StateGameLoading] Finished loading game level", StringComparison.OrdinalIgnoreCase)) > 0) {
                if (line.Date > LogRound.LastRoundStart) {
                    LogRound.LastRoundStart = line.Date;
                    LogRound.succeededPlayerIds.Clear();
                    LogRound.NumPlayersSucceeded = 0;
                    LogRound.IsLastRoundRunning = true;
                    LogRound.IsLastPlayedRoundStillPlaying = false;
                    LogRound.LastPlayedRoundStart = null;
                    LogRound.LastPlayedRoundEnd = null;
                    Stats.InShow = true;
                }

                int index2 = line.Line.IndexOf(". ", index + 62);
                if (index2 < 0) { index2 = line.Line.Length; }
                logRound.Info.Name = line.Line.Substring(index + 62, index2 - index - 62);

                bool isRealLastRound = GetIsRealLastRound(logRound.Info.Name);
                bool isModeException = GetIsModeException(logRound.Info.Name);
                bool isFinalException = GetIsFinalException(logRound.Info.Name);
                bool isTeamException = GetIsTeamException(logRound.Info.Name);

                logRound.Info.Round = !LogRound.IsShowCompletedOrEnded ? round.Count : LogRound.SavedRoundCount + round.Count;
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

                logRound.CountingPlayers = true;
                logRound.GetCurrentPlayerID = true;
            } else if (logRound.Info != null && (index = line.Line.IndexOf("NetworkGameOptions: durationInSeconds=", StringComparison.OrdinalIgnoreCase)) > 0) { //Legacy code (no more present in logs)
                int nextIndex = line.Line.IndexOf(" ", index + 38);
                logRound.Duration = int.Parse(line.Line.Substring(index + 38, nextIndex - index - 38));
                index = line.Line.IndexOf("isFinalRound=", StringComparison.OrdinalIgnoreCase);
                logRound.HasIsFinal = index > 0;
                index = line.Line.IndexOf("isFinalRound=True", StringComparison.OrdinalIgnoreCase);
                logRound.IsFinal = index > 0;
            } else if (logRound.Info != null && logRound.CountingPlayers && line.Line.IndexOf("[ClientGameManager] Finalising spawn", StringComparison.OrdinalIgnoreCase) > 0 || line.Line.IndexOf("[ClientGameManager] Added player ", StringComparison.OrdinalIgnoreCase) > 0) {
                logRound.Info.Players++;
            } else if (logRound.Info != null && logRound.CountingPlayers && line.Line.IndexOf("[CameraDirector] Adding Spectator target", StringComparison.OrdinalIgnoreCase) > 0) {
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
            } else if (logRound.Info != null && logRound.GetCurrentPlayerID && line.Line.IndexOf("[ClientGameManager] Handling bootstrap for local player FallGuy", StringComparison.OrdinalIgnoreCase) > 0 && (index = line.Line.IndexOf("playerID = ", StringComparison.OrdinalIgnoreCase)) > 0) {
                logRound.GetCurrentPlayerID = false;
                int prevIndex = line.Line.IndexOf(",", index + 11);
                logRound.CurrentPlayerID = line.Line.Substring(index + 11, prevIndex - index - 11);
            } else if (logRound.Info != null && line.Line.IndexOf("[GameSession] Changing state from Countdown to Playing", StringComparison.OrdinalIgnoreCase) > 0) {
                logRound.Info.Start = line.Date;
                logRound.Info.Playing = true;
                logRound.CountingPlayers = false;
                logRound.GetCurrentPlayerID = false;
            } else if (logRound.Info != null && line.Line.IndexOf($"HandleServerPlayerProgress PlayerId={logRound.CurrentPlayerID} is succeeded=", StringComparison.OrdinalIgnoreCase) > 0) {
                index = line.Line.IndexOf("succeeded=True", StringComparison.OrdinalIgnoreCase);
                if (index > 0) {
                    logRound.Info.Finish = logRound.Info.End == DateTime.MinValue ? line.Date : logRound.Info.End;
                    if (line.Date > LogRound.LastRoundStart && !LogRound.succeededPlayerIds.Contains(logRound.CurrentPlayerID)) {
                        LogRound.succeededPlayerIds.Add(logRound.CurrentPlayerID);
                        LogRound.NumPlayersSucceeded++;
                    }
                }
                logRound.FindingPosition = true;
            } else if (logRound.Info != null && !LogRound.IsShowCompletedOrEnded && logRound.FindingPosition && (index = line.Line.IndexOf("[ClientGameSession] NumPlayersAchievingObjective=")) > 0) {
                int position = int.Parse(line.Line.Substring(index + 49));
                if (position > 0) {
                    logRound.FindingPosition = false;
                    logRound.Info.Position = position;
                }
            } else if (line.Date > LogRound.LastRoundStart && (index = line.Line.IndexOf($"HandleServerPlayerProgress PlayerId=", StringComparison.OrdinalIgnoreCase)) > 0 && line.Line.IndexOf("succeeded=True", StringComparison.OrdinalIgnoreCase) > 0) {
                int prevIndex = line.Line.IndexOf(" ", index + 36);
                string playerId = line.Line.Substring(index + 36, prevIndex - index - 36);
                if (!LogRound.succeededPlayerIds.Contains(playerId)) {
                    LogRound.succeededPlayerIds.Add(playerId);
                    LogRound.NumPlayersSucceeded++;
                }
            } else if (line.Line.IndexOf("[GameSession] Changing state from Playing to GameOver", StringComparison.OrdinalIgnoreCase) > 0) {
                if (line.Date > LogRound.LastRoundStart) {
                    if (Stats.InShow && LogRound.LastPlayedRoundStart.HasValue && !LogRound.LastPlayedRoundEnd.HasValue) {
                        LogRound.LastPlayedRoundEnd = line.Date;
                    }
                    LogRound.IsLastRoundRunning = false;
                    LogRound.IsLastPlayedRoundStillPlaying = false;
                }
                if (logRound.Info != null) {
                    if (logRound.Info.End == DateTime.MinValue) {
                        logRound.Info.End = line.Date;
                    }
                    logRound.Info.Playing = false;
                }
            } else if (line.Line.IndexOf("[StateMainMenu] Loading scene MainMenu", StringComparison.OrdinalIgnoreCase) > 0
                       || line.Line.IndexOf("[GameStateMachine] Replacing FGClient.StatePrivateLobby with FGClient.StateMainMenu", StringComparison.OrdinalIgnoreCase) > 0
                       || line.Line.IndexOf("[GameStateMachine] Replacing FGClient.StateReloadingToMainMenu with FGClient.StateMainMenu", StringComparison.OrdinalIgnoreCase) > 0
                       || line.Line.IndexOf("[GlobalGameStateClient] SwitchToDisconnectingState", StringComparison.OrdinalIgnoreCase) > 0
                       || line.Line.IndexOf("The remote sent a disconnect request", StringComparison.OrdinalIgnoreCase) > 0
                       || line.Line.IndexOf("[ClientGlobalGameState] Client has been disconnected", StringComparison.OrdinalIgnoreCase) > 0) {
                if (LogRound.LastPlayedRoundStart.HasValue && !LogRound.LastPlayedRoundEnd.HasValue) {
                    LogRound.LastPlayedRoundEnd = line.Date;
                }
                LogRound.IsLastRoundRunning = false;
                LogRound.IsLastPlayedRoundStillPlaying = false;

                logRound.CountingPlayers = false;
                logRound.GetCurrentPlayerID = false;
                logRound.FindingPosition = false;

                if (logRound.Info != null) {
                    if (logRound.Info.End == DateTime.MinValue) {
                        logRound.Info.End = line.Date;
                    }
                    logRound.Info.Playing = false;
                    if (!LogRound.IsShowCompletedOrEnded) {
                        DateTime showStart = DateTime.MinValue;
                        DateTime showEnd = logRound.Info.End;
                        for (int i = 0; i < round.Count; i++) {
                            if (i == 0) {
                                showStart = round[i].Start;
                            }
                            round[i].ShowStart = showStart;
                            round[i].Playing = false;
                            round[i].Round = i + 1;
                            if (round[i].End == DateTime.MinValue) {
                                round[i].End = line.Date;
                            }
                            if (round[i].Start == DateTime.MinValue) {
                                round[i].Start = round[i].End;
                            }
                            if (i < (round.Count - 1)) {
                                round[i].Qualified = true;
                                if (!round[i].Finish.HasValue || round[i].Position == 0) {
                                    round[i].Position = round[i + 1].Players;
                                }
                            } else if (!round[i].Finish.HasValue || round[i].Position == 0) {
                                round[i].Position = round[i].Players;
                            }
                            round[i].VerifyName();
                            round[i].ShowEnd = showEnd;
                        }
                        Stats.InShow = false;
                        logRound.Info = null;
                        LogRound.IsShowCompletedOrEnded = true;
                        return true;
                    }
                }
                Stats.InShow = false;
                logRound.Info = null;
                LogRound.IsShowCompletedOrEnded = true;
            } else if (line.Line.IndexOf(" == [CompletedEpisodeDto] ==", StringComparison.OrdinalIgnoreCase) > 0) {
                if (logRound.Info == null || LogRound.IsShowCompletedOrEnded) { return false; }

                LogRound.SavedRoundCount = logRound.Info.Round;

                if (LogRound.IsLastRoundRunning) {
                    LogRound.LastPlayedRoundStart = logRound.Info.Start;
                    LogRound.IsLastPlayedRoundStillPlaying = true;
                }

                if (logRound.Info.End == DateTime.MinValue) {
                    logRound.Info.End = line.Date;
                }
                logRound.Info.Playing = false;

                RoundInfo roundInfo = null;
                StringReader sr = new StringReader(line.Line);
                string detail;
                bool foundRound = false;
                int maxRound = 0;
                DateTime showStart = DateTime.MinValue;
                int questKudos = 0;
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
                                LogRound.IsShowCompletedOrEnded = true;
                                return false;
                            }
                            roundInfo.VerifyName();

                            if (roundNum == 1) {
                                showStart = roundInfo.Start;
                            }
                            roundInfo.ShowStart = showStart;
                            roundInfo.Playing = false;
                            roundInfo.Round = roundNum;
                        } else {
                            LogRound.IsShowCompletedOrEnded = true;
                            return false;
                        }

                        if (roundInfo.End == DateTime.MinValue) {
                            roundInfo.End = line.Date;
                        }
                        if (roundInfo.Start == DateTime.MinValue) {
                            roundInfo.Start = roundInfo.End;
                        }
                    } else if (foundRound) {
                        if (detail.IndexOf("> Qualified: ", StringComparison.OrdinalIgnoreCase) == 0) {
                            char qualified = detail[13];
                            roundInfo.Qualified = qualified == 'T';
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
                    } else {
                        if (detail.IndexOf("> Kudos: ", StringComparison.OrdinalIgnoreCase) == 0) {
                            questKudos = int.Parse(detail.Substring(9));
                            //> Fame:, > Crowns:, > CurrentCrownShards:
                        }
                    }
                }
                roundInfo.Kudos += questKudos;

                if (round.Count > maxRound) {
                    LogRound.IsShowCompletedOrEnded = true;
                    return false;
                }

                DateTime showEnd = logRound.Info.End;
                for (int i = 0; i < round.Count; i++) {
                    round[i].ShowEnd = showEnd;
                }

                if (logRound.Info.Qualified) {
                    logRound.Info.Position = 1;
                    logRound.Info.Crown = true;
                    logRound.CountingPlayers = false;
                    logRound.GetCurrentPlayerID = false;
                    logRound.FindingPosition = false;
                    Stats.InShow = false;
                }
                logRound.Info = null;
                LogRound.IsShowCompletedOrEnded = true;
                return true;
            }
            return false;
        }
    }
}