using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public RoundInfo Info;
    }
    public class LogFileWatcher {
        private const int GameCheckDelay = 2000;
        private const int UpdateDelay = 500;

        private string filePath;
        private string prevFilePath;
        private List<LogLine> lines = new List<LogLine>();
        private bool running;
        private bool stop;
        private Thread gameWatcher, watcher, parser;

        public Stats StatsForm { get; set; }

        private bool isGameHasBeenLaunched;
        private bool isGameCurrentlyRunning;

        private bool autoChangeProfile;
        private string selectedShowId;
        private bool useShareCode;
        private string sessionId;
        private bool isCreativeWeeklyShow;

        public event Action<List<RoundInfo>> OnParsedLogLines;
        public event Action<List<RoundInfo>> OnParsedLogLinesCurrent;
        public event Action<DateTime> OnNewLogFileDate;
        public event Action<string> OnError;

        public void SetAutoChangeProfile(bool isEnabled) {
            this.autoChangeProfile = isEnabled;
        }

        public void Start(string logDirectory, string fileName) {
            if (this.running) { return; }

#if Debug
            Debug.WriteLine("LogFileWatcher is running!");
#endif
            this.filePath = Path.Combine(logDirectory, fileName);
            this.prevFilePath = Path.Combine(logDirectory, Path.GetFileNameWithoutExtension(fileName) + "-prev.log");
            this.stop = false;
            this.gameWatcher = new Thread(this.GameWatcher) { IsBackground = true };
            this.gameWatcher.Start();
            this.watcher = new Thread(this.ReadLogFile) { IsBackground = true };
            this.watcher.Start();
            this.parser = new Thread(this.ParseLines) { IsBackground = true };
            this.parser.Start();
        }

        public async Task Stop() {
            this.stop = true;
            while (this.running || this.watcher == null || this.watcher.ThreadState == System.Threading.ThreadState.Unstarted) {
                await Task.Delay(50);
            }
            this.lines = new List<LogLine>();
            await Task.Factory.StartNew(() => this.gameWatcher?.Join());
            await Task.Factory.StartNew(() => this.watcher?.Join());
            await Task.Factory.StartNew(() => this.parser?.Join());
        }

        private void GameWatcher() {
            while (!this.stop) {
                try {
                    this.isGameCurrentlyRunning = false;
                    Process[] processes = Process.GetProcesses();
                    string fallGuysProcessName = "FallGuys_client_game";
                    for (int i = 0; i < processes.Length; i++) {
                        string name = processes[i].ProcessName;
                        if (name.IndexOf(fallGuysProcessName, StringComparison.OrdinalIgnoreCase) >= 0) {
                            this.isGameHasBeenLaunched = true;
                            this.isGameCurrentlyRunning = true;
                            Stats.IsGameClosed = false;
                            break;
                        }
                    }
                    if (this.isGameHasBeenLaunched && !this.isGameCurrentlyRunning) {
                        Stats.IsGameClosed = true;
                    }
                } catch (Exception ex) {
                    this.OnError?.Invoke(ex.Message);
                }
                Thread.Sleep(GameCheckDelay);
            }
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
                                Stats.SavedRoundCount = 0;
                                lastDate = line.Date;
                                offset = line.Offset;
                                lock (this.lines) {
                                    this.lines.AddRange(currentLines);
                                    currentLines.Clear();
                                }
                            } else if (line.Line.IndexOf("[StateMatchmaking] Begin", StringComparison.OrdinalIgnoreCase) > 0
                                       || line.Line.IndexOf("[GameStateMachine] Replacing FGClient.StatePrivateLobby with FGClient.StateConnectToGame", StringComparison.OrdinalIgnoreCase) > 0
                                       || line.Line.IndexOf("[GameStateMachine] Replacing FGClient.StatePrivateLobby with FGClient.StateMainMenu", StringComparison.OrdinalIgnoreCase) > 0
                                       || line.Line.IndexOf("[GameStateMachine] Replacing FGClient.StateReloadingToMainMenu with FGClient.StateMainMenu", StringComparison.OrdinalIgnoreCase) > 0
                                       || line.Line.IndexOf("[StateMainMenu] Loading scene MainMenu", StringComparison.OrdinalIgnoreCase) > 0
                                       || (Stats.IsGameClosed && line.Line.IndexOf("The remote sent a disconnect request", StringComparison.OrdinalIgnoreCase) > 0)) {
                                offset = i > 0 ? tempLines[i - 1].Offset : offset;
                                lastDate = line.Date;
                            } else if (line.Line.IndexOf("[HandleSuccessfulLogin] Selected show is", StringComparison.OrdinalIgnoreCase) > 0) {
                                if (this.autoChangeProfile && Stats.InShow && !Stats.EndedShow) {
                                    this.StatsForm.SetLinkedProfile(this.selectedShowId, logRound.PrivateLobby, this.selectedShowId.StartsWith("show_wle_s10_"));
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

        private string GetRoundId(string shareCode) {
            switch (shareCode) {
                case "1127-0302-4545": return "wle_s10_orig_round_001";
                case "2416-3885-2780": return "wle_s10_orig_round_002";
                case "6212-4520-8328": return "wle_s10_orig_round_003";
                case "2777-8402-7007": return "wle_s10_orig_round_004";
                case "2384-0516-9077": return "wle_s10_orig_round_005";
                case "0128-0546-8951": return "wle_s10_orig_round_006";
                case "9189-7523-8901": return "wle_s10_orig_round_007";
                case "0653-2576-5236": return "wle_s10_orig_round_008";
                case "7750-7786-5732": return "wle_s10_orig_round_009";
                case "1794-4080-7040": return "wle_s10_orig_round_012";
                case "4446-4876-4968": return "wle_s10_orig_round_013";
                case "7274-6756-2481": return "wle_s10_orig_round_014";
                case "9426-4640-3293": return "wle_s10_orig_round_015";
                case "7387-6177-4493": return "wle_s10_orig_round_016";
                case "8685-7305-6864": return "wle_s10_orig_round_019";
                case "4582-3265-6150": return "wle_s10_orig_round_020";
                case "8300-9964-4945": return "wle_s10_orig_round_021";
                case "9525-6213-8767": return "wle_s10_orig_round_022";
                case "9078-2884-2372": return "wle_s10_orig_round_023";
                case "1335-7488-1452": return "wle_s10_orig_round_026";
                case "8145-6018-5093": return "wle_s10_orig_round_027";
                case "0633-3680-7102": return "wle_s10_orig_round_028";
                case "3207-5100-4977": return "wle_s10_orig_round_029";
                case "0324-4730-9285": return "wle_s10_orig_round_032";
                case "7488-7333-3120": return "wle_s10_orig_round_033";
                case "0942-4996-5802": return "wle_s10_orig_round_034";
                case "5673-8789-9890": return "wle_s10_orig_round_035";
                case "1521-1552-6833": return "wle_s10_orig_round_036";
                case "5608-1711-5644": return "wle_s10_orig_round_037";
                case "6401-4333-5888": return "wle_s10_orig_round_038";
                case "1920-3797-9890": return "wle_s10_orig_round_039";
                case "1595-7489-8714": return "wle_s10_orig_round_040";
                case "2019-5582-0500": return "wle_s10_orig_round_041";
                case "0567-6834-7490": return "wle_s10_orig_round_042";
                case "8398-4477-7834": return "wle_s10_orig_round_043";
                case "2767-1753-8429": return "wle_s10_orig_round_044";
                case "6748-6192-9739": return "wle_s10_orig_round_045";
                case "9641-5398-7416": return "wle_s10_orig_round_046";
                case "7895-4812-3429": return "wle_s10_orig_round_047";
                case "1468-0990-4257": return "wle_s10_orig_round_048";
                case "8058-9910-7007": return "wle_s10_round_001";
                case "6546-9859-4336": return "wle_s10_round_002";
                case "9366-4809-0021": return "wle_s10_round_003";
                case "8895-2034-3061": return "wle_s10_round_005";
                case "6970-1344-9780": return "wle_s10_round_006";
                case "3541-3776-9131": return "wle_s10_round_007";
                case "9335-2112-8890": return "wle_s10_round_008";
                case "9014-0444-9613": return "wle_s10_round_010";
                case "4409-6583-6207": return "wle_s10_round_011";
                case "8113-7002-5798": return "wle_s10_round_012";
                case "0733-6671-4871": return "wle_s10_orig_round_010";
                case "6498-0353-5009": return "wle_s10_orig_round_011";
                case "7774-2277-5742": return "wle_s10_orig_round_017";
                case "2228-9895-3526": return "wle_s10_orig_round_018";
                case "7652-0829-4538": return "wle_s10_orig_round_024";
                case "1976-1259-2690": return "wle_s10_orig_round_025";
                case "4694-8620-4972": return "wle_s10_orig_round_030";
                case "6464-4069-3540": return "wle_s10_orig_round_031";
                case "8993-4568-6925": return "wle_s10_round_004";
                case "7495-5141-5265": return "wle_s10_round_009";
            }
            return shareCode;
        }

        private bool GetIsRealFinalRound(string roundId, string showId) {
            if (showId.StartsWith("show_wle_s10_") && showId.IndexOf("_srs", StringComparison.OrdinalIgnoreCase) != -1) { this.isCreativeWeeklyShow = true; return true; }

            this.isCreativeWeeklyShow = false;

            return (roundId.IndexOf("round_jinxed", StringComparison.OrdinalIgnoreCase) != -1
                        && roundId.IndexOf("_non_final", StringComparison.OrdinalIgnoreCase) == -1)

                    || (roundId.IndexOf("round_fall_ball", StringComparison.OrdinalIgnoreCase) != -1
                        && roundId.IndexOf("_non_final", StringComparison.OrdinalIgnoreCase) == -1
                        && roundId.IndexOf("_cup_only", StringComparison.OrdinalIgnoreCase) == -1)

                    || (roundId.IndexOf("round_1v1_volleyfall", StringComparison.OrdinalIgnoreCase) != -1
                        && roundId.IndexOf("_final", StringComparison.OrdinalIgnoreCase) != -1)

                    || (roundId.IndexOf("round_pixelperfect", StringComparison.OrdinalIgnoreCase) != -1
                        && roundId.Substring(roundId.Length - 6).ToLower() == "_final")

                    || roundId.EndsWith("_timeattack_final", StringComparison.OrdinalIgnoreCase)

                    || roundId.EndsWith("_xtreme_party_final", StringComparison.OrdinalIgnoreCase);
        }

        private bool GetIsModeException(string roundId) {
            return roundId.IndexOf("round_lava_event_only_slime_climb", StringComparison.OrdinalIgnoreCase) != -1
                   || roundId.IndexOf("round_kraken_attack_only_finals", StringComparison.OrdinalIgnoreCase) != -1
                   || roundId.IndexOf("round_blastball_only_finals", StringComparison.OrdinalIgnoreCase) != -1
                   || roundId.IndexOf("round_floor_fall_only_finals", StringComparison.OrdinalIgnoreCase) != -1
                   || roundId.IndexOf("round_hexsnake_only_finals", StringComparison.OrdinalIgnoreCase) != -1
                   || roundId.IndexOf("round_jump_showdown_only_finals", StringComparison.OrdinalIgnoreCase) != -1
                   || roundId.IndexOf("round_hexaring_only_finals", StringComparison.OrdinalIgnoreCase) != -1
                   || roundId.IndexOf("round_tunnel_final_only_finals", StringComparison.OrdinalIgnoreCase) != -1
                   || roundId.IndexOf("round_thin_ice_only_finals", StringComparison.OrdinalIgnoreCase) != -1
                   || roundId.IndexOf("round_drumtop_event_only", StringComparison.OrdinalIgnoreCase) != -1
                   || roundId.IndexOf("round_floor_fall_event_only", StringComparison.OrdinalIgnoreCase) != -1
                   || roundId.IndexOf("round_floor_fall_event_only_low_grav", StringComparison.OrdinalIgnoreCase) != -1
                   || roundId.IndexOf("round_floor_fall_event_walnut", StringComparison.OrdinalIgnoreCase) != -1
                   || roundId.IndexOf("round_hexaring_event_walnut", StringComparison.OrdinalIgnoreCase) != -1
                   || roundId.IndexOf("round_hexsnake_event_walnut", StringComparison.OrdinalIgnoreCase) != -1
                   || roundId.IndexOf("round_blastball_arenasurvival_blast_ball_trials", StringComparison.OrdinalIgnoreCase) != -1
                   || roundId.IndexOf("round_robotrampage_arena_2_ss2_show1", StringComparison.OrdinalIgnoreCase) != -1;
        }

        private bool GetIsFinalException(string roundId) {
            return ((roundId.IndexOf("round_lava_event_only_slime_climb", StringComparison.OrdinalIgnoreCase) != -1
                     || roundId.IndexOf("round_kraken_attack_only_finals", StringComparison.OrdinalIgnoreCase) != -1
                     || roundId.IndexOf("round_blastball_only_finals", StringComparison.OrdinalIgnoreCase) != -1
                     || roundId.IndexOf("round_floor_fall_only_finals", StringComparison.OrdinalIgnoreCase) != -1
                     || roundId.IndexOf("round_hexsnake_only_finals", StringComparison.OrdinalIgnoreCase) != -1
                     || roundId.IndexOf("round_jump_showdown_only_finals", StringComparison.OrdinalIgnoreCase) != -1
                     || roundId.IndexOf("round_hexaring_only_finals", StringComparison.OrdinalIgnoreCase) != -1
                     || roundId.IndexOf("round_tunnel_final_only_finals", StringComparison.OrdinalIgnoreCase) != -1
                     || roundId.IndexOf("round_thin_ice_only_finals", StringComparison.OrdinalIgnoreCase) != -1
                     || roundId.IndexOf("round_drumtop_event_only", StringComparison.OrdinalIgnoreCase) != -1
                     || roundId.IndexOf("round_floor_fall_event_only", StringComparison.OrdinalIgnoreCase) != -1
                     || roundId.IndexOf("round_floor_fall_event_only_low_grav", StringComparison.OrdinalIgnoreCase) != -1
                     || roundId.IndexOf("round_floor_fall_event_walnut", StringComparison.OrdinalIgnoreCase) != -1
                     || roundId.IndexOf("round_hexaring_event_walnut", StringComparison.OrdinalIgnoreCase) != -1
                     || roundId.IndexOf("round_hexsnake_event_walnut", StringComparison.OrdinalIgnoreCase) != -1)
                         && roundId.Substring(roundId.Length - 6).ToLower() == "_final")

                     || (roundId.IndexOf("round_blastball_arenasurvival_blast_ball_trials", StringComparison.OrdinalIgnoreCase) != -1
                         && roundId.Substring(roundId.Length - 3).ToLower() == "_fn")

                     || (roundId.IndexOf("round_robotrampage_arena_2_ss2_show1", StringComparison.OrdinalIgnoreCase) != -1
                         && roundId.Substring(roundId.Length - 3) == "_03");
        }

        private bool GetIsTeamException(string roundId) {
            return roundId.IndexOf("ound_1v1_volleyfall", StringComparison.OrdinalIgnoreCase) != -1
                   && (roundId.IndexOf("_duos", StringComparison.OrdinalIgnoreCase) != -1
                   || roundId.IndexOf("_squads", StringComparison.OrdinalIgnoreCase) != -1);
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

                Stats.EndedShow = false;

                logRound.PrivateLobby = line.Line.IndexOf("StatePrivateLobby", StringComparison.OrdinalIgnoreCase) > 0;
                logRound.CurrentlyInParty = !logRound.PrivateLobby && (line.Line.IndexOf("solo", StringComparison.OrdinalIgnoreCase) == -1);
                logRound.GetServerPing = true;
                logRound.CountingPlayers = false;
                logRound.GetCurrentPlayerID = false;
                logRound.FindingPosition = false;

                round.Clear();
            } else if (logRound.Info == null && (index = line.Line.IndexOf("[HandleSuccessfulLogin] Selected show is", StringComparison.OrdinalIgnoreCase)) > 0) {
                this.selectedShowId = line.Line.Substring(line.Line.Length - (line.Line.Length - index - 41));
                if (this.selectedShowId.StartsWith("ugc-")) {
                    this.selectedShowId = this.selectedShowId.Substring(4);
                    this.useShareCode = true;
                } else {
                    this.useShareCode = false;
                }
            } else if (logRound.Info == null && (index = line.Line.IndexOf("[HandleSuccessfulLogin] Session: ", StringComparison.OrdinalIgnoreCase)) > 0) {
                //Store SessionID to prevent duplicates (for fallalytics)
                this.sessionId = line.Line.Substring(index + 33);
            } else if (logRound.GetServerPing && line.Line.IndexOf("Client address: ", StringComparison.OrdinalIgnoreCase) > 0) {
                index = line.Line.IndexOf("RTT: ");
                if (index > 0) {
                    logRound.GetServerPing = false;
                    int msIndex = line.Line.IndexOf("ms", index);
                    Stats.LastServerPing = int.Parse(line.Line.Substring(index + 5, msIndex - index - 5));
                }
            } else if ((index = line.Line.IndexOf("[StateGameLoading] Loading game level scene", StringComparison.OrdinalIgnoreCase)) > 0) {
                if (line.Date > Stats.LastRoundStart) {
                    Stats.LastRoundStart = line.Date;
                    Stats.InShow = true;
                    Stats.succeededPlayerIds.Clear();
                    Stats.NumPlayersSucceeded = 0;
                    Stats.IsLastRoundRunning = true;
                    Stats.IsLastPlayedRoundStillPlaying = false;
                    Stats.LastPlayedRoundStart = null;
                    Stats.LastPlayedRoundEnd = null;
                }

                logRound.Info = new RoundInfo { ShowNameId = this.selectedShowId, SessionId = this.sessionId, UseShareCode = this.useShareCode };

                int index2 = line.Line.IndexOf(" ", index + 44);
                if (index2 < 0) { index2 = line.Line.Length; }

                logRound.Info.SceneName = line.Line.Substring(index + 44, index2 - index - 44);
                if (_sceneNameReplacer.TryGetValue(logRound.Info.SceneName, out string newName)) {
                    logRound.Info.SceneName = newName;
                }

                logRound.FindingPosition = false;

                round.Add(logRound.Info);
            } else if (logRound.Info != null && (index = line.Line.IndexOf("[StateGameLoading] Finished loading game level", StringComparison.OrdinalIgnoreCase)) > 0) {
                int index2 = line.Line.IndexOf(". ", index + 62);
                if (index2 < 0) { index2 = line.Line.Length; }

                if (logRound.Info.UseShareCode) {
                    logRound.Info.Name = this.GetRoundId(line.Line.Substring(index + 66, index2 - index - 66));
                } else {
                    logRound.Info.Name = line.Line.Substring(index + 62, index2 - index - 62);
                }

                if (this.GetIsRealFinalRound(logRound.Info.Name, this.selectedShowId) || logRound.Info.UseShareCode) {
                    logRound.Info.IsFinal = true;
                } else if (this.GetIsModeException(logRound.Info.Name)) {
                    logRound.Info.IsFinal = this.GetIsFinalException(logRound.Info.Name);
                } else if (logRound.Info.Name.StartsWith("wle_s10_")) {
                    logRound.Info.IsFinal = logRound.IsFinal || (!logRound.HasIsFinal && LevelStats.ALL.TryGetValue(logRound.Info.Name, out LevelStats levelStats) && levelStats.IsFinal);
                } else {
                    logRound.Info.IsFinal = logRound.IsFinal || (!logRound.HasIsFinal && LevelStats.SceneToRound.TryGetValue(logRound.Info.SceneName, out string roundName) && LevelStats.ALL.TryGetValue(roundName, out LevelStats levelStats) && levelStats.IsFinal);
                }
                logRound.Info.IsTeam = this.GetIsTeamException(logRound.Info.Name);

                logRound.Info.Round = !Stats.EndedShow ? round.Count : Stats.SavedRoundCount + round.Count;
                logRound.Info.Start = line.Date;
                logRound.Info.InParty = logRound.CurrentlyInParty;
                logRound.Info.PrivateLobby = logRound.PrivateLobby;
                logRound.Info.GameDuration = logRound.Duration;

                logRound.CountingPlayers = true;
                logRound.GetCurrentPlayerID = true;
            } else if (logRound.Info != null && (index = line.Line.IndexOf("NetworkGameOptions: durationInSeconds=", StringComparison.OrdinalIgnoreCase)) > 0) { // legacy code // It seems to have been deleted from the log file now.
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
                    if (line.Date > Stats.LastRoundStart && !Stats.succeededPlayerIds.Contains(logRound.CurrentPlayerID)) {
                        Stats.succeededPlayerIds.Add(logRound.CurrentPlayerID);
                        Stats.NumPlayersSucceeded++;
                    }
                    logRound.FindingPosition = true;
                }
            } else if (logRound.Info != null && !Stats.EndedShow && logRound.FindingPosition && (index = line.Line.IndexOf("[ClientGameSession] NumPlayersAchievingObjective=")) > 0) {
                int position = int.Parse(line.Line.Substring(index + 49));
                if (position > 0) {
                    logRound.FindingPosition = false;
                    logRound.Info.Position = position;
                }
            } else if (line.Date > Stats.LastRoundStart && (index = line.Line.IndexOf($"HandleServerPlayerProgress PlayerId=", StringComparison.OrdinalIgnoreCase)) > 0 && line.Line.IndexOf("succeeded=True", StringComparison.OrdinalIgnoreCase) > 0) {
                int prevIndex = line.Line.IndexOf(" ", index + 36);
                string playerId = line.Line.Substring(index + 36, prevIndex - index - 36);
                if (!Stats.succeededPlayerIds.Contains(playerId)) {
                    Stats.succeededPlayerIds.Add(playerId);
                    Stats.NumPlayersSucceeded++;
                }
            } else if (line.Line.IndexOf("[GameSession] Changing state from Playing to GameOver", StringComparison.OrdinalIgnoreCase) > 0) {
                if (line.Date > Stats.LastRoundStart) {
                    if (Stats.InShow && Stats.LastPlayedRoundStart.HasValue && !Stats.LastPlayedRoundEnd.HasValue) {
                        Stats.LastPlayedRoundEnd = line.Date;
                    }
                    Stats.IsLastRoundRunning = false;
                    Stats.IsLastPlayedRoundStillPlaying = false;
                }
                if (logRound.Info != null) {
                    if (logRound.Info.End == DateTime.MinValue) {
                        logRound.Info.End = line.Date;
                    }
                    logRound.Info.Playing = false;
                }
            } else if (line.Line.IndexOf("[GameStateMachine] Replacing FGClient.StatePrivateLobby with FGClient.StateMainMenu", StringComparison.OrdinalIgnoreCase) > 0
                       || line.Line.IndexOf("[GameStateMachine] Replacing FGClient.StateReloadingToMainMenu with FGClient.StateMainMenu", StringComparison.OrdinalIgnoreCase) > 0
                       || line.Line.IndexOf("[StateMainMenu] Loading scene MainMenu", StringComparison.OrdinalIgnoreCase) > 0
                       || (Stats.IsGameClosed && line.Line.IndexOf("The remote sent a disconnect request", StringComparison.OrdinalIgnoreCase) > 0)) {
                if (Stats.InShow && Stats.LastPlayedRoundStart.HasValue && !Stats.LastPlayedRoundEnd.HasValue) {
                    Stats.LastPlayedRoundEnd = line.Date;
                }
                Stats.IsLastRoundRunning = false;
                Stats.IsLastPlayedRoundStillPlaying = false;

                logRound.CountingPlayers = false;
                logRound.GetCurrentPlayerID = false;
                logRound.FindingPosition = false;

                if (logRound.Info != null) {
                    if (logRound.Info.End == DateTime.MinValue) {
                        logRound.Info.End = line.Date;
                    }
                    logRound.Info.Playing = false;
                    if (!Stats.EndedShow) {
                        DateTime showStart = DateTime.MinValue;
                        DateTime showEnd = logRound.Info.End;
                        for (int i = 0; i < round.Count; i++) {
                            if (string.IsNullOrEmpty(round[i].Name)) {
                                round.RemoveAt(i);
                                logRound.Info = null;
                                Stats.InShow = false;
                                Stats.EndedShow = true;
                                return true;
                            }
                            if (round[i].Name.StartsWith("ugc-")) {
                                round[i].Name = round[i].Name.Substring(4);
                            }
                            round[i].VerifyName();
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
                                round[i].AbandonShow = true;
                            } else if (round[i].UseShareCode && round[i].Finish.HasValue) {
                                round[i].Qualified = true;
                                round[i].Crown = true;
                            } else {
                                round[i].AbandonShow = true;
                            }
                            round[i].ShowEnd = showEnd;
                        }
                        logRound.Info = null;
                        Stats.InShow = false;
                        Stats.EndedShow = true;
                        return true;
                    }
                }
                logRound.Info = null;
                Stats.InShow = false;
                Stats.EndedShow = true;
            } else if (line.Line.IndexOf(" == [CompletedEpisodeDto] ==", StringComparison.OrdinalIgnoreCase) > 0) {
                if (logRound.Info == null || Stats.EndedShow) { return false; }

                Stats.SavedRoundCount = logRound.Info.Round;
                Stats.EndedShow = true;

                if (logRound.Info.End == DateTime.MinValue) {
                    Stats.LastPlayedRoundStart = logRound.Info.Start;
                    Stats.IsLastPlayedRoundStillPlaying = true;
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

                        if (roundName.StartsWith("ugc-")) {
                            roundName = roundName.Substring(4);
                        }

                        if (roundNum - 1 < round.Count) {
                            if (roundNum > maxRound) {
                                maxRound = roundNum;
                            }

                            roundInfo = round[roundNum - 1];
                            if (string.IsNullOrEmpty(roundInfo.Name)) {
                                return false;
                            }
                            if (roundInfo.Name.StartsWith("ugc-")) {
                                roundInfo.Name = roundInfo.Name.Substring(4);
                            }
                            if (!roundInfo.Name.Equals(roundName, StringComparison.OrdinalIgnoreCase)) {
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
                    return false;
                }

                DateTime showEnd = logRound.Info.End;
                for (int i = 0; i < round.Count; i++) {
                    round[i].ShowEnd = showEnd;
                }

                if (logRound.Info.Qualified) {
                    if (!this.isCreativeWeeklyShow) {
                        logRound.Info.Position = 1;
                    }
                    logRound.Info.Crown = true;
                    logRound.CountingPlayers = false;
                    logRound.GetCurrentPlayerID = false;
                    logRound.FindingPosition = false;
                }
                logRound.Info = null;
                return true;
            }
            return false;
        }
    }
}