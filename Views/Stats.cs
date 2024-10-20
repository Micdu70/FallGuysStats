﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
#if AllowUpdate
using System.IO.Compression;
#endif
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using LiteDB;
using MaxMind.Db;
using Microsoft.Win32;
using MetroFramework;
using MetroFramework.Controls;
using MetroFramework.Components;
using System.Net;
using System.Text.RegularExpressions;

namespace FallGuysStats {
    public partial class Stats : MetroFramework.Forms.MetroForm {
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        //[DllImport("user32.dll")]
        //private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        //[DllImport("user32.dll")]
        //public static extern IntPtr GetForegroundWindow();
        //[DllImport("user32.dll")]
        //private static extern IntPtr GetActiveWindow();

        public enum DWMWINDOWATTRIBUTE {
            DWMWA_WINDOW_CORNER_PREFERENCE = 33
        }

        // The DWM_WINDOW_CORNER_PREFERENCE enum for DwmSetWindowAttribute's third parameter, which tells the function
        // what value of the enum to set.
        public enum DWM_WINDOW_CORNER_PREFERENCE {
            DWMWCP_DEFAULT = 0,
            DWMWCP_DONOTROUND = 1,
            DWMWCP_ROUND = 2,
            DWMWCP_ROUNDSMALL = 3
        }

        // Import dwmapi.dll and define DwmSetWindowAttribute in C# corresponding to the native function.
        [DllImport("dwmapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern long DwmSetWindowAttribute(IntPtr hWnd,
            DWMWINDOWATTRIBUTE attribute,
            ref DWM_WINDOW_CORNER_PREFERENCE pvAttribute,
            uint cbAttribute);

        [STAThread]
        private static void Main() {
            try {
#if AllowUpdate
                bool isAppUpdated = false;
                if (File.Exists(Path.GetFileName(Assembly.GetEntryAssembly().Location) + ".bak")) {
                    isAppUpdated = true;
                }
                if (isAppUpdated || !IsAlreadyRunning(CultureInfo.CurrentUICulture.Name.Substring(0, 2))) {
#else
                if (!IsAlreadyRunning(CultureInfo.CurrentUICulture.Name.Substring(0, 2))) {
#endif
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new Stats());
                }
            } catch (Exception ex) {
                MessageBox.Show(ex.ToString(), @"Run Exception",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private static bool IsAlreadyRunning(string sysLang) {
            try {
                int processCount = 0;
                Process[] processes = Process.GetProcesses();
                for (int i = 0; i < processes.Length; i++) {
                    if (AppDomain.CurrentDomain.FriendlyName.Equals(processes[i].ProcessName + ".exe")) processCount++;
                    if (processCount > 1) {
                        CurrentLanguage = string.Equals(sysLang, "fr", StringComparison.Ordinal) ? 1 :
                                          string.Equals(sysLang, "ko", StringComparison.Ordinal) ? 2 :
                                          string.Equals(sysLang, "ja", StringComparison.Ordinal) ? 3 :
                                          string.Equals(sysLang, "zh", StringComparison.Ordinal) ? 4 : 0;
                        MessageBox.Show(Multilingual.GetWord("message_tracker_already_running"), Multilingual.GetWord("message_already_running_caption"),
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return true;
                    }
                }
                return false;
            } catch (Exception ex) {
                MessageBox.Show(ex.ToString(), @"Process Exception",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return true;
            }
        }
        private static readonly string LOGFILENAME = "Player.log";
        public static readonly List<DateTime> Seasons = new List<DateTime> {
            new DateTime(2020, 8, 4, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2020, 10, 8, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2020, 12, 15, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2021, 3, 22, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2021, 7, 20, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2021, 11, 30, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2022, 6, 21, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2022, 9, 15, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2022, 11, 22, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2023, 5, 10, 0, 0, 0, DateTimeKind.Utc)
        };
        private static DateTime SeasonStart, WeekStart, DayStart;
        private static DateTime SessionStart = DateTime.UtcNow;

        public static bool HideOverlayTime = false;
        public static bool IsOverlayPingVisible = false;

        public static bool IsGameRunning = false;
        public static bool IsGameHasBeenClosed = false;

        public static bool InShow = false;
        public static bool EndedShow = false;

        public static bool ToggleServerInfo = false;
        public static DateTime ConnectedToServerDate = DateTime.MinValue;
        public static string LastServerIp = string.Empty;
        public static string LastServerCountryCode = string.Empty;
        public static long LastServerPing = 0;
        public static bool IsBadServerPing = false;

        public static List<string> succeededPlayerIds = new List<string>();

        public static int SavedRoundCount { get; set; }
        public static int NumPlayersSucceeded { get; set; }
        public static bool IsLastRoundRunning { get; set; }
        public static bool IsLastPlayedRoundStillPlaying { get; set; }

        public static DateTime LastGameStart { get; set; } = DateTime.MinValue;
        public static DateTime LastRoundLoad { get; set; } = DateTime.MinValue;
        public static DateTime? LastPlayedRoundStart { get; set; } = null;
        public static DateTime? LastPlayedRoundEnd { get; set; } = null;

        public static int CurrentLanguage = 1;

        public static MetroThemeStyle CurrentTheme = MetroThemeStyle.Dark;

        private static readonly FallalyticsReporter FallalyticsReporter = new FallalyticsReporter();

        public static Bitmap ImageOpacity(Image sourceImage, float opacity = 1F) {
            Bitmap bmp = new Bitmap(sourceImage.Width, sourceImage.Height);
            Graphics gp = Graphics.FromImage(bmp);
            ColorMatrix clrMatrix = new ColorMatrix { Matrix33 = opacity };
            ImageAttributes imgAttribute = new ImageAttributes();
            imgAttribute.SetColorMatrix(clrMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            gp.DrawImage(sourceImage, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, sourceImage.Width, sourceImage.Height, GraphicsUnit.Pixel, imgAttribute);
            gp.Dispose();
            return bmp;
        }

        private readonly DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
        private readonly DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
        public List<LevelStats> StatDetails = new List<LevelStats>();
        public List<RoundInfo> CurrentRound = null;
        public List<RoundInfo> AllStats = new List<RoundInfo>();
        public Dictionary<string, LevelStats> StatLookup = new Dictionary<string, LevelStats>();
        private readonly LogFileWatcher logFile = new LogFileWatcher();
        private int Shows, Rounds;
        private int CustomShows, CustomRounds;
        private TimeSpan Duration;
        private int Wins;
        private int Finals;
        private int Kudos;
        private int GoldMedals, SilverMedals, BronzeMedals, PinkMedals, EliminatedMedals;
        private int CustomGoldMedals, CustomSilverMedals, CustomBronzeMedals, CustomPinkMedals, CustomEliminatedMedals;
        private int nextShowID;
        private bool minimizeAfterGameLaunch;
        private bool loadingExisting;
        private bool updateFilterType;
        private bool updateFilterRange;
        private DateTime customfilterRangeStart = DateTime.MinValue;
        private DateTime customfilterRangeEnd = DateTime.MaxValue;
        private int selectedCustomTemplateSeason;
        private bool updateSelectedProfile;
        private bool useLinkedProfiles;
        public LiteDatabase StatsDB;
        public ILiteCollection<RoundInfo> RoundDetails;
        public ILiteCollection<UserSettings> UserSettings;
        public ILiteCollection<Profiles> Profiles;
        public List<Profiles> AllProfiles = new List<Profiles>();
        public List<ToolStripMenuItem> ProfileMenuItems = new List<ToolStripMenuItem>();
        public UserSettings CurrentSettings;
        public Overlay overlay;
        public bool isUpdate;
        public readonly string pathToIPinfoDb = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "country.mmdb");
        public readonly DateTime startupTime = DateTime.UtcNow;
        private DateTime lastAddedShow = DateTime.MinValue;
        private int askedPreviousShows = 0;
        private int currentProfile;
        private int currentLanguage;
        private Color infoStripForeColor;

        private readonly Image numberOne = ImageOpacity(Properties.Resources.number_1, 0.5F);
        private readonly Image numberTwo = ImageOpacity(Properties.Resources.number_2, 0.5F);
        private readonly Image numberThree = ImageOpacity(Properties.Resources.number_3, 0.5F);
        private readonly Image numberFour = ImageOpacity(Properties.Resources.number_4, 0.5F);
        private readonly Image numberFive = ImageOpacity(Properties.Resources.number_5, 0.5F);
        private readonly Image numberSix = ImageOpacity(Properties.Resources.number_6, 0.5F);
        private readonly Image numberSeven = ImageOpacity(Properties.Resources.number_7, 0.5F);
        private readonly Image numberEight = ImageOpacity(Properties.Resources.number_8, 0.5F);
        private readonly Image numberNine = ImageOpacity(Properties.Resources.number_9, 0.5F);

        private bool shiftKeyToggle; //, ctrlKeyToggle;

        private MetroToolTip mtt = new MetroToolTip();
        private readonly MetroToolTip cmtt = new MetroToolTip();
        //private readonly MetroToolTip omtt = new MetroToolTip();
        //private readonly MetroToolTip ocmtt = new MetroToolTip();

        private readonly DWM_WINDOW_CORNER_PREFERENCE windowConerPreference = DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUNDSMALL;

        private bool isStartingUp = true;

        private bool onlyRefreshFilter;

        public Point screenCenter;
        public readonly string FALLGUYSDB_API_URL = "https://api2.fallguysdb.info/api/";

        public readonly string[] publicShowIdList = {
            "main_show",
            "squads_2player_template",
            "squads_4player",
            "event_xtreme_fall_guys_template",
            "event_xtreme_fall_guys_squads_template",
            "event_only_finals_v2_template",
            "event_only_races_any_final_template",
            "event_only_fall_ball_template",
            "event_only_hexaring_template",
            "event_only_floor_fall_template",
            "event_only_floor_fall_low_grav",
            "event_only_blast_ball_trials_template",
            "event_only_thin_ice_template",
            "event_only_slime_climb",
            "event_only_jump_club_template",
            "event_only_hoverboard_template",
            "event_walnut_template",
            "survival_of_the_fittest",
            "show_robotrampage_ss2_show1_template",
            "event_le_anchovy_template",
            "event_pixel_palooza_template",
            "xtreme_party",
            "invisibeans_mode",
            "fall_guys_creative_mode",
            "private_lobbies"
        };

        public readonly Dictionary<string, string> LevelIdReplacerFalloween = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
            { "wle_fp5_falloween_round_1", "current_wle_fp5_falloween_7_01_01" },
            { "wle_fp5_falloween_round_2", "current_wle_fp5_falloween_7_01_02" },
            { "wle_fp5_falloween_round_3", "current_wle_fp5_falloween_7_02_01" },
            { "wle_fp5_falloween_round_4", "current_wle_fp5_falloween_7_01_03" },
            { "wle_fp5_falloween_round_5", "current_wle_fp5_falloween_7_01_05" },
            { "wle_fp5_falloween_round_6", "current_wle_fp5_falloween_7_01_06" },
            { "wle_fp5_falloween_round_7", "current_wle_fp5_falloween_7_01_07" },
            { "wle_fp5_falloween_round_8", "current_wle_fp5_falloween_7_01_08" },
            { "wle_fp5_falloween_round_9", "current_wle_fp5_falloween_7_01_09" },
            { "wle_fp5_falloween_round_10", "current_wle_fp5_falloween_7_04_01" },
            { "wle_fp5_falloween_round_12", "current_wle_fp5_falloween_7_10_01" },
            { "wle_fp5_falloween_round_13", "current_wle_fp5_falloween_7_04_03" },
            { "wle_fp5_falloween_round_14", "current_wle_fp5_falloween_7_04_04" },
            { "wle_fp5_falloween_round_15", "current_wle_fp5_falloween_7_03_01" },
            { "wle_fp5_falloween_round_16", "current_wle_fp5_falloween_7_05_01" },
            { "wle_fp5_falloween_round_17", "current_wle_fp5_falloween_7_06_01" },
            { "wle_fp5_falloween_round_18", "current_wle_fp5_falloween_7_05_02" },
            { "wle_fp5_falloween_round_19", "current_wle_fp5_falloween_7_05_03" },
            { "wle_fp5_falloween_round_20", "current_wle_fp5_falloween_7_06_02" },
            { "wle_fp5_falloween_round_22", "current_wle_fp5_falloween_7_06_04" },
            { "wle_fp5_falloween_round_23", "current_wle_fp5_falloween_7_06_05" },
            { "wle_fp5_falloween_round_25", "current_wle_fp5_falloween_1_04" },
            { "wle_fp5_falloween_round_26", "current_wle_fp5_falloween_1_12" },
            { "wle_fp5_falloween_round_27", "current_wle_fp5_falloween_1_14" },
            { "wle_fp5_falloween_round_29", "current_wle_fp5_falloween_1_11" },
            { "wle_fp5_falloween_round_30", "current_wle_fp5_falloween_1_01" },
            { "wle_fp5_falloween_round_31", "current_wle_fp5_falloween_1_05" },
            { "wle_fp5_falloween_round_32", "current_wle_fp5_falloween_1_06" },
            { "wle_fp5_falloween_round_33", "current_wle_fp5_falloween_2_01" },
            { "wle_fp5_falloween_round_34", "current_wle_fp5_falloween_6_02" },
            { "wle_fp5_falloween_round_35", "current_wle_fp5_falloween_6_03" },
            { "wle_fp5_falloween_round_36", "current_wle_fp5_falloween_11_01" },
            { "wle_fp5_falloween_round_37", "current_wle_fp5_falloween_2_02" },
            { "wle_fp5_falloween_round_38", "current_wle_fp5_falloween_1_09" },
            { "wle_fp5_falloween_round_40", "current_wle_fp5_falloween_1_08" },
            { "wle_fp5_falloween_round_41", "current_wle_fp5_falloween_2_03" },
            { "wle_fp5_falloween_round_42", "current_wle_fp5_falloween_4_03" },
            { "wle_fp5_falloween_round_43", "current_wle_fp5_falloween_4_11" },
            { "wle_fp5_falloween_round_44", "current_wle_fp5_falloween_4_08" },
            { "wle_fp5_falloween_round_45", "current_wle_fp5_falloween_4_13" },
            { "wle_fp5_falloween_round_47", "current_wle_fp5_falloween_4_12" },
            { "wle_fp5_falloween_round_48", "current_wle_fp5_falloween_9_01" },
            { "wle_fp5_falloween_round_49", "current_wle_fp5_falloween_2_03_01" },
            { "wle_fp5_falloween_round_50", "current_wle_fp5_falloween_9_03" },
            { "wle_fp5_falloween_round_51", "current_wle_fp5_falloween_9_02" },
            { "wle_fp5_falloween_round_52", "current_wle_fp5_falloween_9_04" },
            { "wle_fp5_falloween_round_53", "current_wle_fp5_falloween_2_03_02" },
            { "wle_fp5_falloween_round_54", "current_wle_fp5_falloween_9_05" },
            { "wle_fp5_falloween_round_55", "current_wle_fp5_falloween_2_03_03" },
            { "wle_fp5_falloween_round_56", "current_wle_fp5_falloween_2_03_04" },
            { "wle_fp5_falloween_round_57", "current_wle_fp5_falloween_2_03_05" },
            { "wle_fp5_falloween_round_58", "current_wle_fp5_falloween_2_03_06" },
            { "wle_fp5_falloween_round_59", "current_wle_fp5_falloween_10_01" },
            { "wle_fp5_falloween_round_60", "current_wle_fp5_falloween_14_01" },
            { "wle_fp5_falloween_round_61", "current_wle_fp5_falloween_10_02" },
            { "wle_fp5_falloween_round_64", "current_wle_fp5_falloween_12_01" },
            { "wle_fp5_falloween_round_65", "current_wle_fp5_falloween_12_02" },
            { "wle_fp5_falloween_round_66", "current_wle_fp5_falloween_13_01" },
            { "wle_fp5_falloween_round_67", "current_wle_fp5_falloween_4_02" },
            { "wle_fp5_falloween_round_68", "current_wle_fp5_falloween_4_09" },
            { "wle_fp5_falloween_round_69", "current_wle_fp5_falloween_4_19" },
            { "wle_fp5_falloween_round_70", "current_wle_fp5_falloween_5_05" },
            { "wle_fp5_falloween_round_71", "current_wle_fp5_falloween_5_04" },
            { "wle_fp5_falloween_round_72", "current_wle_fp5_falloween_5_03" },
            { "wle_fp5_falloween_round_73", "current_wle_fp5_falloween_5_07" },
            { "wle_fp5_falloween_round_74", "current_wle_fp5_falloween_5_06" },
            { "wle_fp5_falloween_round_75", "current_wle_fp5_falloween_4_10" },
            { "wle_fp5_falloween_round_76", "current_wle_fp5_falloween_4_05" },
            { "wle_fp5_falloween_round_77", "current_wle_fp5_falloween_4_15" },
            { "wle_fp5_falloween_round_78", "current_wle_fp5_falloween_4_06" },
            { "wle_fp5_falloween_round_79", "current_wle_fp5_falloween_4_17" },
            { "wle_fp5_falloween_round_80", "current_wle_fp5_falloween_4_07" },
            { "wle_fp5_falloween_round_81", "current_wle_fp5_falloween_4_14" },
            { "wle_fp5_falloween_round_82", "current_wle_fp5_falloween_4_01" },
            { "wle_fp5_falloween_round_83", "current_wle_fp5_falloween_4_16" },
            { "wle_fp5_falloween_round_84", "current_wle_fp5_falloween_4_04" },
            { "wle_fp5_falloween_round_85", "current_wle_fp5_falloween_4_18" },
            { "wle_fp5_falloween_round_86", "current_wle_fp5_falloween_1_03" },
            { "wle_fp5_falloween_round_87", "current_wle_fp5_falloween_3_04" },
            { "wle_fp5_falloween_round_88", "current_wle_fp5_falloween_3_03" },
            { "wle_fp5_falloween_round_90", "current_wle_fp5_falloween_3_02" }
        };

        public readonly Dictionary<string, string> LevelIdReplacerInDigisShuffleShow = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
            { "wle_shuffle_halloween_1", "current_wle_fp5_falloween_7_01_01" },
            { "wle_shuffle_halloween_2", "current_wle_fp5_falloween_7_01_02" },
            { "wle_shuffle_halloween_3", "current_wle_fp5_falloween_7_02_01" },
            { "wle_shuffle_halloween_4", "current_wle_fp5_falloween_7_01_03" },
            { "wle_shuffle_halloween_5", "current_wle_fp5_falloween_7_01_05" },
            { "wle_shuffle_halloween_6", "current_wle_fp5_falloween_7_01_06" },
            { "wle_shuffle_halloween_7", "current_wle_fp5_falloween_7_01_07" },
            { "wle_shuffle_halloween_8", "current_wle_fp5_falloween_7_01_08" },
            { "wle_shuffle_halloween_9", "current_wle_fp5_falloween_7_01_09" },
            { "wle_shuffle_halloween_10", "current_wle_fp5_falloween_7_04_01" },
            { "wle_shuffle_halloween_12", "current_wle_fp5_falloween_7_10_01" },
            { "wle_shuffle_halloween_13", "current_wle_fp5_falloween_7_04_03" },
            { "wle_shuffle_halloween_14", "current_wle_fp5_falloween_7_04_04" },
            { "wle_shuffle_halloween_15", "current_wle_fp5_falloween_7_03_01" },
            { "wle_shuffle_halloween_16", "current_wle_fp5_falloween_7_05_01" },
            { "wle_shuffle_halloween_17", "current_wle_fp5_falloween_7_06_01" },
            { "wle_shuffle_halloween_18", "current_wle_fp5_falloween_7_05_02" },
            { "wle_shuffle_halloween_19", "current_wle_fp5_falloween_7_05_03" },
            { "wle_shuffle_halloween_20", "current_wle_fp5_falloween_7_06_02" },
            { "wle_shuffle_halloween_22", "current_wle_fp5_falloween_7_06_04" },
            { "wle_shuffle_halloween_23", "current_wle_fp5_falloween_7_06_05" },
            { "wle_shuffle_halloween_25", "current_wle_fp5_falloween_1_04" },
            { "wle_shuffle_halloween_26", "current_wle_fp5_falloween_1_12" },
            { "wle_shuffle_halloween_27", "current_wle_fp5_falloween_1_14" },
            { "wle_shuffle_halloween_29", "current_wle_fp5_falloween_1_11" },
            { "wle_shuffle_halloween_30", "current_wle_fp5_falloween_1_01" },
            { "wle_shuffle_halloween_31", "current_wle_fp5_falloween_1_05" },
            { "wle_shuffle_halloween_32", "current_wle_fp5_falloween_1_06" },
            { "wle_shuffle_halloween_33", "current_wle_fp5_falloween_2_01" },
            { "wle_shuffle_halloween_34", "current_wle_fp5_falloween_6_02" },
            { "wle_shuffle_halloween_35", "current_wle_fp5_falloween_6_03" },
            { "wle_shuffle_halloween_36", "current_wle_fp5_falloween_11_01" },
            { "wle_shuffle_halloween_37", "current_wle_fp5_falloween_2_02" },
            { "wle_shuffle_halloween_38", "current_wle_fp5_falloween_1_09" },
            { "wle_shuffle_halloween_40", "current_wle_fp5_falloween_1_08" },
            { "wle_shuffle_halloween_41", "current_wle_fp5_falloween_2_03" },
            { "wle_shuffle_halloween_42", "current_wle_fp5_falloween_4_03" },
            { "wle_shuffle_halloween_43", "current_wle_fp5_falloween_4_11" },
            { "wle_shuffle_halloween_44", "current_wle_fp5_falloween_4_08" },
            { "wle_shuffle_halloween_45", "current_wle_fp5_falloween_4_13" },
            { "wle_shuffle_halloween_47", "current_wle_fp5_falloween_4_12" },
            { "wle_shuffle_halloween_48", "current_wle_fp5_falloween_9_01" },
            { "wle_shuffle_halloween_49", "current_wle_fp5_falloween_2_03_01" },
            { "wle_shuffle_halloween_50", "current_wle_fp5_falloween_9_03" },
            { "wle_shuffle_halloween_51", "current_wle_fp5_falloween_9_02" },
            { "wle_shuffle_halloween_52", "current_wle_fp5_falloween_9_04" },
            { "wle_shuffle_halloween_53", "current_wle_fp5_falloween_2_03_02" },
            { "wle_shuffle_halloween_54", "current_wle_fp5_falloween_9_05" },
            { "wle_shuffle_halloween_55", "current_wle_fp5_falloween_2_03_03" },
            { "wle_shuffle_halloween_56", "current_wle_fp5_falloween_2_03_04" },
            { "wle_shuffle_halloween_57", "current_wle_fp5_falloween_2_03_05" },
            { "wle_shuffle_halloween_58", "current_wle_fp5_falloween_2_03_06" },
            { "wle_shuffle_halloween_59", "current_wle_fp5_falloween_10_01" },
            { "wle_shuffle_halloween_60", "current_wle_fp5_falloween_14_01" },
            { "wle_shuffle_halloween_61", "current_wle_fp5_falloween_10_02" },
            { "wle_shuffle_halloween_64", "current_wle_fp5_falloween_12_01" },
            { "wle_shuffle_halloween_65", "current_wle_fp5_falloween_12_02" },
            { "wle_shuffle_halloween_66", "current_wle_fp5_falloween_13_01" },
            { "wle_shuffle_halloween_67", "current_wle_fp5_falloween_4_02" },
            { "wle_shuffle_halloween_68", "current_wle_fp5_falloween_4_09" },
            { "wle_shuffle_halloween_69", "current_wle_fp5_falloween_4_19" },
            { "wle_shuffle_halloween_70", "current_wle_fp5_falloween_5_05" },
            { "wle_shuffle_halloween_71", "current_wle_fp5_falloween_5_04" },
            { "wle_shuffle_halloween_72", "current_wle_fp5_falloween_5_03" },
            { "wle_shuffle_halloween_73", "current_wle_fp5_falloween_5_07" },
            { "wle_shuffle_halloween_74", "current_wle_fp5_falloween_5_06" },
            { "wle_shuffle_halloween_75", "current_wle_fp5_falloween_4_10" },
            { "wle_shuffle_halloween_76", "current_wle_fp5_falloween_4_05" },
            { "wle_shuffle_halloween_77", "current_wle_fp5_falloween_4_15" },
            { "wle_shuffle_halloween_78", "current_wle_fp5_falloween_4_06" },
            { "wle_shuffle_halloween_79", "current_wle_fp5_falloween_4_17" },
            { "wle_shuffle_halloween_80", "current_wle_fp5_falloween_4_07" },
            { "wle_shuffle_halloween_81", "current_wle_fp5_falloween_4_14" },
            { "wle_shuffle_halloween_82", "current_wle_fp5_falloween_4_01" },
            { "wle_shuffle_halloween_83", "current_wle_fp5_falloween_4_16" },
            { "wle_shuffle_halloween_84", "current_wle_fp5_falloween_4_04" },
            { "wle_shuffle_halloween_85", "current_wle_fp5_falloween_4_18" },
            { "wle_shuffle_halloween_86", "current_wle_fp5_falloween_1_03" },
            { "wle_shuffle_halloween_87", "current_wle_fp5_falloween_3_04" },
            { "wle_shuffle_halloween_88", "current_wle_fp5_falloween_3_03" },
            { "wle_shuffle_halloween_90", "current_wle_fp5_falloween_3_02" }
        };

        private Stats() {
            this.StatsDB = new LiteDatabase(@"data.db");
            this.StatsDB.Pragma("UTC_DATE", true);
            this.UserSettings = this.StatsDB.GetCollection<UserSettings>("UserSettings");
            this.StatsDB.BeginTrans();
            if (this.UserSettings.Count() == 0) {
                this.CurrentSettings = this.GetDefaultSettings();
                this.UserSettings.Insert(this.CurrentSettings);
            } else {
                try {
                    this.CurrentSettings = this.UserSettings.FindAll().First();
                    CurrentLanguage = this.CurrentSettings.Multilingual;
                    CurrentTheme = this.CurrentSettings.Theme == 0 ? MetroThemeStyle.Light :
                                   this.CurrentSettings.Theme == 1 ? MetroThemeStyle.Dark : MetroThemeStyle.Default;
                } catch {
                    this.UserSettings.DeleteAll();
                    this.CurrentSettings = GetDefaultSettings();
                    this.UserSettings.Insert(this.CurrentSettings);
                }
            }
            this.StatsDB.Commit();

            this.RemoveUpdateFiles();

            this.InitializeComponent();

            this.BackMaxSize = 56;
            this.BackImage = this.Icon.ToBitmap();

            this.infoStrip.Renderer = new CustomToolStripSystemRenderer();
            this.infoStrip2.Renderer = new CustomToolStripSystemRenderer();
            DwmSetWindowAttribute(this.menu.Handle, DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE, ref windowConerPreference, sizeof(uint));
            DwmSetWindowAttribute(this.menuFilters.DropDown.Handle, DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE, ref windowConerPreference, sizeof(uint));
            DwmSetWindowAttribute(this.menuStatsFilter.DropDown.Handle, DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE, ref windowConerPreference, sizeof(uint));
            DwmSetWindowAttribute(this.menuPartyFilter.DropDown.Handle, DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE, ref windowConerPreference, sizeof(uint));
            DwmSetWindowAttribute(this.menuProfile.DropDown.Handle, DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE, ref windowConerPreference, sizeof(uint));

            this.RoundDetails = this.StatsDB.GetCollection<RoundInfo>("RoundDetails");
            this.Profiles = this.StatsDB.GetCollection<Profiles>("Profiles");

            this.StatsDB.BeginTrans();

            if (this.Profiles.Count() == 0) {
                using (SelectLanguage initLanguageForm = new SelectLanguage(CultureInfo.CurrentUICulture.Name.Substring(0, 2))) {
                    //initLanguageForm.Icon = this.Icon;
                    this.EnableInfoStrip(false);
                    this.EnableMainMenu(false);
                    if (initLanguageForm.ShowDialog(this) == DialogResult.OK) {
                        CurrentLanguage = initLanguageForm.selectedLanguage;
                        Overlay.SetDefaultFont(18, CurrentLanguage);
                        this.CurrentSettings.Multilingual = initLanguageForm.selectedLanguage;
                        if (initLanguageForm.autoGenerateProfiles) {
                            for (int i = this.publicShowIdList.Length; i >= 1; i--) {
                                string showId = this.publicShowIdList[i - 1];
                                this.Profiles.Insert(new Profiles { ProfileId = i - 1, ProfileName = Multilingual.GetShowName(showId), ProfileOrder = i, LinkedShowId = showId });
                            }
                            this.CurrentSettings.AutoChangeProfile = true;
                        } else {
                            this.Profiles.Insert(new Profiles { ProfileId = 3, ProfileName = Multilingual.GetWord("main_profile_custom"), ProfileOrder = 4, LinkedShowId = "private_lobbies" });
                            this.Profiles.Insert(new Profiles { ProfileId = 2, ProfileName = Multilingual.GetWord("main_profile_squad"), ProfileOrder = 3, LinkedShowId = "squads_4player" });
                            this.Profiles.Insert(new Profiles { ProfileId = 1, ProfileName = Multilingual.GetWord("main_profile_duo"), ProfileOrder = 2, LinkedShowId = "squads_2player_template" });
                            this.Profiles.Insert(new Profiles { ProfileId = 0, ProfileName = Multilingual.GetWord("main_profile_solo"), ProfileOrder = 1, LinkedShowId = "main_show" });
                        }
                    }
                    this.EnableInfoStrip(true);
                    this.EnableMainMenu(true);
                }
            }

            this.RoundDetails.EnsureIndex(x => x.Name);
            this.RoundDetails.EnsureIndex(x => x.ShowID);
            this.RoundDetails.EnsureIndex(x => x.Round);
            this.RoundDetails.EnsureIndex(x => x.Start);
            this.RoundDetails.EnsureIndex(x => x.InParty);
            this.StatsDB.Commit();

            this.UpdateDatabaseVersion();

            foreach (KeyValuePair<string, LevelStats> entry in LevelStats.ALL) {
                this.StatLookup.Add(entry.Key, entry.Value);
                this.StatDetails.Add(entry.Value);
            }

            this.ChangeMainLanguage();
            this.InitMainDataGridView();
            this.UpdateGridRoundName();

            this.UpdateHoopsieLegends();

            this.CurrentRound = new List<RoundInfo>();

            this.overlay = new Overlay { Text = @"Fall Guys Stats Overlay", StatsForm = this, Icon = this.Icon, ShowIcon = true, BackgroundResourceName = this.CurrentSettings.OverlayBackgroundResourceName, TabResourceName = this.CurrentSettings.OverlayTabResourceName };

            Screen screen = this.GetCurrentScreen(this.overlay.Location);
            Point screenLocation = screen != null ? screen.Bounds.Location : Screen.PrimaryScreen.Bounds.Location;
            Size screenSize = screen != null ? screen.Bounds.Size : Screen.PrimaryScreen.Bounds.Size;
            this.screenCenter = new Point(screenLocation.X + (screenSize.Width / 2), screenLocation.Y + (screenSize.Height / 2));

            this.logFile.OnParsedLogLines += this.LogFile_OnParsedLogLines;
            this.logFile.OnNewLogFileDate += this.LogFile_OnNewLogFileDate;
            this.logFile.OnError += this.LogFile_OnError;
            this.logFile.OnParsedLogLinesCurrent += this.LogFile_OnParsedLogLinesCurrent;
            this.logFile.StatsForm = this;
            this.logFile.autoChangeProfile = this.CurrentSettings.AutoChangeProfile;
            this.logFile.preventOverlayMouseClicks = this.CurrentSettings.PreventOverlayMouseClicks;

            string fixedPosition = this.CurrentSettings.OverlayFixedPosition;
            this.overlay.SetFixedPosition(
                !string.IsNullOrEmpty(fixedPosition) && fixedPosition.Equals("ne"),
                !string.IsNullOrEmpty(fixedPosition) && fixedPosition.Equals("nw"),
                !string.IsNullOrEmpty(fixedPosition) && fixedPosition.Equals("se"),
                !string.IsNullOrEmpty(fixedPosition) && fixedPosition.Equals("sw"),
                !string.IsNullOrEmpty(fixedPosition) && fixedPosition.Equals("free")
            );
            if (this.overlay.IsFixed()) this.overlay.Cursor = Cursors.Default;
            this.overlay.Opacity = this.CurrentSettings.OverlayBackgroundOpacity / 100D;
            this.overlay.Show();
            this.overlay.Hide();
            this.overlay.StartTimer();

            this.ReloadProfileMenuItems();

            this.UpdateGameExeLocation();

            this.SaveUserSettings();

            this.SetTheme(CurrentTheme);

            this.cmtt.OwnerDraw = true;
            this.cmtt.Draw += this.Cmtt_Draw;
        }

        [DllImport("User32.dll")]
        static extern bool MoveWindow(IntPtr h, int x, int y, int width, int height, bool redraw);
        private void Cmtt_Draw(object sender, DrawToolTipEventArgs e) {
            // Draw the standard background.
            //e.DrawBackground();
            // Draw the custom background.
            e.Graphics.FillRectangle(Brushes.WhiteSmoke, e.Bounds);

            // Draw the standard border.
            e.DrawBorder();
            // Draw the custom border to appear 3-dimensional.
            //e.Graphics.DrawLines(SystemPens.ControlLightLight, new[] {
            //    new Point (0, e.Bounds.Height - 1), 
            //    new Point (0, 0), 
            //    new Point (e.Bounds.Width - 1, 0)
            //});
            //e.Graphics.DrawLines(SystemPens.ControlDarkDark, new[] {
            //    new Point (0, e.Bounds.Height - 1), 
            //    new Point (e.Bounds.Width - 1, e.Bounds.Height - 1), 
            //    new Point (e.Bounds.Width - 1, 0)
            //});

            // Draw the standard text with customized formatting options.
            e.DrawText(TextFormatFlags.TextBoxControl | TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.WordBreak | TextFormatFlags.LeftAndRightPadding);
            // Draw the custom text.
            // The using block will dispose the StringFormat automatically.
            //using (StringFormat sf = new StringFormat()) {
            //    sf.Alignment = StringAlignment.Near;
            //    sf.LineAlignment = StringAlignment.Near;
            //    sf.HotkeyPrefix = System.Drawing.Text.HotkeyPrefix.None;
            //    sf.FormatFlags = StringFormatFlags.NoWrap;
            //    e.Graphics.DrawString(e.ToolTipText, Overlay.GetMainFont(12), SystemBrushes.ActiveCaptionText, e.Bounds, sf);
            //    //using (Font f = new Font("Tahoma", 9)) {
            //    //    e.Graphics.DrawString(e.ToolTipText, f, SystemBrushes.ActiveCaptionText, e.Bounds, sf);
            //    //}
            //}

            MetroToolTip t = (MetroToolTip)sender;
            PropertyInfo h = t.GetType().GetProperty("Handle", BindingFlags.NonPublic | BindingFlags.Instance);
            IntPtr handle = (IntPtr)h.GetValue(t);
            Control c = e.AssociatedControl;
            if (c != null && c.Parent != null) {
                Point location = c.Parent.PointToScreen(new Point(c.Right - e.Bounds.Width, c.Bottom));
                MoveWindow(handle, location.X, location.Y, e.Bounds.Width, e.Bounds.Height, false);
            }
        }
        //private void mtt_Draw(object sender, DrawToolTipEventArgs e) {
        //    e.DrawBackground();
        //    e.DrawBorder();
        //    e.DrawText();
        //    MetroToolTip t = (MetroToolTip)sender;
        //    PropertyInfo h = t.GetType().GetProperty("Handle", BindingFlags.NonPublic | BindingFlags.Instance);
        //    IntPtr handle = (IntPtr)h.GetValue(t);
        //    Control c = e.AssociatedControl;
        //    Point location = c.Parent.PointToScreen(new Point(c.Right - e.Bounds.Width, c.Bottom));
        //    MoveWindow(handle, location.X, location.Y, e.Bounds.Width, e.Bounds.Height, false);
        //}

        public class CustomToolStripSystemRenderer : ToolStripSystemRenderer {
            protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e) {
                //base.OnRenderToolStripBorder(e);
            }
        }

        public class CustomLightArrowRenderer : ToolStripProfessionalRenderer {
            public CustomLightArrowRenderer() : base(new CustomLightColorTable()) { }
            protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e) {
                //var tsMenuItem = e.Item as ToolStripMenuItem;
                //if (tsMenuItem != null) e.ArrowColor = CurrentTheme == MetroThemeStyle.Dark ? Color.DarkGray : Color.FromArgb(17, 17, 17);
                //Point point = new Point(e.ArrowRectangle.Left + e.ArrowRectangle.Width / 2, e.ArrowRectangle.Top + e.ArrowRectangle.Height / 2);
                //Point[] points = new Point[3]
                //{
                //    new Point(point.X - 2, point.Y - 4),
                //    new Point(point.X - 2, point.Y + 4),
                //    new Point(point.X + 2, point.Y)
                //};
                //e.Graphics.FillPolygon(Brushes.DarkGray, points);
                e.ArrowColor = Color.FromArgb(17, 17, 17);
                base.OnRenderArrow(e);
            }
        }

        public class CustomDarkArrowRenderer : ToolStripProfessionalRenderer {
            public CustomDarkArrowRenderer() : base(new CustomDarkColorTable()) { }
            protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e) {
                //var tsMenuItem = e.Item as ToolStripMenuItem;
                //if (tsMenuItem != null) e.ArrowColor = CurrentTheme == MetroThemeStyle.Dark ? Color.DarkGray : Color.FromArgb(17, 17, 17);
                //Point point = new Point(e.ArrowRectangle.Left + e.ArrowRectangle.Width / 2, e.ArrowRectangle.Top + e.ArrowRectangle.Height / 2);
                //Point[] points = new Point[3]
                //{
                //    new Point(point.X - 2, point.Y - 4),
                //    new Point(point.X - 2, point.Y + 4),
                //    new Point(point.X + 2, point.Y)
                //};
                //e.Graphics.FillPolygon(Brushes.DarkGray, points);
                e.ArrowColor = Color.DarkGray;
                base.OnRenderArrow(e);
            }
        }

        private class CustomLightColorTable : ProfessionalColorTable {
            public CustomLightColorTable() { UseSystemColors = false; }
            //public override Color ToolStripBorder {
            //    get { return Color.Red; }
            //}
            public override Color MenuBorder {
                get { return Color.White; }
            }
            public override Color ToolStripDropDownBackground {
                get { return Color.White; }
            }
            public override Color MenuItemBorder {
                get { return Color.DarkSeaGreen; }
            }
            public override Color MenuItemSelected {
                get { return Color.LightGreen; }
            }
            //public override Color MenuItemSelectedGradientBegin {
            //    get { return Color.LawnGreen; }
            //}
            //public override Color MenuItemSelectedGradientEnd {
            //    get { return Color.MediumSeaGreen; }
            //}
            //public override Color MenuStripGradientBegin {
            //    get { return Color.AliceBlue; }
            //}
            //public override Color MenuStripGradientEnd {
            //    get { return Color.DodgerBlue; }
            //}
        }

        private class CustomDarkColorTable : ProfessionalColorTable {
            public CustomDarkColorTable() { UseSystemColors = false; }
            //public override Color ToolStripBorder {
            //    get { return Color.Red; }
            //}
            public override Color MenuBorder {
                get { return Color.FromArgb(17, 17, 17); }
            }
            public override Color ToolStripDropDownBackground {
                get { return Color.FromArgb(17, 17, 17); }
            }
            public override Color MenuItemBorder {
                get { return Color.DarkSeaGreen; }
            }
            public override Color MenuItemSelected {
                get { return Color.LightGreen; }
            }
            //public override Color MenuItemSelectedGradientBegin {
            //    get { return Color.LawnGreen; }
            //}
            //public override Color MenuItemSelectedGradientEnd {
            //    get { return Color.MediumSeaGreen; }
            //}
            //public override Color MenuStripGradientBegin {
            //    get { return Color.AliceBlue; }
            //}
            //public override Color MenuStripGradientEnd {
            //    get { return Color.DodgerBlue; }
            //}
        }

        public sealed override string Text {
            get { return base.Text; }
            set { base.Text = value; }
        }

        private void SetTheme(MetroThemeStyle theme) {
            if (this.Theme == theme) { return; }

            this.SuspendLayout();
            this.Theme = theme;
            this.menu.Renderer = theme == MetroThemeStyle.Light ? (ToolStripRenderer)new CustomLightArrowRenderer() : new CustomDarkArrowRenderer();
            foreach (Control c1 in Controls) {
                if (c1 is MetroLabel ml1) {
                    ml1.Theme = theme;
                }
                if (c1 is MenuStrip ms1) {
                    foreach (ToolStripMenuItem tsmi1 in ms1.Items) {
                        switch (tsmi1.Name) {
                            case "menuSettings":
                                tsmi1.Image = theme == MetroThemeStyle.Light ? Properties.Resources.setting_icon : Properties.Resources.setting_gray_icon;
                                break;
                            case "menuFilters":
                                tsmi1.Image = theme == MetroThemeStyle.Light ? Properties.Resources.filter_icon : Properties.Resources.filter_gray_icon;
                                break;
                            case "menuProfile":
                                tsmi1.Image = theme == MetroThemeStyle.Light ? Properties.Resources.profile_icon : Properties.Resources.profile_gray_icon;
                                break;
                            //case "menuOverlay": break;
                            case "menuUpdate":
                            case "menuHelp":
                                tsmi1.Image = theme == MetroThemeStyle.Light ? Properties.Resources.github_icon : Properties.Resources.github_gray_icon;
                                break;
                                //case "menuLaunchFallGuys": break;
                        }
                        tsmi1.ForeColor = theme == MetroThemeStyle.Light ? Color.Black : Color.DarkGray;
                        tsmi1.MouseEnter += this.Menu_MouseEnter;
                        tsmi1.MouseLeave += this.Menu_MouseLeave;
                        foreach (var item1 in tsmi1.DropDownItems) {
                            if (item1 is ToolStripMenuItem subTsmi1) {
                                if (subTsmi1.Name.Equals("menuEditProfiles")) { subTsmi1.Image = theme == MetroThemeStyle.Light ? Properties.Resources.setting_icon : Properties.Resources.setting_gray_icon; }
                                subTsmi1.ForeColor = theme == MetroThemeStyle.Light ? Color.Black : Color.DarkGray;
                                subTsmi1.BackColor = theme == MetroThemeStyle.Light ? Color.White : Color.FromArgb(17, 17, 17);
                                subTsmi1.MouseEnter += this.Menu_MouseEnter;
                                subTsmi1.MouseLeave += this.Menu_MouseLeave;
                                foreach (var item2 in subTsmi1.DropDownItems) {
                                    if (item2 is ToolStripMenuItem subTsmi2) {
                                        if (subTsmi2.Name.Equals("menuCustomRangeStats")) { subTsmi2.Image = theme == MetroThemeStyle.Light ? Properties.Resources.calendar_icon : Properties.Resources.calendar_gray_icon; }
                                        subTsmi2.ForeColor = theme == MetroThemeStyle.Light ? Color.Black : Color.DarkGray;
                                        subTsmi2.BackColor = theme == MetroThemeStyle.Light ? Color.White : Color.FromArgb(17, 17, 17);
                                        subTsmi2.MouseEnter += this.Menu_MouseEnter;
                                        subTsmi2.MouseLeave += this.Menu_MouseLeave;
                                    } else if (item2 is ToolStripSeparator subTss2) {
                                        subTss2.Paint += this.CustomToolStripSeparatorCustom_Paint;
                                        subTss2.BackColor = theme == MetroThemeStyle.Light ? Color.White : Color.FromArgb(17, 17, 17);
                                        subTss2.ForeColor = theme == MetroThemeStyle.Light ? Color.Black : Color.DarkGray;
                                    }
                                }
                            } else if (item1 is ToolStripSeparator subTss1) {
                                subTss1.Paint += this.CustomToolStripSeparatorCustom_Paint;
                                subTss1.BackColor = theme == MetroThemeStyle.Light ? Color.White : Color.FromArgb(17, 17, 17);
                                subTss1.ForeColor = theme == MetroThemeStyle.Light ? Color.Black : Color.DarkGray;
                            }
                        }
                    }
                } else if (c1 is ToolStrip ts1) {
                    ts1.BackColor = Color.Transparent;
                    foreach (ToolStripLabel tsl1 in ts1.Items) {
                        switch (tsl1.Name) {
                            case "lblCurrentProfile":
                                tsl1.ForeColor = theme == MetroThemeStyle.Light ? Color.Red : Color.FromArgb(0, 192, 192);
                                break;
                            case "lblTotalTime":
                                tsl1.Image = theme == MetroThemeStyle.Light ? Properties.Resources.clock_icon : Properties.Resources.clock_gray_icon;
                                tsl1.ForeColor = theme == MetroThemeStyle.Light ? Color.Blue : Color.Orange;
                                //tsl1.ForeColor = theme == MetroThemeStyle.Light ? Color.DarkSlateGray : Color.DarkGray;
                                break;
                            case "lblTotalShows":
                            case "lblTotalWins":
                                tsl1.ForeColor = theme == MetroThemeStyle.Light ? Color.Blue : Color.Orange;
                                break;
                            case "lblTotalRounds":
                                tsl1.Image = theme == MetroThemeStyle.Light ? Properties.Resources.round_icon : Properties.Resources.round_gray_icon;
                                tsl1.ForeColor = theme == MetroThemeStyle.Light ? Color.Blue : Color.Orange;
                                break;
                            case "lblTotalFinals":
                                tsl1.Image = theme == MetroThemeStyle.Light ? Properties.Resources.final_icon : Properties.Resources.final_gray_icon;
                                tsl1.ForeColor = theme == MetroThemeStyle.Light ? Color.Blue : Color.Orange;
                                break;
                            case "lblGoldMedal":
                            case "lblSilverMedal":
                            case "lblBronzeMedal":
                            case "lblPinkMedal":
                            case "lblEliminatedMedal":
                            case "lblKudos":
                                tsl1.ForeColor = theme == MetroThemeStyle.Light ? Color.DarkSlateGray : Color.DarkGray;
                                break;
                        }
                    }
                }
            }

            foreach (object item in this.gridDetails.CMenu.Items) {
                if (item is ToolStripMenuItem tsi) {
                    tsi.BackColor = theme == MetroThemeStyle.Light ? Color.White : Color.FromArgb(17, 17, 17);
                    tsi.ForeColor = theme == MetroThemeStyle.Light ? Color.Black : Color.DarkGray;
                    tsi.MouseEnter += this.CMenu_MouseEnter;
                    tsi.MouseLeave += this.CMenu_MouseLeave;
                    switch (tsi.Name) {
                        case "exportItemCSV":
                        case "exportItemHTML":
                        case "exportItemBBCODE":
                        case "exportItemMD":
                            tsi.Image = theme == MetroThemeStyle.Light ? Properties.Resources.export : Properties.Resources.export_gray;
                            break;
                    }
                }
            }

            this.dataGridViewCellStyle1.BackColor = theme == MetroThemeStyle.Light ? Color.LightGray : Color.FromArgb(2, 2, 2);
            this.dataGridViewCellStyle1.ForeColor = theme == MetroThemeStyle.Light ? Color.Black : Color.DarkGray;
            this.dataGridViewCellStyle1.SelectionBackColor = theme == MetroThemeStyle.Light ? Color.Cyan : Color.DarkSlateBlue;
            //this.dataGridViewCellStyle1.SelectionForeColor = Color.Black;
            this.dataGridViewCellStyle2.BackColor = theme == MetroThemeStyle.Light ? Color.White : Color.FromArgb(49, 51, 56);
            this.dataGridViewCellStyle2.ForeColor = theme == MetroThemeStyle.Light ? Color.Black : Color.WhiteSmoke;
            this.dataGridViewCellStyle2.SelectionBackColor = theme == MetroThemeStyle.Light ? Color.DeepSkyBlue : Color.PaleGreen;
            this.dataGridViewCellStyle2.SelectionForeColor = Color.Black;

            this.ResumeLayout(false);
            this.Refresh();
        }
        private void CustomToolStripSeparatorCustom_Paint(Object sender, PaintEventArgs e) {
            ToolStripSeparator separator = (ToolStripSeparator)sender;
            e.Graphics.FillRectangle(new SolidBrush(this.Theme == MetroThemeStyle.Light ? Color.White : Color.FromArgb(17, 17, 17)), 0, 0, separator.Width, separator.Height); // CUSTOM_COLOR_BACKGROUND
            e.Graphics.DrawLine(new Pen(this.Theme == MetroThemeStyle.Light ? Color.Black : Color.DarkGray), 30, separator.Height / 2, separator.Width - 4, separator.Height / 2); // CUSTOM_COLOR_FOREGROUND
        }
        private void CMenu_MouseEnter(object sender, EventArgs e) {
            if (sender is ToolStripMenuItem tsi) {
                tsi.ForeColor = Color.Black;
                switch (tsi.Name) {
                    case "exportItemCSV":
                    case "exportItemHTML":
                    case "exportItemBBCODE":
                    case "exportItemMD":
                        tsi.Image = Properties.Resources.export;
                        break;
                }
            }
        }
        private void CMenu_MouseLeave(object sender, EventArgs e) {
            if (sender is ToolStripMenuItem tsi) {
                tsi.ForeColor = this.Theme == MetroThemeStyle.Light ? Color.Black : Color.DarkGray;
                switch (tsi.Name) {
                    case "exportItemCSV":
                    case "exportItemHTML":
                    case "exportItemBBCODE":
                    case "exportItemMD":
                        tsi.Image = this.Theme == MetroThemeStyle.Light ? Properties.Resources.export : Properties.Resources.export_gray;
                        break;
                }
            }
        }
        private void Menu_MouseEnter(object sender, EventArgs e) {
            ToolStripMenuItem tsmi = sender as ToolStripMenuItem;
            switch (tsmi.Name) {
                case "menuSettings":
                    tsmi.Image = Properties.Resources.setting_icon;
                    break;
                case "menuFilters":
                    tsmi.Image = Properties.Resources.filter_icon;
                    break;
                case "menuCustomRangeStats":
                    tsmi.Image = Properties.Resources.calendar_icon;
                    break;
                case "menuProfile":
                    tsmi.Image = Properties.Resources.profile_icon;
                    break;
                //case "menuOverlay": break;
                case "menuUpdate":
                case "menuHelp":
                    tsmi.Image = Properties.Resources.github_icon;
                    break;
                //case "menuLaunchFallGuys": break;
                case "menuEditProfiles":
                    tsmi.Image = Properties.Resources.setting_icon;
                    break;
            }
            tsmi.ForeColor = Color.Black;
        }
        private void Menu_MouseLeave(object sender, EventArgs e) {
            ToolStripMenuItem tsmi = sender as ToolStripMenuItem;
            switch (tsmi.Name) {
                case "menuSettings":
                    tsmi.Image = this.Theme == MetroThemeStyle.Light ? Properties.Resources.setting_icon : Properties.Resources.setting_gray_icon;
                    break;
                case "menuFilters":
                    tsmi.Image = this.Theme == MetroThemeStyle.Light ? Properties.Resources.filter_icon : Properties.Resources.filter_gray_icon;
                    break;
                case "menuCustomRangeStats":
                    tsmi.Image = this.Theme == MetroThemeStyle.Light ? Properties.Resources.calendar_icon : Properties.Resources.calendar_gray_icon;
                    break;
                case "menuProfile":
                    tsmi.Image = this.Theme == MetroThemeStyle.Light ? Properties.Resources.profile_icon : Properties.Resources.profile_gray_icon;
                    break;
                //case "menuOverlay": break;
                case "menuUpdate":
                case "menuHelp":
                    tsmi.Image = this.Theme == MetroThemeStyle.Light ? Properties.Resources.github_icon : Properties.Resources.github_gray_icon;
                    break;
                //case "menuLaunchFallGuys": break;
                case "menuEditProfiles":
                    tsmi.Image = this.Theme == MetroThemeStyle.Light ? Properties.Resources.setting_icon : Properties.Resources.setting_gray_icon;
                    break;
            }
            tsmi.ForeColor = this.Theme == MetroThemeStyle.Dark ? Color.DarkGray : Color.Black;
        }
        private void InfoStrip_MouseEnter(object sender, EventArgs e) {
            if (sender is ToolStripLabel lblInfo) {
                //this.infoStripForeColor = lblInfo.ForeColor;
                this.Cursor = Cursors.Hand;
                this.infoStripForeColor = lblInfo.Name.Equals("lblCurrentProfile")
                                          ? this.Theme == MetroThemeStyle.Light ? Color.Red : Color.FromArgb(0, 192, 192)
                                          : this.Theme == MetroThemeStyle.Light ? Color.Blue : Color.Orange;

                lblInfo.ForeColor = lblInfo.Name.Equals("lblCurrentProfile")
                                    ? this.Theme == MetroThemeStyle.Light ? Color.FromArgb(245, 154, 168) : Color.FromArgb(231, 251, 255)
                                    : this.Theme == MetroThemeStyle.Light ? Color.FromArgb(147, 174, 248) : Color.FromArgb(255, 250, 244);
            }
        }
        private void InfoStrip_MouseLeave(object sender, EventArgs e) {
            this.Cursor = Cursors.Default;
            if (sender is ToolStripLabel lblInfo) {
                lblInfo.ForeColor = this.infoStripForeColor;
            }
        }

        public void ReloadProfileMenuItems() {
            this.ProfileMenuItems.Clear();
            this.menuProfile.DropDownItems.Clear();
            this.menuProfile.DropDownItems.Add(this.menuEditProfiles);
            this.AllProfiles.Clear();
            this.AllProfiles.AddRange(this.Profiles.FindAll());
            int profileNumber = 0;
            for (int i = this.AllProfiles.Count - 1; i >= 0; i--) {
                Profiles profile = this.AllProfiles[i];
                ToolStripMenuItem menuItem = new ToolStripMenuItem {
                    Checked = this.CurrentSettings.SelectedProfile == profile.ProfileId,
                    CheckOnClick = true,
                    CheckState = this.CurrentSettings.SelectedProfile == profile.ProfileId ? CheckState.Checked : CheckState.Unchecked,
                    Name = "menuProfile" + profile.ProfileId
                };
                switch (profileNumber++) {
                    case 0: menuItem.Image = this.numberOne; break;
                    case 1: menuItem.Image = this.numberTwo; break;
                    case 2: menuItem.Image = this.numberThree; break;
                    case 3: menuItem.Image = this.numberFour; break;
                    case 4: menuItem.Image = this.numberFive; break;
                    case 5: menuItem.Image = this.numberSix; break;
                    case 6: menuItem.Image = this.numberSeven; break;
                    case 7: menuItem.Image = this.numberEight; break;
                    case 8: menuItem.Image = this.numberNine; break;
                }
                menuItem.ForeColor = this.Theme == MetroThemeStyle.Light ? Color.Black : Color.DarkGray;
                menuItem.BackColor = this.Theme == MetroThemeStyle.Light ? Color.White : Color.FromArgb(17, 17, 17);
                menuItem.Size = new Size(180, 22);
                menuItem.Text = profile.ProfileName;
                menuItem.Click += this.MenuStats_Click;
                menuItem.Paint += this.MenuProfile_Paint;
                menuItem.MouseMove += this.SetCursor_MouseMove;
                menuItem.MouseLeave += this.SetCursor_MouseLeave;
                this.menuProfile.DropDownItems.Add(menuItem);
                this.ProfileMenuItems.Add(menuItem);
                if (this.CurrentSettings.SelectedProfile == profile.ProfileId) {
                    this.SetCurrentProfileIcon(!string.IsNullOrEmpty(profile.LinkedShowId));
                    this.MenuStats_Click(menuItem, EventArgs.Empty);
                }
            }
        }

        private void MenuProfile_Paint(object sender, PaintEventArgs e) {
            if (this.AllProfiles.FindIndex(profile => profile.ProfileId.ToString().Equals(((ToolStripMenuItem)sender).Name.Substring(11)) && !string.IsNullOrEmpty(profile.LinkedShowId)) != -1) {
                e.Graphics.DrawImage(this.CurrentSettings.AutoChangeProfile ? Properties.Resources.link_on_icon :
                                     this.Theme == MetroThemeStyle.Light ? Properties.Resources.link_icon :
                                     Properties.Resources.link_gray_icon, 24, 5, 11, 11);
            }
        }

        private void RemoveUpdateFiles() {
#if AllowUpdate
            string filePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            foreach (string file in Directory.EnumerateFiles(filePath, "*.bak")) {
                try {
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                } catch { }
            }
#endif
        }

        private void UpdateDatabaseVersion() {
            if (!this.CurrentSettings.UpdatedDateFormat) {
                this.AllStats.AddRange(this.RoundDetails.FindAll());
                this.StatsDB.BeginTrans();
                for (int i = this.AllStats.Count - 1; i >= 0; i--) {
                    RoundInfo info = this.AllStats[i];
                    info.Start = DateTime.SpecifyKind(info.Start.ToLocalTime(), DateTimeKind.Utc);
                    info.End = DateTime.SpecifyKind(info.End.ToLocalTime(), DateTimeKind.Utc);
                    info.Finish = info.Finish.HasValue ? DateTime.SpecifyKind(info.Finish.Value.ToLocalTime(), DateTimeKind.Utc) : (DateTime?)null;
                    this.RoundDetails.Update(info);
                }
                this.StatsDB.Commit();
                this.AllStats.Clear();
                this.CurrentSettings.UpdatedDateFormat = true;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.Version == 0) {
                this.CurrentSettings.SwitchBetweenQualify = this.CurrentSettings.SwitchBetweenLongest;
                this.CurrentSettings.Version = 1;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.Version == 1) {
                this.CurrentSettings.SwitchBetweenPlayers = this.CurrentSettings.SwitchBetweenLongest;
                this.CurrentSettings.Version = 2;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.Version == 2) {
                this.CurrentSettings.SwitchBetweenStreaks = this.CurrentSettings.SwitchBetweenLongest;
                this.CurrentSettings.Version = 3;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.Version == 3 || this.CurrentSettings.Version == 4) {
                this.AllStats.AddRange(this.RoundDetails.FindAll());
                this.StatsDB.BeginTrans();
                for (int i = this.AllStats.Count - 1; i >= 0; i--) {
                    RoundInfo info = this.AllStats[i];

                    int index;
                    if ((index = info.Name.IndexOf("_variation", StringComparison.OrdinalIgnoreCase)) > 0) {
                        info.Name = info.Name.Substring(0, index);
                        this.RoundDetails.Update(info);
                    }
                }
                this.StatsDB.Commit();
                this.AllStats.Clear();
                this.CurrentSettings.Version = 5;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.Version == 5 || this.CurrentSettings.Version == 6) {
                this.AllStats.AddRange(this.RoundDetails.FindAll());
                this.StatsDB.BeginTrans();
                for (int i = this.AllStats.Count - 1; i >= 0; i--) {
                    RoundInfo info = this.AllStats[i];

                    int index;
                    if ((index = info.Name.IndexOf("_northernlion", StringComparison.OrdinalIgnoreCase)) > 0) {
                        info.Name = info.Name.Substring(0, index);
                        RoundDetails.Update(info);
                    }
                }
                this.StatsDB.Commit();
                this.AllStats.Clear();
                this.CurrentSettings.Version = 7;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.Version == 7) {
                this.AllStats.AddRange(this.RoundDetails.FindAll());
                this.StatsDB.BeginTrans();
                for (int i = this.AllStats.Count - 1; i >= 0; i--) {
                    RoundInfo info = this.AllStats[i];

                    int index;
                    if ((index = info.Name.IndexOf("_hard_mode", StringComparison.OrdinalIgnoreCase)) > 0) {
                        info.Name = info.Name.Substring(0, index);
                        this.RoundDetails.Update(info);
                    }
                }
                this.StatsDB.Commit();
                this.AllStats.Clear();
                this.CurrentSettings.Version = 8;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.Version == 8) {
                this.AllStats.AddRange(this.RoundDetails.FindAll());
                this.StatsDB.BeginTrans();
                for (int i = this.AllStats.Count - 1; i >= 0; i--) {
                    RoundInfo info = this.AllStats[i];

                    int index;
                    if ((index = info.Name.IndexOf("_event_", StringComparison.OrdinalIgnoreCase)) > 0) {
                        info.Name = info.Name.Substring(0, index);
                        this.RoundDetails.Update(info);
                    }
                }
                this.StatsDB.Commit();
                this.AllStats.Clear();
                this.CurrentSettings.Version = 9;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.Version == 9) {
                this.AllStats.AddRange(this.RoundDetails.FindAll());
                this.StatsDB.BeginTrans();
                for (int i = this.AllStats.Count - 1; i >= 0; i--) {
                    RoundInfo info = this.AllStats[i];

                    if (info.Name.Equals("round_fall_mountain", StringComparison.OrdinalIgnoreCase)) {
                        info.Name = "round_fall_mountain_hub_complete";
                        this.RoundDetails.Update(info);
                    }
                }
                this.StatsDB.Commit();
                this.AllStats.Clear();
                this.CurrentSettings.Version = 10;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.Version == 10) {
                this.AllStats.AddRange(this.RoundDetails.FindAll());
                this.StatsDB.BeginTrans();
                for (int i = this.AllStats.Count - 1; i >= 0; i--) {
                    RoundInfo info = this.AllStats[i];

                    int index;
                    if ((index = info.Name.IndexOf("_event_", StringComparison.OrdinalIgnoreCase)) > 0
                        || (index = info.Name.IndexOf(". D", StringComparison.OrdinalIgnoreCase)) > 0) {
                        info.Name = info.Name.Substring(0, index);
                        this.RoundDetails.Update(info);
                    }
                }
                this.StatsDB.Commit();
                this.AllStats.Clear();
                this.CurrentSettings.Version = 11;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.Version == 11) {
                this.AllStats.AddRange(this.RoundDetails.FindAll());
                this.AllStats.Sort();
                this.StatsDB.BeginTrans();
                int lastShow = -1;
                for (int i = this.AllStats.Count - 1; i >= 0; i--) {
                    RoundInfo info = this.AllStats[i];

                    if (lastShow != info.ShowID) {
                        lastShow = info.ShowID;
                        info.IsFinal = this.StatLookup.TryGetValue(info.Name, out LevelStats stats)
                            ? stats.IsFinal && (info.Name != "round_floor_fall" || info.Round >= 3 || (i > 0 && this.AllStats[i - 1].Name != "round_floor_fall"))
                            : false;
                    } else {
                        info.IsFinal = false;
                    }

                    this.RoundDetails.Update(info);
                }
                this.StatsDB.Commit();
                this.AllStats.Clear();
                this.CurrentSettings.Version = 12;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.Version == 12 || this.CurrentSettings.Version == 13) {
                this.AllStats.AddRange(this.RoundDetails.FindAll());
                this.StatsDB.BeginTrans();
                for (int i = this.AllStats.Count - 1; i >= 0; i--) {
                    RoundInfo info = this.AllStats[i];

                    if (info.Name.IndexOf("round_fruitpunch", StringComparison.OrdinalIgnoreCase) == 0) {
                        info.Name = "round_fruitpunch_s4_show";
                        this.RoundDetails.Update(info);
                    } else if (info.Name.IndexOf("round_hoverboardsurvival", StringComparison.OrdinalIgnoreCase) == 0) {
                        info.Name = "round_hoverboardsurvival_s4_show";
                        this.RoundDetails.Update(info);
                    } else if (info.Name.IndexOf("round_basketfall", StringComparison.OrdinalIgnoreCase) == 0) {
                        info.Name = "round_basketfall_s4_show";
                        this.RoundDetails.Update(info);
                    } else if (info.Name.IndexOf("round_territory", StringComparison.OrdinalIgnoreCase) == 0) {
                        info.Name = "round_territory_control_s4_show";
                        this.RoundDetails.Update(info);
                    } else if (info.Name.IndexOf("round_shortcircuit", StringComparison.OrdinalIgnoreCase) == 0) {
                        info.Name = "round_shortcircuit";
                        this.RoundDetails.Update(info);
                    } else if (info.Name.IndexOf("round_gauntlet_06", StringComparison.OrdinalIgnoreCase) == 0) {
                        info.Name = "round_gauntlet_06";
                        this.RoundDetails.Update(info);
                    } else if (info.Name.IndexOf("round_tunnel_race", StringComparison.OrdinalIgnoreCase) == 0) {
                        info.Name = "round_tunnel_race";
                        this.RoundDetails.Update(info);
                    } else if (info.Name.IndexOf("round_1v1_button", StringComparison.OrdinalIgnoreCase) == 0) {
                        info.Name = "round_1v1_button_basher";
                        this.RoundDetails.Update(info);
                    } else if (info.Name.IndexOf("round_slimeclimb", StringComparison.OrdinalIgnoreCase) == 0) {
                        info.Name = "round_slimeclimb_2";
                        this.RoundDetails.Update(info);
                    }
                }
                this.StatsDB.Commit();
                this.AllStats.Clear();
                this.CurrentSettings.Version = 14;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.Version == 14) {
                this.AllStats.AddRange(this.RoundDetails.FindAll());
                this.StatsDB.BeginTrans();
                for (int i = AllStats.Count - 1; i >= 0; i--) {
                    RoundInfo info = AllStats[i];

                    if (info.Name.IndexOf("round_king_of", StringComparison.OrdinalIgnoreCase) == 0) {
                        info.Name = "round_king_of_the_hill";
                        this.RoundDetails.Update(info);
                    } else if (info.Name.IndexOf("round_drumtop", StringComparison.OrdinalIgnoreCase) == 0) {
                        info.Name = "round_drumtop";
                        this.RoundDetails.Update(info);
                    } else if (info.Name.IndexOf("round_penguin_solos", StringComparison.OrdinalIgnoreCase) == 0) {
                        info.Name = "round_penguin_solos";
                        this.RoundDetails.Update(info);
                    } else if (info.Name.IndexOf("round_gauntlet_07", StringComparison.OrdinalIgnoreCase) == 0) {
                        info.Name = "round_gauntlet_07";
                        this.RoundDetails.Update(info);
                    } else if (info.Name.IndexOf("round_robotrampage", StringComparison.OrdinalIgnoreCase) == 0) {
                        info.Name = "round_robotrampage_arena_2";
                        this.RoundDetails.Update(info);
                    } else if (info.Name.IndexOf("round_crown_maze", StringComparison.OrdinalIgnoreCase) == 0) {
                        info.Name = "round_crown_maze";
                        this.RoundDetails.Update(info);
                    }
                }
                this.StatsDB.Commit();
                this.AllStats.Clear();
                this.CurrentSettings.Version = 15;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.Version == 15) {
                this.AllStats.AddRange(this.RoundDetails.FindAll());
                this.StatsDB.BeginTrans();
                for (int i = this.AllStats.Count - 1; i >= 0; i--) {
                    RoundInfo info = this.AllStats[i];

                    if (info.Name.IndexOf("round_gauntlet_08", StringComparison.OrdinalIgnoreCase) == 0) {
                        info.Name = "round_gauntlet_08";
                        this.RoundDetails.Update(info);
                    } else if (info.Name.IndexOf("round_airtime", StringComparison.OrdinalIgnoreCase) == 0) {
                        info.Name = "round_airtime";
                        this.RoundDetails.Update(info);
                    } else if (info.Name.IndexOf("round_follow-", StringComparison.OrdinalIgnoreCase) == 0) {
                        info.Name = "round_follow-the-leader_s6_launch";
                        this.RoundDetails.Update(info);
                    } else if (info.Name.IndexOf("round_pipedup", StringComparison.OrdinalIgnoreCase) == 0) {
                        info.Name = "round_pipedup_s6_launch";
                        this.RoundDetails.Update(info);
                    } else if (info.Name.IndexOf("round_see_saw_360", StringComparison.OrdinalIgnoreCase) == 0) {
                        info.Name = "round_see_saw_360";
                        this.RoundDetails.Update(info);
                    }
                }
                this.StatsDB.Commit();
                this.AllStats.Clear();
                this.CurrentSettings.Version = 16;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.Version == 16) {
                this.AllStats.AddRange(this.RoundDetails.FindAll());
                this.StatsDB.BeginTrans();
                for (int i = AllStats.Count - 1; i >= 0; i--) {
                    RoundInfo info = AllStats[i];

                    if (info.Name.IndexOf("round_fruit_bowl", StringComparison.OrdinalIgnoreCase) == 0) {
                        info.Name = "round_fruit_bowl";
                        this.RoundDetails.Update(info);
                    }
                }
                this.StatsDB.Commit();
                this.AllStats.Clear();
                this.CurrentSettings.Version = 17;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.Version == 17) {
                this.AllStats.AddRange(this.RoundDetails.FindAll());
                this.StatsDB.BeginTrans();
                for (int i = this.AllStats.Count - 1; i >= 0; i--) {
                    RoundInfo info = AllStats[i];

                    if (info.Name.IndexOf("round_invisibeans", StringComparison.OrdinalIgnoreCase) == 0) {
                        info.Name = "round_invisibeans";
                        this.RoundDetails.Update(info);
                    }
                }
                this.StatsDB.Commit();
                this.AllStats.Clear();
                this.CurrentSettings.Version = 18;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.Version == 18) {
                this.AllStats.AddRange(this.RoundDetails.FindAll());
                this.StatsDB.BeginTrans();
                for (int i = AllStats.Count - 1; i >= 0; i--) {
                    RoundInfo info = this.AllStats[i];

                    if (info.Name.IndexOf("round_1v1_volleyfall", StringComparison.OrdinalIgnoreCase) == 0) {
                        info.Name = "round_1v1_volleyfall_symphony_launch_show";
                        this.RoundDetails.Update(info);
                    } else if (info.Name.IndexOf("round_gauntlet_09", StringComparison.OrdinalIgnoreCase) == 0) {
                        info.Name = "round_gauntlet_09_symphony_launch_show";
                        this.RoundDetails.Update(info);
                    } else if (info.Name.IndexOf("round_short_circuit_2", StringComparison.OrdinalIgnoreCase) == 0) {
                        info.Name = "round_short_circuit_2_symphony_launch_show";
                        this.RoundDetails.Update(info);
                    } else if (info.Name.IndexOf("round_hoops_revenge", StringComparison.OrdinalIgnoreCase) == 0) {
                        info.Name = "round_hoops_revenge_symphony_launch_show";
                        this.RoundDetails.Update(info);
                    } else if (info.Name.IndexOf("round_hexaring", StringComparison.OrdinalIgnoreCase) == 0) {
                        info.Name = "round_hexaring_symphony_launch_show";
                        this.RoundDetails.Update(info);
                    } else if (info.Name.IndexOf("round_spin_ring", StringComparison.OrdinalIgnoreCase) == 0) {
                        info.Name = "round_spin_ring_symphony_launch_show";
                        this.RoundDetails.Update(info);
                    } else if (info.Name.IndexOf("round_blastball", StringComparison.OrdinalIgnoreCase) == 0) {
                        info.Name = "round_blastball_arenasurvival_symphony_launch_show";
                        this.RoundDetails.Update(info);
                    }
                }
                this.StatsDB.Commit();
                this.AllStats.Clear();
                this.CurrentSettings.Version = 19;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.Version == 19) {
                this.AllStats.AddRange(this.RoundDetails.FindAll());
                this.StatsDB.BeginTrans();
                for (int i = this.AllStats.Count - 1; i >= 0; i--) {
                    RoundInfo info = this.AllStats[i];

                    if (info.Name.IndexOf("round_satellitehoppers", StringComparison.OrdinalIgnoreCase) == 0) {
                        info.Name = "round_satellitehoppers_almond";
                        this.RoundDetails.Update(info);
                    } else if (info.Name.IndexOf("round_ffa_button_bashers", StringComparison.OrdinalIgnoreCase) == 0) {
                        info.Name = "round_ffa_button_bashers_squads_almond";
                        this.RoundDetails.Update(info);
                    } else if (info.Name.IndexOf("round_hoverboardsurvival2", StringComparison.OrdinalIgnoreCase) == 0) {
                        info.Name = "round_hoverboardsurvival2_almond";
                        this.RoundDetails.Update(info);
                    } else if (info.Name.IndexOf("round_gauntlet_10", StringComparison.OrdinalIgnoreCase) == 0) {
                        info.Name = "round_gauntlet_10_almond";
                        this.RoundDetails.Update(info);
                    } else if (info.Name.IndexOf("round_starlink", StringComparison.OrdinalIgnoreCase) == 0) {
                        info.Name = "round_starlink_almond";
                        this.RoundDetails.Update(info);
                    } else if (info.Name.IndexOf("round_tiptoefinale", StringComparison.OrdinalIgnoreCase) == 0) {
                        info.Name = "round_tiptoefinale_almond";
                        this.RoundDetails.Update(info);
                    } else if (info.Name.IndexOf("round_pixelperfect", StringComparison.OrdinalIgnoreCase) == 0) {
                        info.Name = "round_pixelperfect_almond";
                        this.RoundDetails.Update(info);
                    } else if (info.Name.IndexOf("round_hexsnake", StringComparison.OrdinalIgnoreCase) == 0) {
                        info.Name = "round_hexsnake_almond";
                        this.RoundDetails.Update(info);
                    }
                }
                this.StatsDB.Commit();
                this.AllStats.Clear();
                this.CurrentSettings.Version = 20;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.Version == 20) {
                this.AllStats.AddRange(this.RoundDetails.FindAll());
                this.StatsDB.BeginTrans();
                for (int i = this.AllStats.Count - 1; i >= 0; i--) {
                    RoundInfo info = this.AllStats[i];

                    if (info.Name.IndexOf("round_follow-the-leader", StringComparison.OrdinalIgnoreCase) == 0) {
                        info.Name = "round_follow-the-leader_s6_launch";
                        this.RoundDetails.Update(info);
                    }
                }
                this.StatsDB.Commit();
                this.AllStats.Clear();
                this.CurrentSettings.Version = 21;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.Version == 21) {
                this.AllStats.AddRange(this.RoundDetails.FindAll());
                this.StatsDB.BeginTrans();
                for (int i = this.AllStats.Count - 1; i >= 0; i--) {
                    RoundInfo info = this.AllStats[i];

                    if (info.Name.IndexOf("round_slippy_slide", StringComparison.OrdinalIgnoreCase) == 0) {
                        info.Name = "round_slippy_slide";
                        this.RoundDetails.Update(info);
                    } else if (info.Name.IndexOf("round_follow_the_line", StringComparison.OrdinalIgnoreCase) == 0) {
                        info.Name = "round_follow_the_line";
                        this.RoundDetails.Update(info);
                    } else if (info.Name.IndexOf("round_slide_chute", StringComparison.OrdinalIgnoreCase) == 0) {
                        info.Name = "round_slide_chute";
                        this.RoundDetails.Update(info);
                    } else if (info.Name.IndexOf("round_blastballruins", StringComparison.OrdinalIgnoreCase) == 0) {
                        info.Name = "round_blastballruins";
                        this.RoundDetails.Update(info);
                    } else if (info.Name.IndexOf("round_kraken_attack", StringComparison.OrdinalIgnoreCase) == 0) {
                        info.Name = "round_kraken_attack";
                        this.RoundDetails.Update(info);
                    } else if (info.Name.IndexOf("round_bluejay", StringComparison.OrdinalIgnoreCase) == 0) {
                        info.Name = "round_bluejay";
                        this.RoundDetails.Update(info);
                    }
                }
                this.StatsDB.Commit();
                this.AllStats.Clear();
                this.CurrentSettings.Version = 23;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.Version == 23) {
                this.CurrentSettings.OverlayColor = 0;
                this.CurrentSettings.GameExeLocation = string.Empty;
                this.CurrentSettings.GameShortcutLocation = string.Empty;
                this.CurrentSettings.AutoLaunchGameOnStartup = false;
                this.CurrentSettings.Version = 24;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.Version == 24) {
                this.CurrentSettings.WinsFilter = 1;
                this.CurrentSettings.QualifyFilter = 1;
                this.CurrentSettings.FastestFilter = 1;
                this.CurrentSettings.Version = 25;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.Version == 25) {
                this.CurrentSettings.OverlayBackground = 0;
                this.CurrentSettings.OverlayBackgroundResourceName = string.Empty;
                this.CurrentSettings.OverlayTabResourceName = string.Empty;
                this.CurrentSettings.IsOverlayBackgroundCustomized = false;
                this.CurrentSettings.OverlayFontColorSerialized = string.Empty;
                this.CurrentSettings.Version = 26;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.Version == 26) {
                this.CurrentSettings.OverlayBackgroundOpacity = 100;
                this.CurrentSettings.Version = 27;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.Version == 27) {
                this.CurrentSettings.PreventOverlayMouseClicks = false;
                this.CurrentSettings.Version = 28;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.Version == 28) {
                this.CurrentSettings.Visible = true;
                this.CurrentSettings.Version = 29;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.Version == 29) {
                this.CurrentSettings.SystemTrayIcon = true;
                this.CurrentSettings.Version = 30;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.Version == 30) {
                this.AllStats.AddRange(this.RoundDetails.FindAll());
                this.StatsDB.BeginTrans();
                for (int i = this.AllStats.Count - 1; i >= 0; i--) {
                    RoundInfo info = this.AllStats[i];
                    if (info.Name.Equals("wle_s10_user_creative_round")) {
                        info.Name = "wle_s10_user_creative_race_round";
                        this.RoundDetails.Update(info);
                    }
                }
                this.StatsDB.Commit();
                this.AllStats.Clear();
                this.CurrentSettings.Version = 31;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.Version == 31) {
                this.CurrentSettings.OverlayColor = this.CurrentSettings.OverlayColor > 0 ? this.CurrentSettings.OverlayColor + 1 : this.CurrentSettings.OverlayColor;
                this.CurrentSettings.Version = 32;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.Version == 32) {
                this.CurrentSettings.FilterType += 1;
                this.CurrentSettings.SelectedCustomTemplateSeason = -1;
                this.CurrentSettings.CustomFilterRangeStart = DateTime.MinValue;
                this.CurrentSettings.CustomFilterRangeEnd = DateTime.MaxValue;
                this.CurrentSettings.Version = 33;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.Version == 33) {
                this.AllStats.AddRange(this.RoundDetails.FindAll());
                this.StatsDB.BeginTrans();
                for (int i = this.AllStats.Count - 1; i >= 0; i--) {
                    RoundInfo info = this.AllStats[i];
                    if (info.Name.Equals("round_bluejay_40")) {
                        info.Name = "round_bluejay";
                        this.RoundDetails.Update(info);
                    }
                }
                this.StatsDB.Commit();
                this.AllStats.Clear();
                this.CurrentSettings.Version = 34;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.Version == 34) {
                this.AllStats.AddRange(this.RoundDetails.FindAll());
                this.StatsDB.BeginTrans();
                for (int i = this.AllStats.Count - 1; i >= 0; i--) {
                    RoundInfo info = this.AllStats[i];
                    if (info.UseShareCode && info.CreativeLastModifiedDate != DateTime.MinValue && string.IsNullOrEmpty(info.CreativeOnlinePlatformId)) {
                        info.CreativeLastModifiedDate = DateTime.MinValue;
                        this.RoundDetails.Update(info);
                    }
                }
                this.StatsDB.Commit();
                this.AllStats.Clear();
                this.CurrentSettings.Version = 35;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.Version == 35) {
                this.CurrentSettings.AutoUpdate = true;
                this.CurrentSettings.Version = 36;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.Version == 36) {
                this.AllStats.AddRange(this.RoundDetails.FindAll());
                this.StatsDB.BeginTrans();
                for (int i = this.AllStats.Count - 1; i >= 0; i--) {
                    RoundInfo info = this.AllStats[i];
                    if (info.Name.Equals("round_follow-the-leader_ss2_launch") || info.Name.Equals("round_follow-the-leader_ss2_parrot")) {
                        info.Name = "round_follow_the_line";
                        this.RoundDetails.Update(info);
                    }
                }
                this.StatsDB.Commit();
                this.AllStats.Clear();
                this.CurrentSettings.Version = 37;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.Version == 37) {
                this.AllProfiles.AddRange(this.Profiles.FindAll());
                this.StatsDB.BeginTrans();
                for (int i = this.AllProfiles.Count - 1; i >= 0; i--) {
                    Profiles profile = this.AllProfiles[i];
                    if (!string.IsNullOrEmpty(profile.LinkedShowId) && profile.LinkedShowId.Equals("event_only_survival_ss2_3009_0210_2022")) {
                        profile.LinkedShowId = "survival_of_the_fittest";
                    }
                }
                this.Profiles.DeleteAll();
                this.Profiles.InsertBulk(this.AllProfiles);
                this.StatsDB.Commit();
                this.AllProfiles.Clear();
                this.CurrentSettings.Version = 38;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.Version == 38) {
                this.AllStats.AddRange(this.RoundDetails.FindAll());
                this.StatsDB.BeginTrans();
                this.CurrentSettings.NotifyServerConnected = false;
                for (int i = this.AllStats.Count - 1; i >= 0; i--) {
                    RoundInfo info = this.AllStats[i];
                    if (!string.IsNullOrEmpty(info.ShowNameId) && !info.IsFinal &&
                        (info.ShowNameId.StartsWith("show_wle_s10_wk") ||
                         info.ShowNameId.StartsWith("wle_s10_player_round_wk") ||
                         info.ShowNameId.StartsWith("show_wle_s10_player_round_wk") ||
                         info.ShowNameId.StartsWith("current_wle_fp"))) {
                        info.IsFinal = true;
                        this.RoundDetails.Update(info);
                    }
                }
                this.StatsDB.Commit();
                this.AllStats.Clear();
                this.CurrentSettings.Version = 39;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.Version == 39) {
                this.AllStats.AddRange(this.RoundDetails.FindAll());
                this.StatsDB.BeginTrans();
                for (int i = this.AllStats.Count - 1; i >= 0; i--) {
                    RoundInfo info = this.AllStats[i];
                    if (!string.IsNullOrEmpty(info.ShowNameId) && !info.IsFinal &&
                        (info.ShowNameId.StartsWith("show_wle_s10_wk") ||
                         info.ShowNameId.StartsWith("wle_s10_player_round_wk") ||
                         info.ShowNameId.StartsWith("show_wle_s10_player_round_wk") ||
                         info.ShowNameId.StartsWith("current_wle_fp"))) {
                        info.IsFinal = true;
                        this.RoundDetails.Update(info);
                    }
                }
                this.StatsDB.Commit();
                this.AllStats.Clear();
                this.CurrentSettings.Version = 40;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.Version == 40) {
                this.AllStats.AddRange(this.RoundDetails.FindAll());
                this.StatsDB.BeginTrans();
                for (int i = this.AllStats.Count - 1; i >= 0; i--) {
                    RoundInfo info = this.AllStats[i];
                    if (!string.IsNullOrEmpty(info.ShowNameId) && info.ShowNameId.StartsWith("wle_mrs_bagel") && info.Name.StartsWith("wle_mrs_bagel_final")) {
                        info.IsFinal = true;
                        this.RoundDetails.Update(info);
                    }
                }
                this.StatsDB.Commit();
                this.AllStats.Clear();
                this.CurrentSettings.Version = 41;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.Version == 41) {
                this.AllStats.AddRange(this.RoundDetails.FindAll());
                this.StatsDB.BeginTrans();
                for (int i = this.AllStats.Count - 1; i >= 0; i--) {
                    RoundInfo info = this.AllStats[i];
                    if (!string.IsNullOrEmpty(info.ShowNameId) && !info.IsFinal &&
                        (info.ShowNameId.StartsWith("show_wle_s10_wk") ||
                         info.ShowNameId.StartsWith("wle_s10_player_round_wk") ||
                         info.ShowNameId.StartsWith("show_wle_s10_player_round_wk") ||
                         info.ShowNameId.StartsWith("current_wle_fp"))) {
                        info.IsFinal = true;
                        this.RoundDetails.Update(info);
                    }
                }
                this.StatsDB.Commit();
                this.AllStats.Clear();
                this.CurrentSettings.Version = 42;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.Version == 42) {
                this.AllStats.AddRange(this.RoundDetails.FindAll());
                this.StatsDB.BeginTrans();
                for (int i = this.AllStats.Count - 1; i >= 0; i--) {
                    RoundInfo info = this.AllStats[i];
                    if (!string.IsNullOrEmpty(info.ShowNameId) && !info.IsFinal &&
                        (info.ShowNameId.StartsWith("show_wle_s10_wk") ||
                         info.ShowNameId.StartsWith("wle_s10_player_round_wk") ||
                         info.ShowNameId.StartsWith("show_wle_s10_player_round_wk") ||
                         info.ShowNameId.StartsWith("current_wle_fp"))) {
                        info.IsFinal = true;
                        this.RoundDetails.Update(info);
                    }
                }
                this.StatsDB.Commit();
                this.AllStats.Clear();
                this.CurrentSettings.Version = 43;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.Version == 44) {
                this.CurrentSettings.ShowChangelog = true;
                this.CurrentSettings.Version = 45;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.Version == 45) {
                this.AllStats.AddRange(this.RoundDetails.FindAll());
                this.StatsDB.BeginTrans();
                for (int i = this.AllStats.Count - 1; i >= 0; i--) {
                    RoundInfo info = this.AllStats[i];
                    if (!string.IsNullOrEmpty(info.ShowNameId) && !info.IsFinal &&
                        (info.ShowNameId.StartsWith("show_wle_s10_wk") ||
                         info.ShowNameId.StartsWith("wle_s10_player_round_wk") ||
                         info.ShowNameId.StartsWith("show_wle_s10_player_round_wk") ||
                         info.ShowNameId.StartsWith("current_wle_fp"))) {
                        info.IsFinal = true;
                        this.RoundDetails.Update(info);
                    }
                }
                this.StatsDB.Commit();
                this.AllStats.Clear();
                this.CurrentSettings.Version = 46;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.Version == 46) {
                this.AllStats.AddRange(this.RoundDetails.FindAll());
                this.StatsDB.BeginTrans();
                for (int i = this.AllStats.Count - 1; i >= 0; i--) {
                    RoundInfo info = this.AllStats[i];
                    if (!string.IsNullOrEmpty(info.ShowNameId) && !info.IsFinal &&
                        (info.ShowNameId.StartsWith("show_wle_s10_wk") ||
                         info.ShowNameId.StartsWith("wle_s10_player_round_wk") ||
                         info.ShowNameId.StartsWith("show_wle_s10_player_round_wk") ||
                         info.ShowNameId.StartsWith("current_wle_fp"))) {
                        info.IsFinal = true;
                        this.RoundDetails.Update(info);
                    }
                }
                this.StatsDB.Commit();
                this.AllStats.Clear();
                this.CurrentSettings.Version = 47;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.Version == 47) {
                this.AllStats.AddRange(this.RoundDetails.FindAll());
                this.StatsDB.BeginTrans();
                for (int i = this.AllStats.Count - 1; i >= 0; i--) {
                    RoundInfo info = this.AllStats[i];
                    if (!string.IsNullOrEmpty(info.ShowNameId) &&
                        (info.ShowNameId.StartsWith("show_wle_s10_wk") || info.ShowNameId.StartsWith("event_wle_s10_wk")) && info.ShowNameId.EndsWith("_mrs") &&
                        !this.IsFinalWithCreativeLevel(info.Name)) {
                        info.IsFinal = false;
                        this.RoundDetails.Update(info);
                    }
                }
                this.StatsDB.Commit();
                this.AllStats.Clear();
                this.CurrentSettings.GroupingCreativeRoundLevels = true;
                this.CurrentSettings.Version = 48;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.Version == 48) {
                this.AllStats.AddRange(this.RoundDetails.FindAll());
                this.StatsDB.BeginTrans();
                for (int i = this.AllStats.Count - 1; i >= 0; i--) {
                    RoundInfo info = this.AllStats[i];
                    if (!string.IsNullOrEmpty(info.ShowNameId) &&
                        info.ShowNameId.Equals("main_show") &&
                        this.IsFinalWithCreativeLevel(info.Name)) {
                        info.IsFinal = true;
                        this.RoundDetails.Update(info);
                    }
                }
                this.StatsDB.Commit();
                this.AllStats.Clear();
                this.CurrentSettings.GroupingCreativeRoundLevels = true;
                this.CurrentSettings.Version = 49;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.Version == 49) {
                this.AllStats.AddRange(this.RoundDetails.FindAll());
                this.StatsDB.BeginTrans();
                for (int i = this.AllStats.Count - 1; i >= 0; i--) {
                    RoundInfo info = this.AllStats[i];
                    if (!string.IsNullOrEmpty(info.ShowNameId) && !info.IsFinal &&
                        info.ShowNameId.StartsWith("wle_s10_cf_round_")) {
                        info.IsFinal = true;
                        this.RoundDetails.Update(info);
                    }
                }
                this.StatsDB.Commit();
                this.AllStats.Clear();
                this.CurrentSettings.Version = 50;
                this.SaveUserSettings();
            }

            //
            // "Frenchy Edition" mods
            //

            if (this.CurrentSettings.FrenchyEditionDB <= 3) {
                this.CurrentSettings.Theme = 1;
                this.CurrentSettings.OverlayBackgroundOpacity = 100;
                this.CurrentSettings.HideOverlayPercentages = true;
                this.CurrentSettings.WinsFilter = 1;
                this.CurrentSettings.QualifyFilter = 1;
                this.CurrentSettings.FastestFilter = 0;
                this.CurrentSettings.OverlayColor = 0;
                this.CurrentSettings.OverlayVisible = true;
                this.CurrentSettings.PlayerByConsoleType = true;
                this.CurrentSettings.ColorByRoundType = true;
                this.CurrentSettings.AutoChangeProfile = false;
                this.CurrentSettings.AutoUpdate = true;
                this.CurrentSettings.FrenchyEditionDB = 4;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.FrenchyEditionDB == 4) {
                this.CurrentSettings.WinPerDayGraphStyle = 1;
                this.CurrentSettings.FrenchyEditionDB = 5;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.FrenchyEditionDB == 5) {
                this.CurrentSettings.EnableFallalyticsReporting = true;
                this.CurrentSettings.FrenchyEditionDB = 6;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.FrenchyEditionDB == 6) {
                this.CurrentSettings.SystemTrayIcon = false;
                this.CurrentSettings.FrenchyEditionDB = 7;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.FrenchyEditionDB == 7) {
                this.CurrentSettings.SwitchBetweenLongest = false;
                this.CurrentSettings.OnlyShowLongest = false;
                this.CurrentSettings.FrenchyEditionDB = 8;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.FrenchyEditionDB == 8) {
                this.CurrentSettings.SwitchBetweenPlayers = true;
                this.CurrentSettings.OnlyShowPing = false;
                this.CurrentSettings.FrenchyEditionDB = 9;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.FrenchyEditionDB == 9) {
                this.AllStats.AddRange(this.RoundDetails.FindAll());
                this.StatsDB.BeginTrans();
                for (int i = this.AllStats.Count - 1; i >= 0; i--) {
                    RoundInfo info = this.AllStats[i];
                    if (info.Name.Equals("wle_s10_orig_round_045")) {
                        info.Name = "wle_s10_orig_round_045_long";
                        this.RoundDetails.Update(info);
                    }
                }
                this.StatsDB.Commit();
                this.AllStats.Clear();
                this.CurrentSettings.OverlayNotOnTop = false;
                this.CurrentSettings.FrenchyEditionDB = 10;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.FrenchyEditionDB == 10) {
                this.AllStats.AddRange(this.RoundDetails.FindAll());
                this.StatsDB.BeginTrans();
                for (int i = this.AllStats.Count - 1; i >= 0; i--) {
                    RoundInfo info = this.AllStats[i];
                    if (info.UseShareCode && info.CreativeLastModifiedDate != DateTime.MinValue && string.IsNullOrEmpty(info.CreativeOnlinePlatformId)) {
                        info.CreativeLastModifiedDate = DateTime.MinValue;
                        this.RoundDetails.Update(info);
                    }
                }
                this.StatsDB.Commit();
                this.AllStats.Clear();
                if (this.CurrentSettings.OverlayBackgroundOpacity < 5) {
                    this.CurrentSettings.OverlayBackgroundOpacity = 5;
                }
                this.CurrentSettings.FrenchyEditionDB = 11;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.FrenchyEditionDB == 11) {
                this.AllStats.AddRange(this.RoundDetails.FindAll());
                this.StatsDB.BeginTrans();
                for (int i = this.AllStats.Count - 1; i >= 0; i--) {
                    RoundInfo info = this.AllStats[i];
                    if (info.Name.StartsWith("ugc-", StringComparison.OrdinalIgnoreCase)) {
                        info.Name = info.Name.Substring(4);
                        info.ShowNameId = info.ShowNameId.Substring(4);
                        info.UseShareCode = true;
                        info.AbandonShow = true;
                        info.IsFinal = true;
                        this.RoundDetails.Update(info);
                    }
                }
                this.StatsDB.Commit();
                this.AllStats.Clear();
                this.CurrentSettings.FrenchyEditionDB = 12;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.FrenchyEditionDB == 12) {
                this.AllStats.AddRange(this.RoundDetails.FindAll());
                this.StatsDB.BeginTrans();
                for (int i = this.AllStats.Count - 1; i >= 0; i--) {
                    RoundInfo info = this.AllStats[i];
                    if (info.Name.Equals("current_wle_fp4_10_08") && !info.PrivateLobby) {
                        info.PrivateLobby = true;
                        info.Name = "1654-1285-7119";
                        info.ShowNameId = "1654-1285-7119";
                        info.UseShareCode = true;
                        this.RoundDetails.Update(info);
                    }
                }
                this.StatsDB.Commit();
                this.AllStats.Clear();
                this.CurrentSettings.FrenchyEditionDB = 13;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.FrenchyEditionDB == 13) {
                this.AllStats.AddRange(this.RoundDetails.FindAll());
                this.StatsDB.BeginTrans();
                for (int i = this.AllStats.Count - 1; i >= 0; i--) {
                    RoundInfo info = this.AllStats[i];
                    if (!string.IsNullOrEmpty(info.ShowNameId) && !info.IsFinal &&
                        info.ShowNameId.StartsWith("show_wle_s10_player_round_wk")) {
                        info.IsFinal = true;
                        this.RoundDetails.Update(info);
                    }
                }
                this.StatsDB.Commit();
                this.AllStats.Clear();
                this.CurrentSettings.FrenchyEditionDB = 14;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.FrenchyEditionDB == 14) {
                this.AllStats.AddRange(this.RoundDetails.FindAll());
                this.StatsDB.BeginTrans();
                for (int i = this.AllStats.Count - 1; i >= 0; i--) {
                    RoundInfo info = this.AllStats[i];
                    if (!string.IsNullOrEmpty(info.ShowNameId) && info.IsFinal &&
                        info.ShowNameId.Equals("survival_of_the_fittest") &&
                        info.Name.Equals("round_kraken_attack") &&
                        info.Round != 4) {
                        info.IsFinal = false;
                        this.RoundDetails.Update(info);
                    }
                }
                this.StatsDB.Commit();
                this.AllStats.Clear();
                this.CurrentSettings.FrenchyEditionDB = 15;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.FrenchyEditionDB == 15) {
                this.AllStats.AddRange(this.RoundDetails.FindAll());
                this.StatsDB.BeginTrans();
                for (int i = this.AllStats.Count - 1; i >= 0; i--) {
                    RoundInfo info = this.AllStats[i];
                    if (!string.IsNullOrEmpty(info.ShowNameId) && info.IsFinal &&
                        info.ShowNameId.Equals("event_only_hexaring_template") &&
                        info.Round < 3) {
                        info.IsFinal = false;
                        this.RoundDetails.Update(info);
                    }
                }
                this.StatsDB.Commit();
                this.AllStats.Clear();
                this.CurrentSettings.FrenchyEditionDB = 16;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.FrenchyEditionDB == 16) {
                this.AllStats.AddRange(this.RoundDetails.FindAll());
                this.StatsDB.BeginTrans();
                for (int i = this.AllStats.Count - 1; i >= 0; i--) {
                    RoundInfo info = this.AllStats[i];
                    if (!string.IsNullOrEmpty(info.ShowNameId) && info.IsFinal &&
                        info.ShowNameId.Equals("event_only_thin_ice_template") &&
                        info.Round < 3) {
                        info.IsFinal = false;
                        this.RoundDetails.Update(info);
                    }
                }
                this.StatsDB.Commit();
                this.AllStats.Clear();
                this.CurrentSettings.FrenchyEditionDB = 17;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.FrenchyEditionDB == 17) {
                this.AllStats.AddRange(this.RoundDetails.FindAll());
                this.StatsDB.BeginTrans();
                for (int i = this.AllStats.Count - 1; i >= 0; i--) {
                    RoundInfo info = this.AllStats[i];
                    if (!string.IsNullOrEmpty(info.ShowNameId) &&
                        info.ShowNameId.Equals("wle_mrs_shuffle_show")) {
                        info.Name = info.Name.StartsWith("mrs_wle_fp") ? $"current_{info.Name.Substring(4)}" : info.Name.Substring(4);
                        info.IsFinal = true;
                        this.RoundDetails.Update(info);
                    }
                }
                this.StatsDB.Commit();
                this.AllStats.Clear();
                this.CurrentSettings.FrenchyEditionDB = 18;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.FrenchyEditionDB == 18) {
                this.AllStats.AddRange(this.RoundDetails.FindAll());
                this.StatsDB.BeginTrans();
                for (int i = this.AllStats.Count - 1; i >= 0; i--) {
                    RoundInfo info = this.AllStats[i];
                    if (info.Name.Equals("round_invisibeans") || info.Name.Equals("round_pumpkin_pie")) {
                        if (info.Name.Equals("round_pumpkin_pie")) {
                            info.Name = "round_invisibeans";
                        }
                        info.IsFinal = true;
                        this.RoundDetails.Update(info);
                    }
                }
                this.StatsDB.Commit();
                this.AllStats.Clear();
                this.CurrentSettings.FrenchyEditionDB = 19;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.FrenchyEditionDB == 19) {
                this.AllStats.AddRange(this.RoundDetails.FindAll());
                this.StatsDB.BeginTrans();
                for (int i = this.AllStats.Count - 1; i >= 0; i--) {
                    RoundInfo info = this.AllStats[i];
                    if (!string.IsNullOrEmpty(info.ShowNameId) &&
                        info.ShowNameId.Equals("wle_mrs_shuffle_show") &&
                        info.Name.StartsWith("shuffle_halloween_")) {
                        info.Name = $"wle_fp5_falloween_round_{info.Name.Substring(info.Name.LastIndexOf('_') + 1)}";
                        this.RoundDetails.Update(info);
                    }
                }
                this.StatsDB.Commit();
                this.AllStats.Clear();
                this.CurrentSettings.FrenchyEditionDB = 20;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.FrenchyEditionDB == 20) {
                string filePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string[] fileList = {
                    Path.Combine(filePath, "LiteDB.dll"), Path.Combine(filePath, "GeoLite2-Country.mmdb"),
                    Path.Combine(filePath, "COPYRIGHT-GEOLITE2.txt"), Path.Combine(filePath, "LICENSE-GEOLITE2.txt")
                };
                foreach (string file in fileList) {
                    if (File.Exists(file)) {
                        try {
                            File.SetAttributes(file, FileAttributes.Normal);
                            File.Delete(file);
                        } catch { }
                    }
                }
                this.CurrentSettings.FrenchyEditionDB = 21;
                this.SaveUserSettings();
            }
            if (this.CurrentSettings.FrenchyEditionDB == 21) {
                this.AllStats.AddRange(this.RoundDetails.FindAll());
                this.StatsDB.BeginTrans();
                for (int i = this.AllStats.Count - 1; i >= 0; i--) {
                    RoundInfo info = this.AllStats[i];
                    if (!string.IsNullOrEmpty(info.ShowNameId) &&
                        info.ShowNameId.Equals("current_wle_fp5_falloween_7_01_04")) {
                        info.ShowNameId = "current_wle_fp5_falloween_7_01_05";
                        info.Name = "current_wle_fp5_falloween_7_01_05";
                        this.RoundDetails.Update(info);
                    }
                }
                this.StatsDB.Commit();
                this.AllStats.Clear();
                this.CurrentSettings.FrenchyEditionDB = 22;
                this.SaveUserSettings();
            }
            if (this.CurrentSettings.FrenchyEditionDB == 22) {
                this.AllStats.AddRange(this.RoundDetails.FindAll());
                this.StatsDB.BeginTrans();
                for (int i = this.AllStats.Count - 1; i >= 0; i--) {
                    RoundInfo info = this.AllStats[i];
                    if (!string.IsNullOrEmpty(info.ShowNameId) &&
                        info.ShowNameId.Equals("event_only_hoverboard_template") &&
                        info.Round == 3) {
                        info.IsFinal = true;
                        this.RoundDetails.Update(info);
                    }
                }
                this.StatsDB.Commit();
                this.AllStats.Clear();
                this.CurrentSettings.FrenchyEditionDB = 23;
                this.SaveUserSettings();
            }
            if (this.CurrentSettings.FrenchyEditionDB == 23) {
                this.AllStats.AddRange(this.RoundDetails.FindAll());
                this.StatsDB.BeginTrans();
                for (int i = this.AllStats.Count - 1; i >= 0; i--) {
                    RoundInfo info = this.AllStats[i];
                    if (!string.IsNullOrEmpty(info.ShowNameId) &&
                        info.ShowNameId.Equals("wle_shuffle_discover")) {
                        info.IsFinal = true;
                        this.RoundDetails.Update(info);
                    }
                }
                this.StatsDB.Commit();
                this.AllStats.Clear();
                this.CurrentSettings.FrenchyEditionDB = 24;
                this.SaveUserSettings();
            }
            if (this.CurrentSettings.FrenchyEditionDB == 24) {
                this.AllStats.AddRange(this.RoundDetails.FindAll());
                this.StatsDB.BeginTrans();
                for (int i = this.AllStats.Count - 1; i >= 0; i--) {
                    RoundInfo info = this.AllStats[i];
                    if (!string.IsNullOrEmpty(info.ShowNameId) &&
                        info.ShowNameId.Equals("wle_mrs_shuffle_show") &&
                        info.Name.StartsWith("wle_fp5_falloween_round_")) {
                        if (this.LevelIdReplacerFalloween.TryGetValue(info.Name, out string newName)) {
                            info.Name = newName;
                            this.RoundDetails.Update(info);
                        }
                    }
                }
                this.StatsDB.Commit();
                this.AllStats.Clear();
                this.CurrentSettings.FrenchyEditionDB = 25;
                this.SaveUserSettings();
            }
            if (this.CurrentSettings.FrenchyEditionDB == 25) {
                this.AllStats.AddRange(this.RoundDetails.FindAll());
                this.StatsDB.BeginTrans();
                for (int i = this.AllStats.Count - 1; i >= 0; i--) {
                    RoundInfo info = this.AllStats[i];
                    if (!string.IsNullOrEmpty(info.ShowNameId) &&
                        Regex.IsMatch(info.ShowNameId, @"^\d{4}-\d{4}-\d{4}$")) {
                        info.Name = "user_creative_race_round";
                        this.RoundDetails.Update(info);
                    }
                }
                this.StatsDB.Commit();
                this.AllStats.Clear();
                this.CurrentSettings.SwitchBetweenLongest = true;
                this.CurrentSettings.DisplayCurrentTime = true;
                this.CurrentSettings.GroupingCreativeRoundLevels = true;
                this.CurrentSettings.Version = 56;
                this.CurrentSettings.FrenchyEditionDB = 26;
                this.SaveUserSettings();
            }
        }
        private UserSettings GetDefaultSettings() {
            return new UserSettings {
                ID = 1,
                Theme = 1,
                CycleTimeSeconds = 5,
                FilterType = 1,
                CustomFilterRangeStart = DateTime.MinValue,
                CustomFilterRangeEnd = DateTime.MaxValue,
                SelectedCustomTemplateSeason = -1,
                SelectedProfile = 0,
                LogPath = null,
                EnableFallalyticsReporting = true,
                FallalyticsAPIKey = string.Empty,
                OverlayBackground = 0,
                OverlayBackgroundResourceName = string.Empty,
                OverlayTabResourceName = string.Empty,
                OverlayBackgroundOpacity = 100,
                IsOverlayBackgroundCustomized = false,
                NotifyServerConnected = false,
                OverlayColor = 0,
                OverlayLocationX = null,
                OverlayLocationY = null,
                OverlayFixedPosition = string.Empty,
                OverlayFixedPositionX = null,
                OverlayFixedPositionY = null,
                OverlayFixedWidth = null,
                OverlayFixedHeight = null,
                FlippedDisplay = false,
                FixedFlippedDisplay = false,
                SwitchBetweenLongest = true,
                SwitchBetweenQualify = true,
                SwitchBetweenPlayers = true,
                SwitchBetweenStreaks = true,
                OnlyShowLongest = false,
                OnlyShowGold = false,
                OnlyShowPing = false,
                OnlyShowFinalStreak = false,
                OverlayVisible = true,
                OverlayNotOnTop = false,
                OverlayFontSerialized = string.Empty,
                OverlayFontColorSerialized = string.Empty,
                PlayerByConsoleType = true,
                ColorByRoundType = true,
                AutoChangeProfile = false,
                DisplayCurrentTime = true,
                PreviousWins = 0,
                WinsFilter = 1,
                QualifyFilter = 1,
                FastestFilter = 0,
                HideWinsInfo = false,
                HideRoundInfo = false,
                HideTimeInfo = false,
                ShowOverlayTabs = false,
                ShowPercentages = false,
                AutoUpdate = true,
                PreventOverlayMouseClicks = false,
                MaximizedWindowState = false,
                SystemTrayIcon = false,
                StartMinimized = false,
                FormLocationX = null,
                FormLocationY = null,
                FormWidth = null,
                FormHeight = null,
                OverlayWidth = 786,
                OverlayHeight = 99,
                HideOverlayPercentages = true,
                HoopsieHeros = false,
                GameExeLocation = string.Empty,
                GameShortcutLocation = string.Empty,
                AutoLaunchGameOnStartup = false,
                IgnoreLevelTypeWhenSorting = false,
                GroupingCreativeRoundLevels = true,
                UpdatedDateFormat = true,
                WinPerDayGraphStyle = 1,
                ShowChangelog = true,
                Visible = true,
                Version = 56,
                FrenchyEditionDB = 26
            };
        }
        private bool IsFinalWithCreativeLevel(string levelId) {
            return levelId.Equals("wle_s10_orig_round_010") ||
                   levelId.Equals("wle_s10_orig_round_011") ||
                   levelId.Equals("wle_s10_orig_round_017") ||
                   levelId.Equals("wle_s10_orig_round_018") ||
                   levelId.Equals("wle_s10_orig_round_024") ||
                   levelId.Equals("wle_s10_orig_round_025") ||
                   levelId.Equals("wle_s10_orig_round_030") ||
                   levelId.Equals("wle_s10_orig_round_031") ||
                   levelId.Equals("wle_s10_round_004") ||
                   levelId.Equals("wle_s10_round_009");
        }
        private void UpdateHoopsieLegends() {
            LevelStats level = this.StatLookup["round_hoops_blockade_solo"];
            string newName = this.CurrentSettings.HoopsieHeros ? Multilingual.GetWord("main_hoopsie_heroes") : Multilingual.GetWord("main_hoopsie_legends");
            if (level.Name != newName) {
                level.Name = newName;
            }
        }
        private bool IsCreativeLevel(string levelId) {
            return levelId.StartsWith("wle_s10_round_") || levelId.StartsWith("wle_s10_orig_round_")
                   || levelId.StartsWith("wle_mrs_bagel_") || levelId.StartsWith("wle_s10_bt_round_")
                   || levelId.StartsWith("current_wle_fp") || levelId.StartsWith("wle_s10_player_round_wk")
                   || levelId.StartsWith("wle_s10_cf_round_") || levelId.StartsWith("wle_s10_long_round_")
                   || levelId.StartsWith("wle_discover_level_wk") || levelId.Equals("wle_fp2_wk6_01");
        }
        private void UpdateGridRoundName() {
            foreach (KeyValuePair<string, string> item in Multilingual.GetRoundsDictionary()) {
                if (this.IsCreativeLevel(item.Key)) { continue; }
                LevelStats level = this.StatLookup[item.Key];
                level.Name = item.Value;
            }
            this.SortGridDetails(0, true);
            this.gridDetails.Invalidate();
        }
        public void UpdateDates() {
            if (DateTime.Now.Date.ToUniversalTime() == DayStart) { return; }

            DateTime currentUTC = DateTime.UtcNow;
            for (int i = Seasons.Count - 1; i >= 0; i--) {
                if (currentUTC > Seasons[i]) {
                    SeasonStart = Seasons[i];
                    break;
                }
            }
            WeekStart = DateTime.Now.Date.AddDays(-7).ToUniversalTime();
            DayStart = DateTime.Now.Date.ToUniversalTime();

            this.ResetStats();
        }
        public void SaveUserSettings() {
            lock (this.StatsDB) {
                this.StatsDB.BeginTrans();
                this.UserSettings.Update(this.CurrentSettings);
                this.StatsDB.Commit();
            }
        }
        public void ResetStats() {
            for (int i = 0; i < this.StatDetails.Count; i++) {
                LevelStats calculator = StatDetails[i];
                calculator.Clear();
            }

            this.ClearTotals();

            List<RoundInfo> rounds = new List<RoundInfo>();
            int profile = this.currentProfile;

            lock (this.StatsDB) {
                this.AllStats.Clear();
                this.nextShowID = 0;
                this.lastAddedShow = DateTime.MinValue;
                if (this.RoundDetails.Count() > 0) {
                    this.AllStats.AddRange(this.RoundDetails.FindAll());
                    this.AllStats.Sort();

                    if (this.AllStats.Count > 0) {
                        this.nextShowID = this.AllStats[this.AllStats.Count - 1].ShowID;

                        int lastAddedShowID = -1;
                        for (int i = this.AllStats.Count - 1; i >= 0; i--) {
                            RoundInfo info = this.AllStats[i];
                            info.ToLocalTime();
                            if (info.Profile != profile) { continue; }

                            if (info.ShowID == lastAddedShowID || (IsInStatsFilter(info) && IsInPartyFilter(info))) {
                                lastAddedShowID = info.ShowID;
                                rounds.Add(info);
                            }

                            if (info.Start > lastAddedShow && info.Round == 1) {
                                this.lastAddedShow = info.Start;
                            }
                        }
                    }
                }
            }

            lock (this.CurrentRound) {
                this.CurrentRound.Clear();
                for (int i = this.AllStats.Count - 1; i >= 0; i--) {
                    RoundInfo info = AllStats[i];
                    if (info.Profile != profile) { continue; }

                    this.CurrentRound.Insert(0, info);
                    if (info.Round == 1) {
                        break;
                    }
                }
            }

            rounds.Sort();
            this.loadingExisting = true;
            this.LogFile_OnParsedLogLines(rounds);
            this.loadingExisting = false;
        }

        private void MenuTodaysShow_MouseEnter(object sender, EventArgs e) {
            this.Cursor = Cursors.Hand;
        }
        private void MenuTodaysShow_MouseLeave(object sender, EventArgs e) {
            this.Cursor = Cursors.Default;
        }

        private void MenuOverlay_MouseEnter(object sender, EventArgs e) {
            this.Cursor = Cursors.Hand;
        }
        private void MenuOverlay_MouseLeave(object sender, EventArgs e) {
            this.Cursor = Cursors.Default;
        }

        private void SetCursor_MouseMove(object sender, MouseEventArgs e) {
            this.Cursor = Cursors.Hand;
        }
        private void SetCursor_MouseLeave(object sender, EventArgs e) {
            this.Cursor = Cursors.Default;
        }

        private void SaveWindowState() {
            if (this.overlay.Visible) {
                if (!this.overlay.IsFixed()) {
                    this.CurrentSettings.OverlayLocationX = this.overlay.Location.X;
                    this.CurrentSettings.OverlayLocationY = this.overlay.Location.Y;
                    this.CurrentSettings.OverlayWidth = this.overlay.Width;
                    this.CurrentSettings.OverlayHeight = this.overlay.Height;
                } else {
                    this.CurrentSettings.OverlayFixedPositionX = this.overlay.Location.X;
                    this.CurrentSettings.OverlayFixedPositionY = this.overlay.Location.Y;
                    this.CurrentSettings.OverlayFixedWidth = this.overlay.Width;
                    this.CurrentSettings.OverlayFixedHeight = this.overlay.Height;
                }
            }

            if (this.WindowState != FormWindowState.Normal) {
                this.CurrentSettings.FormLocationX = this.RestoreBounds.Location.X;
                this.CurrentSettings.FormLocationY = this.RestoreBounds.Location.Y;
                this.CurrentSettings.FormWidth = this.RestoreBounds.Size.Width;
                this.CurrentSettings.FormHeight = this.RestoreBounds.Size.Height;
                this.CurrentSettings.MaximizedWindowState = this.WindowState == FormWindowState.Maximized;
            } else {
                this.CurrentSettings.FormLocationX = this.Location.X;
                this.CurrentSettings.FormLocationY = this.Location.Y;
                this.CurrentSettings.FormWidth = this.Size.Width;
                this.CurrentSettings.FormHeight = this.Size.Height;
                this.CurrentSettings.MaximizedWindowState = false;
            }
        }
        private void Stats_FormClosing(object sender, FormClosingEventArgs e) {
            try {
                if (!this.overlay.Disposing && !this.overlay.IsDisposed && !this.IsDisposed && !this.Disposing) {
                    //this.CurrentSettings.FilterType = this.menuAllStats.Checked ? 1 : this.menuSeasonStats.Checked ? 2 : this.menuWeekStats.Checked ? 3 : this.menuDayStats.Checked ? 4 : 5;
                    //this.CurrentSettings.SelectedProfile = this.currentProfile;
                    if (!this.isUpdate) { this.SaveWindowState(); }
                    this.SaveUserSettings();
                }
                this.StatsDB.Dispose();
            } catch (Exception ex) {
                MessageBox.Show(this, ex.ToString(), $"{Multilingual.GetWord("message_program_error_caption")}",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void Stats_Load(object sender, EventArgs e) {
            try {
#if AllowUpdate
                /* try {
                    if (this.CurrentSettings.AutoUpdate && this.CheckForUpdate(true)) {
                        return;
                    }
                } catch {
                    // ignored
                } */
#endif
                if (this.CurrentSettings.AutoLaunchGameOnStartup) {
                    this.LaunchGame(true);
                }

                this.MenuStats_Click(this.menuProfile.DropDownItems[$@"menuProfile{this.CurrentSettings.SelectedProfile}"], EventArgs.Empty);

                this.UpdateDates();
            } catch (Exception ex) {
                MessageBox.Show(this, ex.ToString(), $"{Multilingual.GetWord("message_program_error_caption")}",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void Stats_Shown(object sender, EventArgs e) {
            try {
                this.RemoveUpdateFiles();

                this.SetMainDataGridViewOrder();

                if (this.CurrentSettings.FormWidth.HasValue) {
                    this.Size = new Size(this.CurrentSettings.FormWidth.Value, this.CurrentSettings.FormHeight.Value);
                }
                if (this.CurrentSettings.FormLocationX.HasValue && IsOnScreen(this.CurrentSettings.FormLocationX.Value, this.CurrentSettings.FormLocationY.Value, this.Width, this.Height)) {
                    this.Location = new Point(this.CurrentSettings.FormLocationX.Value, this.CurrentSettings.FormLocationY.Value);
                }

                string logFilePath = Path.Combine($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}Low", "Mediatonic", "FallGuys_client");
                if (!string.IsNullOrEmpty(this.CurrentSettings.LogPath)) {
                    logFilePath = this.CurrentSettings.LogPath;
                }
                this.logFile.Start(logFilePath, LOGFILENAME);

                this.overlay.ArrangeDisplay(string.IsNullOrEmpty(this.CurrentSettings.OverlayFixedPosition) ? this.CurrentSettings.FlippedDisplay : this.CurrentSettings.FixedFlippedDisplay, this.CurrentSettings.ShowOverlayTabs,
                    this.CurrentSettings.HideWinsInfo, this.CurrentSettings.HideRoundInfo, this.CurrentSettings.HideTimeInfo,
                    this.CurrentSettings.OverlayColor, string.IsNullOrEmpty(this.CurrentSettings.OverlayFixedPosition) ? this.CurrentSettings.OverlayWidth : this.CurrentSettings.OverlayFixedWidth, string.IsNullOrEmpty(this.CurrentSettings.OverlayFixedPosition) ? this.CurrentSettings.OverlayHeight : this.CurrentSettings.OverlayFixedHeight,
                    this.CurrentSettings.OverlayFontSerialized, this.CurrentSettings.OverlayFontColorSerialized);
                if (this.CurrentSettings.OverlayVisible) { this.ToggleOverlay(this.overlay); }

                this.selectedCustomTemplateSeason = this.CurrentSettings.SelectedCustomTemplateSeason;
                this.customfilterRangeStart = this.CurrentSettings.CustomFilterRangeStart;
                this.customfilterRangeEnd = this.CurrentSettings.CustomFilterRangeEnd;

                this.menuAllStats.Checked = false;
                switch (this.CurrentSettings.FilterType) {
                    case 0:
                        this.menuCustomRangeStats.Checked = true;
                        this.MenuStats_Click(this.menuCustomRangeStats, EventArgs.Empty);
                        break;
                    case 1:
                        this.menuAllStats.Checked = true;
                        this.MenuStats_Click(this.menuAllStats, EventArgs.Empty);
                        break;
                    case 2:
                        this.menuSeasonStats.Checked = true;
                        this.MenuStats_Click(this.menuSeasonStats, EventArgs.Empty);
                        break;
                    case 3:
                        this.menuWeekStats.Checked = true;
                        this.MenuStats_Click(this.menuWeekStats, EventArgs.Empty);
                        break;
                    case 4:
                        this.menuDayStats.Checked = true;
                        this.MenuStats_Click(this.menuDayStats, EventArgs.Empty);
                        break;
                    case 5:
                        this.menuSessionStats.Checked = true;
                        this.MenuStats_Click(this.menuSessionStats, EventArgs.Empty);
                        break;
                    default:
                        this.menuAllStats.Checked = true;
                        this.MenuStats_Click(this.menuAllStats, EventArgs.Empty);
                        break;
                }

                this.WindowState = this.CurrentSettings.MaximizedWindowState ? FormWindowState.Maximized : FormWindowState.Normal;

                if (this.CurrentSettings.StartMinimized || this.minimizeAfterGameLaunch) {
                    this.WindowState = FormWindowState.Minimized;
                } else if (this.WindowState == FormWindowState.Normal) {
                    this.Activate();
                }

                this.isStartingUp = false;
            } catch (Exception ex) {
                MessageBox.Show(this, ex.ToString(), $"{Multilingual.GetWord("message_program_error_caption")}",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void LogFile_OnError(string error) {
            if (!this.Disposing && !this.IsDisposed) {
                try {
                    if (this.InvokeRequired) {
                        this.Invoke((Action<string>)LogFile_OnError, error);
                    } else {
                        MessageBox.Show(this, error, $"{Multilingual.GetWord("message_program_error_caption")}",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                } catch { }
            }
        }
        private void LogFile_OnNewLogFileDate(DateTime newDate) {
            if (SessionStart != newDate) {
                SessionStart = newDate;
                if (this.menuSessionStats.Checked) {
                    this.MenuStats_Click(this.menuSessionStats, EventArgs.Empty);
                }
            }
        }
        private void LogFile_OnParsedLogLinesCurrent(List<RoundInfo> round) {
            lock (this.CurrentRound) {
                if (this.CurrentRound == null || this.CurrentRound.Count != round.Count) {
                    this.CurrentRound = round;
                } else {
                    for (int i = 0; i < this.CurrentRound.Count; i++) {
                        RoundInfo info = this.CurrentRound[i];
                        if (!info.Equals(round[i])) {
                            this.CurrentRound = round;
                            break;
                        }
                    }
                }
            }
        }
        private void LogFile_OnParsedLogLines(List<RoundInfo> round) {
            try {
                if (this.InvokeRequired) {
                    this.Invoke((Action<List<RoundInfo>>)this.LogFile_OnParsedLogLines, round);
                    return;
                }

                lock (this.StatsDB) {
                    if (!this.loadingExisting) { this.StatsDB.BeginTrans(); }

                    int profile = this.currentProfile;
                    for (int k = 0; k < round.Count; k++) {
                        RoundInfo stat = round[k];

                        if (!this.loadingExisting) {
                            RoundInfo info = null;
                            for (int i = this.AllStats.Count - 1; i >= 0; i--) {
                                RoundInfo temp = this.AllStats[i];
                                if (temp.Start == stat.Start && temp.Name == stat.Name) {
                                    info = temp;
                                    break;
                                }
                            }

                            if (info == null && stat.Start > this.lastAddedShow) {
                                if (stat.ShowEnd < this.startupTime && this.askedPreviousShows == 0) {
                                    HideOverlayTime = true;
                                    using (EditShows editShows = new EditShows()) {
                                        editShows.FunctionFlag = "add";
                                        //editShows.Icon = this.Icon;
                                        editShows.Profiles = this.AllProfiles;
                                        editShows.StatsForm = this;
                                        this.EnableInfoStrip(false);
                                        this.EnableMainMenu(false);
                                        if (editShows.ShowDialog(this) == DialogResult.OK) {
                                            this.askedPreviousShows = 1;
                                            if (editShows.UseLinkedProfiles) {
                                                this.useLinkedProfiles = true;
                                            } else {
                                                profile = editShows.SelectedProfileId;
                                            }
                                        } else {
                                            this.askedPreviousShows = 2;
                                        }
                                        this.EnableInfoStrip(true);
                                        this.EnableMainMenu(true);
                                        HideOverlayTime = false;
                                    }
                                }

                                if (stat.ShowEnd < this.startupTime && this.askedPreviousShows == 2) {
                                    continue;
                                }

                                if (stat.ShowEnd < this.startupTime) {
                                    if (this.useLinkedProfiles) {
                                        profile = this.GetLinkedProfileId(stat.ShowNameId, stat.PrivateLobby, this.IsCreativeShow(stat.ShowNameId));
                                    }
                                    this.SetProfileMenu(profile);
                                }

                                if (stat.Round == 1) {
                                    this.nextShowID++;
                                    this.lastAddedShow = stat.Start;
                                }
                                stat.ShowID = nextShowID;
                                stat.Profile = profile;

                                if (stat.UseShareCode && string.IsNullOrEmpty(stat.CreativeShareCode) && !LevelStats.ALL.ContainsKey(stat.Name)) {
                                    try {
                                        JsonElement resData = this.GetApiData(this.FALLGUYSDB_API_URL, $"creative/{stat.ShowNameId}.json").GetProperty("data").GetProperty("snapshot");
                                        JsonElement versionMetadata = resData.GetProperty("version_metadata");
                                        string[] creativeAuthorInfo = this.FindCreativeAuthor(resData.GetProperty("author").GetProperty("name_per_platform"));
                                        stat.CreativeShareCode = resData.GetProperty("share_code").GetString();
                                        stat.CreativeOnlinePlatformId = creativeAuthorInfo[0];
                                        stat.CreativeAuthor = creativeAuthorInfo[1];
                                        stat.CreativeVersion = versionMetadata.GetProperty("version").GetInt32();
                                        stat.CreativeStatus = versionMetadata.GetProperty("status").GetString();
                                        stat.CreativeTitle = versionMetadata.GetProperty("title").GetString();
                                        stat.CreativeDescription = versionMetadata.GetProperty("description").GetString();
                                        stat.CreativeMaxPlayer = versionMetadata.GetProperty("max_player_count").GetInt32();
                                        stat.CreativePlatformId = versionMetadata.GetProperty("platform_id").GetString();
                                        stat.CreativeLastModifiedDate = versionMetadata.GetProperty("last_modified_date").GetDateTime();
                                        stat.CreativePlayCount = resData.GetProperty("play_count").GetInt32();
                                    } catch {
                                        stat.CreativeShareCode = null;
                                        stat.CreativeOnlinePlatformId = null;
                                        stat.CreativeAuthor = null;
                                        stat.CreativeVersion = 0;
                                        stat.CreativeStatus = null;
                                        stat.CreativeTitle = null;
                                        stat.CreativeDescription = null;
                                        stat.CreativeMaxPlayer = 0;
                                        stat.CreativePlatformId = null;
                                        stat.CreativeLastModifiedDate = DateTime.MinValue;
                                        stat.CreativePlayCount = 0;
                                    }
                                }

                                this.RoundDetails.Insert(stat);
                                this.AllStats.Add(stat);

                                //Below is where reporting to fallaytics happen
                                //Must have enabled the setting to enable tracking
                                //Must not be a private lobby
                                //Must be a game that is played after FallGuysStats started
                                if (this.CurrentSettings.EnableFallalyticsReporting && !stat.PrivateLobby && stat.ShowEnd > this.startupTime) {
                                    Task.Run(() => { FallalyticsReporter.Report(stat, this.CurrentSettings.FallalyticsAPIKey); });
                                }
                            } else {
                                continue;
                            }
                        }

                        if (!stat.PrivateLobby) {
                            if (stat.Round == 1) {
                                this.Shows++;
                            }
                            this.Rounds++;
                        } else {
                            if (stat.Round == 1) {
                                this.CustomShows++;
                            }
                            this.CustomRounds++;
                        }
                        this.Duration += stat.End - stat.Start;

                        if (!stat.PrivateLobby) {
                            if (stat.Qualified) {
                                switch (stat.Tier) {
                                    case 0:
                                        this.PinkMedals++;
                                        break;
                                    case 1:
                                        this.GoldMedals++;
                                        break;
                                    case 2:
                                        this.SilverMedals++;
                                        break;
                                    case 3:
                                        this.BronzeMedals++;
                                        break;
                                }
                            } else {
                                this.EliminatedMedals++;
                            }
                        } else {
                            if (stat.Qualified) {
                                switch (stat.Tier) {
                                    case 0:
                                        this.CustomPinkMedals++;
                                        break;
                                    case 1:
                                        this.CustomGoldMedals++;
                                        break;
                                    case 2:
                                        this.CustomSilverMedals++;
                                        break;
                                    case 3:
                                        this.CustomBronzeMedals++;
                                        break;
                                }
                            } else {
                                this.CustomEliminatedMedals++;
                            }
                        }

                        this.Kudos += stat.Kudos;

                        // add new type of round to the rounds lookup
                        if (!this.StatLookup.ContainsKey(stat.Name)) {
                            string roundName = stat.Name;
                            if (roundName.StartsWith("round_", StringComparison.OrdinalIgnoreCase)) {
                                roundName = roundName.Substring(6).Replace('_', ' ');
                            } else {
                                roundName = roundName.Replace('_', ' ');
                            }

                            LevelStats newLevel = new LevelStats(roundName, LevelType.Creative, BestRecordType.Fastest, true, false, 0, Properties.Resources.round_creative_icon, Properties.Resources.round_creative_big_icon);

                            this.StatLookup.Add(stat.Name, newLevel);
                            this.StatDetails.Add(newLevel);
                            this.gridDetails.DataSource = null;
                            this.gridDetails.DataSource = this.StatDetails;
                        }

                        stat.ToLocalTime();
                        LevelStats levelStats = this.StatLookup[stat.Name];

                        if (!stat.PrivateLobby) {
                            if (stat.IsFinal || stat.Crown) {
                                this.Finals++;
                                if (stat.Qualified) {
                                    this.Wins++;
                                }
                            }
                        }
                        levelStats.Add(stat);

                        if (!this.loadingExisting) {
                            if (stat.ShowEnd < this.startupTime) {
                                if (k == round.Count - 1) {
                                    this.onlyRefreshFilter = true;
                                    if (this.menuCustomRangeStats.Checked) {
                                        MenuStats_Click(this.menuCustomRangeStats, EventArgs.Empty);
                                    } else if (this.menuAllStats.Checked) {
                                        MenuStats_Click(this.menuAllStats, EventArgs.Empty);
                                    } else if (this.menuSeasonStats.Checked) {
                                        MenuStats_Click(this.menuSeasonStats, EventArgs.Empty);
                                    } else if (this.menuWeekStats.Checked) {
                                        MenuStats_Click(this.menuWeekStats, EventArgs.Empty);
                                    } else if (this.menuDayStats.Checked) {
                                        MenuStats_Click(this.menuDayStats, EventArgs.Empty);
                                    } else if (this.menuSessionStats.Checked) {
                                        MenuStats_Click(this.menuSessionStats, EventArgs.Empty);
                                    }
                                    this.onlyRefreshFilter = false;
                                }
                            } else if (stat.Round == round.Count) {
                                this.onlyRefreshFilter = true;
                                if (this.menuCustomRangeStats.Checked) {
                                    MenuStats_Click(this.menuCustomRangeStats, EventArgs.Empty);
                                } else if (this.menuAllStats.Checked) {
                                    MenuStats_Click(this.menuAllStats, EventArgs.Empty);
                                } else if (this.menuSeasonStats.Checked) {
                                    MenuStats_Click(this.menuSeasonStats, EventArgs.Empty);
                                } else if (this.menuWeekStats.Checked) {
                                    MenuStats_Click(this.menuWeekStats, EventArgs.Empty);
                                } else if (this.menuDayStats.Checked) {
                                    MenuStats_Click(this.menuDayStats, EventArgs.Empty);
                                } else if (this.menuSessionStats.Checked) {
                                    MenuStats_Click(this.menuSessionStats, EventArgs.Empty);
                                }
                                this.onlyRefreshFilter = false;
                                if (this.menuSoloStats.Checked && stat.InParty) {
                                    MenuStats_Click(this.menuSoloStats, EventArgs.Empty);
                                } else if (this.menuPartyStats.Checked && !stat.InParty) {
                                    MenuStats_Click(this.menuPartyStats, EventArgs.Empty);
                                }
                            }
                        }
                    }

                    if (!this.loadingExisting) {
                        this.StatsDB.Commit();
                    }
                }

                lock (this.CurrentRound) {
                    this.CurrentRound.Clear();
                    for (int i = round.Count - 1; i >= 0; i--) {
                        RoundInfo info = round[i];
                        this.CurrentRound.Insert(0, info);
                        if (info.Round == 1) {
                            break;
                        }
                    }
                }

                if (!this.Disposing && !this.IsDisposed) {
                    try {
                        this.UpdateTotals();
                    } catch {
                        // ignored
                    }
                }
            } catch (Exception ex) {
                MessageBox.Show(this, ex.ToString(), $"{Multilingual.GetWord("message_program_error_caption")}",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private bool IsInStatsFilter(RoundInfo info) {
            return (this.menuCustomRangeStats.Checked && info.Start >= this.customfilterRangeStart && info.Start <= this.customfilterRangeEnd) ||
                    this.menuAllStats.Checked ||
                   (this.menuSeasonStats.Checked && info.Start > SeasonStart) ||
                   (this.menuWeekStats.Checked && info.Start > WeekStart) ||
                   (this.menuDayStats.Checked && info.Start > DayStart) ||
                   (this.menuSessionStats.Checked && info.Start > SessionStart);
        }
        private bool IsInPartyFilter(RoundInfo info) {
            return this.menuAllPartyStats.Checked ||
                   (this.menuSoloStats.Checked && !info.InParty) ||
                   (this.menuPartyStats.Checked && info.InParty);
        }
        public string GetCurrentFilterName() {
            if (this.menuCustomRangeStats.Checked && this.selectedCustomTemplateSeason > -1) {
                return (this.selectedCustomTemplateSeason >= 0 && this.selectedCustomTemplateSeason <= 5) ? $"S{this.selectedCustomTemplateSeason + 1}" :
                        (this.selectedCustomTemplateSeason > 5) ? $"SS{this.selectedCustomTemplateSeason - 5}" :
                        Multilingual.GetWord("main_custom_range");
            } else {
                return this.menuCustomRangeStats.Checked ? Multilingual.GetWord("main_custom_range") :
                        this.menuAllStats.Checked ? Multilingual.GetWord("main_all") :
                        this.menuSeasonStats.Checked ? Multilingual.GetWord("main_season") :
                        this.menuWeekStats.Checked ? Multilingual.GetWord("main_week") :
                        this.menuDayStats.Checked ? Multilingual.GetWord("main_day") : Multilingual.GetWord("main_session");
            }
        }
        public string GetCurrentProfileName() {
            if (this.AllProfiles.Count == 0) return String.Empty;
            return this.AllProfiles.Find(p => p.ProfileId == this.GetCurrentProfileId()).ProfileName;
        }
        public int GetCurrentProfileId() {
            return this.currentProfile;
        }
        private int GetProfileIdFromName(string profileName) {
            return this.AllProfiles.Find(p => p.ProfileName.Equals(profileName)).ProfileId;
        }
        private string GetCurrentProfileLinkedShowId() {
            string currentProfileLinkedShowId = this.AllProfiles.Find(p => p.ProfileId == this.GetCurrentProfileId()).LinkedShowId;
            return !string.IsNullOrEmpty(currentProfileLinkedShowId) ? currentProfileLinkedShowId : string.Empty;
        }
        private string GetLinkedShowId(string showId) {
            switch (showId) {
                case "turbo_show":
                    return "main_show";
                case "squadcelebration":
                case "event_day_at_races_squads_template":
                    return "squads_4player";
                case "invisibeans_template":
                case "invisibeans_pistachio_template":
                    return "invisibeans_mode";
                default:
                    return showId;
            }
        }
        private int GetLinkedProfileId(string showId, bool isPrivateLobbies, bool isCreativeShow) {
            if (string.IsNullOrEmpty(showId)) { return 0; }

            showId = this.GetLinkedShowId(showId);

            for (int i = 0; i < this.AllProfiles.Count; i++) {
                if (isPrivateLobbies) {
                    if (!string.IsNullOrEmpty(this.AllProfiles[i].LinkedShowId) && this.AllProfiles[i].LinkedShowId.Equals("private_lobbies")) {
                        return this.AllProfiles[i].ProfileId;
                    }
                } else {
                    if (isCreativeShow) {
                        if (!string.IsNullOrEmpty(this.AllProfiles[i].LinkedShowId) && this.AllProfiles[i].LinkedShowId.Equals("fall_guys_creative_mode")) {
                            return this.AllProfiles[i].ProfileId;
                        }
                    } else {
                        if (!string.IsNullOrEmpty(this.AllProfiles[i].LinkedShowId) && showId.IndexOf(this.AllProfiles[i].LinkedShowId, StringComparison.OrdinalIgnoreCase) != -1) {
                            return this.AllProfiles[i].ProfileId;
                        }
                    }
                }
            }
            if (isPrivateLobbies) { // return corresponding linked profile when possible if no linked "private_lobbies" profile was found
                for (int j = 0; j < this.AllProfiles.Count; j++) {
                    if (!string.IsNullOrEmpty(this.AllProfiles[j].LinkedShowId) && showId.IndexOf(this.AllProfiles[j].LinkedShowId, StringComparison.OrdinalIgnoreCase) != -1) {
                        return this.AllProfiles[j].ProfileId;
                    }
                }
            }
            // return ProfileId 0 if no linked profile was found/matched
            return 0;
        }
        public void SetLinkedProfileMenu(string showId, bool isPrivateLobbies, bool isCreativeShow) {
            if ((this.AllProfiles.Count <= 1) || string.IsNullOrEmpty(showId)) { return; }

            showId = this.GetLinkedShowId(showId);

            if (this.GetCurrentProfileLinkedShowId().Equals(showId)) { return; }

            for (int i = 0; i < this.AllProfiles.Count; i++) {
                if (isPrivateLobbies) {
                    if (!string.IsNullOrEmpty(this.AllProfiles[i].LinkedShowId) && this.AllProfiles[i].LinkedShowId.Equals("private_lobbies")) {
                        ToolStripMenuItem item = this.ProfileMenuItems[this.AllProfiles.Count - 1 - i];
                        if (!item.Checked) { this.MenuStats_Click(item, EventArgs.Empty); }
                        return;
                    }
                } else {
                    if (isCreativeShow) {
                        if (!string.IsNullOrEmpty(this.AllProfiles[i].LinkedShowId) && this.AllProfiles[i].LinkedShowId.Equals("fall_guys_creative_mode")) {
                            ToolStripMenuItem item = this.ProfileMenuItems[this.AllProfiles.Count - 1 - i];
                            if (!item.Checked) { this.MenuStats_Click(item, EventArgs.Empty); }
                            return;
                        }
                    } else {
                        if (!string.IsNullOrEmpty(this.AllProfiles[i].LinkedShowId) && showId.IndexOf(this.AllProfiles[i].LinkedShowId, StringComparison.OrdinalIgnoreCase) != -1) {
                            ToolStripMenuItem item = this.ProfileMenuItems[this.AllProfiles.Count - 1 - i];
                            if (!item.Checked) { this.MenuStats_Click(item, EventArgs.Empty); }
                            return;
                        }
                    }
                }
            }
            if (isPrivateLobbies) { // select corresponding linked profile when possible if no linked "private_lobbies" profile was found
                for (int j = 0; j < this.AllProfiles.Count; j++) {
                    if (!string.IsNullOrEmpty(this.AllProfiles[j].LinkedShowId) && showId.IndexOf(this.AllProfiles[j].LinkedShowId, StringComparison.OrdinalIgnoreCase) != -1) {
                        ToolStripMenuItem item = this.ProfileMenuItems[this.AllProfiles.Count - 1 - j];
                        if (!item.Checked) { this.MenuStats_Click(item, EventArgs.Empty); }
                        return;
                    }
                }
            }
            // select ProfileId 0 if no linked profile was found/matched
            for (int k = 0; k < this.AllProfiles.Count; k++) {
                if (this.AllProfiles[k].ProfileId == 0) {
                    ToolStripMenuItem item = this.ProfileMenuItems[this.AllProfiles.Count - 1 - k];
                    if (!item.Checked) { this.MenuStats_Click(item, EventArgs.Empty); }
                    return;
                }
            }
        }
        private void SetProfileMenu(int profile) {
            ToolStripMenuItem tsmi = this.menuProfile.DropDownItems[$@"menuProfile{profile}"] as ToolStripMenuItem;
            if (tsmi.Checked) { return; }

            this.MenuStats_Click(tsmi, EventArgs.Empty);
        }
        private void SetCurrentProfileIcon(bool linked) {
            if (this.CurrentSettings.AutoChangeProfile) {
                this.lblCurrentProfile.Image = linked ? Properties.Resources.profile2_linked_icon : Properties.Resources.profile2_unlinked_icon;
                this.overlay.SetCurrentProfileForeColor(linked ? Color.GreenYellow
                    : string.IsNullOrEmpty(this.CurrentSettings.OverlayFontColorSerialized) ? Color.White
                    : (Color)new ColorConverter().ConvertFromString(this.CurrentSettings.OverlayFontColorSerialized));
            } else {
                this.lblCurrentProfile.Image = Properties.Resources.profile2_icon;
                this.overlay.SetCurrentProfileForeColor(string.IsNullOrEmpty(this.CurrentSettings.OverlayFontColorSerialized) ? Color.White
                    : (Color)new ColorConverter().ConvertFromString(this.CurrentSettings.OverlayFontColorSerialized));
            }
        }
        public StatSummary GetLevelInfo(string roundName, BestRecordType recordType) {
            StatSummary summary = new StatSummary {
                AllWins = 0,
                TotalShows = 0,
                TotalPlays = 0,
                TotalWins = 0,
                TotalFinals = 0
            };

            int lastShow = -1;

            if (!this.StatLookup.TryGetValue(roundName, out LevelStats currentLevel)) {
                currentLevel = new LevelStats(roundName, LevelType.Creative, BestRecordType.Fastest, true, false, 0, Properties.Resources.round_creative_icon, Properties.Resources.round_creative_big_icon);
            }

            for (int i = 0; i < this.AllStats.Count; i++) {
                RoundInfo info = this.AllStats[i];
                if (info.Profile != this.currentProfile) { continue; }

                TimeSpan finishTime = info.Finish.GetValueOrDefault(info.Start) - info.Start;
                bool hasFinishTime = finishTime.TotalSeconds > 1.1 ? true : false;
                bool hasLevelDetails = this.StatLookup.TryGetValue(info.Name, out LevelStats levelDetails);
                bool isCurrentLevel = currentLevel.Name.Equals(hasLevelDetails ? levelDetails.Name : info.Name, StringComparison.OrdinalIgnoreCase);

                int startRoundShowId = info.ShowID;
                RoundInfo endRound = info;
                for (int j = i + 1; j < this.AllStats.Count; j++) {
                    if (this.AllStats[j].ShowID != startRoundShowId) {
                        break;
                    }
                    endRound = this.AllStats[j];
                }

                bool isInWinsFilter = !endRound.PrivateLobby &&
                                      (this.CurrentSettings.WinsFilter == 0 ||
                                      (this.CurrentSettings.WinsFilter == 1 && this.IsInStatsFilter(endRound) && this.IsInPartyFilter(info)) ||
                                      (this.CurrentSettings.WinsFilter == 2 && endRound.Start > SeasonStart) ||
                                      (this.CurrentSettings.WinsFilter == 3 && endRound.Start > WeekStart) ||
                                      (this.CurrentSettings.WinsFilter == 4 && endRound.Start > DayStart) ||
                                      (this.CurrentSettings.WinsFilter == 5 && endRound.Start > SessionStart));
                bool isInQualifyFilter = (!endRound.PrivateLobby || (endRound.UseShareCode && !LevelStats.ALL.ContainsKey(endRound.Name))) &&
                                         (this.CurrentSettings.QualifyFilter == 0 ||
                                         (this.CurrentSettings.QualifyFilter == 1 && this.IsInStatsFilter(endRound) && this.IsInPartyFilter(info)) ||
                                         (this.CurrentSettings.QualifyFilter == 2 && endRound.Start > SeasonStart) ||
                                         (this.CurrentSettings.QualifyFilter == 3 && endRound.Start > WeekStart) ||
                                         (this.CurrentSettings.QualifyFilter == 4 && endRound.Start > DayStart) ||
                                         (this.CurrentSettings.QualifyFilter == 5 && endRound.Start > SessionStart));
                bool isInFastestFilter = this.CurrentSettings.FastestFilter == 0 ||
                                         (this.CurrentSettings.FastestFilter == 1 && this.IsInStatsFilter(endRound) && this.IsInPartyFilter(info)) ||
                                         (this.CurrentSettings.FastestFilter == 2 && endRound.Start > SeasonStart) ||
                                         (this.CurrentSettings.FastestFilter == 3 && endRound.Start > WeekStart) ||
                                         (this.CurrentSettings.FastestFilter == 4 && endRound.Start > DayStart) ||
                                         (this.CurrentSettings.FastestFilter == 5 && endRound.Start > SessionStart);

                if (info.ShowID != lastShow) {
                    lastShow = info.ShowID;
                    if (isInWinsFilter) {
                        summary.TotalShows++;
                    }
                }

                if (isCurrentLevel) {
                    if (isInQualifyFilter) {
                        summary.TotalPlays++;
                    }

                    if (isInFastestFilter) {
                        if ((!hasLevelDetails || recordType == BestRecordType.HighScore)
                            && info.Score.HasValue && (!summary.BestScore.HasValue || info.Score.Value > summary.BestScore.Value)) {
                            summary.BestScore = info.Score;
                        }
                    }
                }

                if (ReferenceEquals(info, endRound) && ((hasLevelDetails && levelDetails.IsFinal) || info.Crown) && !endRound.PrivateLobby) {
                    if (info.IsFinal) {
                        summary.CurrentFinalStreak++;
                        if (summary.BestFinalStreak < summary.CurrentFinalStreak) {
                            summary.BestFinalStreak = summary.CurrentFinalStreak;
                        }
                    }
                }

                if (isCurrentLevel) {
                    if (isInFastestFilter) {
                        if (hasFinishTime && (!summary.BestFinish.HasValue || summary.BestFinish.Value > finishTime)) {
                            summary.BestFinish = finishTime;
                        }
                        if (hasFinishTime && (!summary.LongestFinish.HasValue || summary.LongestFinish.Value < finishTime)) {
                            summary.LongestFinish = finishTime;
                        }
                    }

                    if (hasFinishTime && (!summary.BestFinishOverall.HasValue || summary.BestFinishOverall.Value > finishTime)) {
                        summary.BestFinishOverall = finishTime;
                    }
                    if (hasFinishTime && (!summary.LongestFinishOverall.HasValue || summary.LongestFinishOverall.Value < finishTime)) {
                        summary.LongestFinishOverall = finishTime;
                    }
                }

                if (info.Qualified) {
                    if (hasLevelDetails && (info.IsFinal || info.Crown)) {
                        if (!info.PrivateLobby) {
                            summary.AllWins++;
                        }

                        if (isInWinsFilter) {
                            summary.TotalWins++;
                            summary.TotalFinals++;
                        }

                        if (!info.PrivateLobby) {
                            summary.CurrentStreak++;
                            if (summary.CurrentStreak > summary.BestStreak) {
                                summary.BestStreak = summary.CurrentStreak;
                            }
                        }
                    }

                    if (isCurrentLevel) {
                        if (isInQualifyFilter) {
                            if (info.Tier == (int)QualifyTier.Gold) {
                                summary.TotalGolds++;
                            }
                            summary.TotalQualify++;
                        }
                    }
                } else if (!info.PrivateLobby) {
                    if (!info.IsFinal && !info.Crown) {
                        summary.CurrentFinalStreak = 0;
                    }
                    summary.CurrentStreak = 0;
                    if (isInWinsFilter && hasLevelDetails && (info.IsFinal || info.Crown)) {
                        summary.TotalFinals++;
                    }
                }
            }

            return summary;
        }
        private void ClearTotals() {
            this.Wins = 0;
            this.Shows = 0;
            this.Rounds = 0;
            this.CustomRounds = 0;
            this.Duration = TimeSpan.Zero;
            this.CustomShows = 0;
            this.Finals = 0;
            this.GoldMedals = 0;
            this.SilverMedals = 0;
            this.BronzeMedals = 0;
            this.PinkMedals = 0;
            this.EliminatedMedals = 0;
            this.CustomGoldMedals = 0;
            this.CustomSilverMedals = 0;
            this.CustomBronzeMedals = 0;
            this.CustomPinkMedals = 0;
            this.CustomEliminatedMedals = 0;
            this.Kudos = 0;
        }
        private void UpdateTotals() {
            try {
                this.lblCurrentProfile.Text = $"{this.GetCurrentProfileName()}";
                this.lblCurrentProfile.ToolTipText = $"{Multilingual.GetWord("profile_change_tooltiptext")}";
                this.lblTotalShows.Text = $"{this.Shows}{Multilingual.GetWord("main_inning")}";
                if (this.CustomShows > 0) this.lblTotalShows.Text += $" ({Multilingual.GetWord("main_profile_custom")} : {this.CustomShows}{Multilingual.GetWord("main_inning")})";
                this.lblTotalShows.ToolTipText = $"{Multilingual.GetWord("shows_detail_tooltiptext")}";
                this.lblTotalRounds.Text = $"{this.Rounds}{Multilingual.GetWord("main_round")}";
                if (this.CustomRounds > 0) this.lblTotalRounds.Text += $" ({Multilingual.GetWord("main_profile_custom")} : {this.CustomRounds}{Multilingual.GetWord("main_round")})";
                this.lblTotalRounds.ToolTipText = $"{Multilingual.GetWord("rounds_detail_tooltiptext")}";
                this.lblTotalTime.Text = $"{(int)this.Duration.TotalHours}{Multilingual.GetWord("main_hour")}{this.Duration:mm}{Multilingual.GetWord("main_min")}{this.Duration:ss}{Multilingual.GetWord("main_sec")}";
                this.lblTotalTime.ToolTipText = $"{Multilingual.GetWord("stats_detail_tooltiptext")}";
                float winChance = (float)this.Wins * 100 / (this.Shows == 0 ? 1 : this.Shows);
                this.lblTotalWins.Text = $"{this.Wins}{Multilingual.GetWord("main_win")} ({Math.Truncate(winChance * 10) / 10} %)";
                this.lblTotalWins.ToolTipText = $"{Multilingual.GetWord("wins_detail_tooltiptext")}";
                float finalChance = (float)this.Finals * 100 / (this.Shows == 0 ? 1 : this.Shows);
                this.lblTotalFinals.Text = $"{this.Finals}{Multilingual.GetWord("main_inning")} ({Math.Truncate(finalChance * 10) / 10} %)";
                this.lblTotalFinals.ToolTipText = $"{Multilingual.GetWord("finals_detail_tooltiptext")}";
                this.lblGoldMedal.Text = $"{this.GoldMedals}";
                if (this.CustomGoldMedals > 0) this.lblGoldMedal.Text += $" ({this.CustomGoldMedals})";
                this.lblSilverMedal.Text = $"{this.SilverMedals}";
                if (this.CustomSilverMedals > 0) this.lblSilverMedal.Text += $" ({this.CustomSilverMedals})";
                this.lblBronzeMedal.Text = $"{this.BronzeMedals}";
                if (this.CustomBronzeMedals > 0) this.lblBronzeMedal.Text += $" ({this.CustomBronzeMedals})";
                this.lblPinkMedal.Text = $"{this.PinkMedals}";
                if (this.CustomPinkMedals > 0) this.lblPinkMedal.Text += $" ({this.CustomPinkMedals})";
                this.lblEliminatedMedal.Text = $"{this.EliminatedMedals}";
                if (this.CustomEliminatedMedals > 0) this.lblEliminatedMedal.Text += $" ({this.CustomEliminatedMedals})";
                this.lblGoldMedal.Visible = this.GoldMedals != 0 || this.CustomGoldMedals != 0;
                this.lblSilverMedal.Visible = this.SilverMedals != 0 || this.CustomSilverMedals != 0;
                this.lblBronzeMedal.Visible = this.BronzeMedals != 0 || this.CustomBronzeMedals != 0;
                this.lblPinkMedal.Visible = this.PinkMedals != 0 || this.CustomPinkMedals != 0;
                this.lblEliminatedMedal.Visible = this.EliminatedMedals != 0 || this.CustomEliminatedMedals != 0;

                this.lblKudos.Text = $"{this.Kudos}";
                this.lblKudos.Visible = this.Kudos != 0;
                this.gridDetails.Refresh();
            } catch (Exception ex) {
                MessageBox.Show(this, ex.ToString(), $"{Multilingual.GetWord("message_program_error_caption")}",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void ShowNotification(string title, string text, ToolTipIcon toolTipIcon) {
            MessageBox.Show(this, text, title,
                MessageBoxButtons.OK, toolTipIcon == ToolTipIcon.None ? MessageBoxIcon.None :
                                                     toolTipIcon == ToolTipIcon.Error ? MessageBoxIcon.Error :
                                                     toolTipIcon == ToolTipIcon.Info ? MessageBoxIcon.Information :
                                                     toolTipIcon == ToolTipIcon.Warning ? MessageBoxIcon.Warning : MessageBoxIcon.None);
        }
        // public void AllocOverlayCustomTooltip() {
        //     this.ocmtt = new MetroToolTip();
        //     this.ocmtt.OwnerDraw = true;
        //     this.ocmtt.Draw += this.ocmtt_Draw;
        // }
        // public void ShowOverlayCustomTooltip(string message, IWin32Window window, Point position, int duration = -1) {
        //     if (duration == -1) {
        //         this.ocmtt.Show(message, window, position);
        //     } else {
        //         this.ocmtt.Show(message, window, position, duration);
        //     }
        // }
        // public void HideOverlayCustomTooltip(IWin32Window window) {
        //     this.ocmtt.Hide(window);
        // }
        public void ShowCustomTooltip(string message, IWin32Window window, Point position, int duration = -1) {
            if (duration == -1) {
                this.cmtt.Show(message, window, position);
            } else {
                this.cmtt.Show(message, window, position, duration);
            }
        }
        public void HideCustomTooltip(IWin32Window window) {
            this.cmtt.Hide(window);
        }

        public void AllocTooltip() {
            this.mtt = new MetroToolTip {
                Theme = MetroThemeStyle.Dark
            };
        }
        public void ShowTooltip(string message, IWin32Window window, Point position, int duration = -1) {
            if (duration == -1) {
                this.mtt.Show(message, window, position);
            } else {
                this.mtt.Show(message, window, position, duration);
            }
        }
        public void HideTooltip(IWin32Window window) {
            this.mtt.Hide(window);
        }

        private void GridDetails_DataSourceChanged(object sender, EventArgs e) {
            this.SetMainDataGridView();
        }
        private int GetDataGridViewColumnWidth(string columnName, string columnText) {
            int sizeOfText;
            switch (columnName) {
                case "RoundIcon":
                    sizeOfText = 13;
                    break;
                case "Name":
                    return 0;
                case "Played":
                    sizeOfText = TextRenderer.MeasureText(columnText, this.dataGridViewCellStyle1.Font).Width;
                    break;
                case "Qualified":
                    sizeOfText = TextRenderer.MeasureText(columnText, this.dataGridViewCellStyle1.Font).Width;
                    sizeOfText += CurrentLanguage == 2 || CurrentLanguage == 4 ? 5 : 0;
                    break;
                case "Gold":
                    sizeOfText = TextRenderer.MeasureText(columnText, this.dataGridViewCellStyle1.Font).Width;
                    sizeOfText += CurrentLanguage == 1 ? 12 : CurrentLanguage == 4 ? 5 : 0;
                    break;
                case "Silver":
                    sizeOfText = TextRenderer.MeasureText(columnText, this.dataGridViewCellStyle1.Font).Width;
                    sizeOfText += CurrentLanguage == 4 ? 5 : 0;
                    break;
                case "Bronze":
                    sizeOfText = TextRenderer.MeasureText(columnText, this.dataGridViewCellStyle1.Font).Width;
                    sizeOfText += CurrentLanguage == 4 ? 5 : 0;
                    break;
                case "Kudos":
                    sizeOfText = TextRenderer.MeasureText(columnText, this.dataGridViewCellStyle1.Font).Width;
                    break;
                case "Fastest":
                    sizeOfText = TextRenderer.MeasureText(columnText, this.dataGridViewCellStyle1.Font).Width;
                    sizeOfText += CurrentLanguage == 4 ? 20 : 0;
                    break;
                case "Longest":
                    sizeOfText = TextRenderer.MeasureText(columnText, this.dataGridViewCellStyle1.Font).Width;
                    sizeOfText += CurrentLanguage == 4 ? 20 : 0;
                    break;
                case "AveFinish":
                    sizeOfText = TextRenderer.MeasureText(columnText, this.dataGridViewCellStyle1.Font).Width;
                    sizeOfText += CurrentLanguage == 4 ? 20 : 0;
                    break;
                default:
                    return 0;
            }

            return sizeOfText + 24;
        }
        private void InitMainDataGridView() {
            this.dataGridViewCellStyle1.Font = Overlay.GetMainFont(10);
            this.dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            this.dataGridViewCellStyle1.BackColor = Color.LightGray;
            //this.dataGridViewCellStyle1.ForeColor = Color.Black;
            //this.dataGridViewCellStyle1.SelectionBackColor = Color.Cyan;
            //this.dataGridViewCellStyle1.SelectionForeColor = Color.Black;
            this.dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            this.gridDetails.ColumnHeadersDefaultCellStyle = this.dataGridViewCellStyle1;
            this.gridDetails.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.gridDetails.ColumnHeadersHeight = 20;

            this.dataGridViewCellStyle2.Font = Overlay.GetMainFont(12);
            this.dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            //this.dataGridViewCellStyle2.BackColor = Color.White;
            //this.dataGridViewCellStyle2.ForeColor = Color.Black;
            //this.dataGridViewCellStyle2.SelectionBackColor = Color.DeepSkyBlue;
            //this.dataGridViewCellStyle2.SelectionForeColor = Color.Black;
            this.dataGridViewCellStyle2.WrapMode = DataGridViewTriState.False;
            this.gridDetails.DefaultCellStyle = this.dataGridViewCellStyle2;
            this.gridDetails.RowTemplate.Height = 25;

            this.gridDetails.DataSource = this.StatDetails;
        }
        private void SetMainDataGridView() {
            try {
                if (this.gridDetails.Columns.Count == 0) { return; }

                int pos = 0;
                this.gridDetails.Columns["RoundBigIcon"].Visible = false;
                this.gridDetails.Columns["AveKudos"].Visible = false;
                this.gridDetails.Columns["AveDuration"].Visible = false;
                this.gridDetails.Setup("RoundIcon", pos++, this.GetDataGridViewColumnWidth("RoundIcon", ""), "", DataGridViewContentAlignment.MiddleCenter);
                this.gridDetails.Columns["RoundIcon"].Resizable = DataGridViewTriState.False;
                this.gridDetails.Setup("Name", pos++, this.GetDataGridViewColumnWidth("Name", Multilingual.GetWord("main_round_name")), Multilingual.GetWord("main_round_name"), DataGridViewContentAlignment.MiddleLeft);
                this.gridDetails.Setup("Played", pos++, this.GetDataGridViewColumnWidth("Played", Multilingual.GetWord("main_played")), Multilingual.GetWord("main_played"), DataGridViewContentAlignment.MiddleRight);
                this.gridDetails.Setup("Qualified", pos++, this.GetDataGridViewColumnWidth("Qualified", Multilingual.GetWord("main_qualified")), Multilingual.GetWord("main_qualified"), DataGridViewContentAlignment.MiddleRight);
                this.gridDetails.Setup("Gold", pos++, this.GetDataGridViewColumnWidth("Gold", Multilingual.GetWord("main_gold")), Multilingual.GetWord("main_gold"), DataGridViewContentAlignment.MiddleRight);
                this.gridDetails.Setup("Silver", pos++, this.GetDataGridViewColumnWidth("Silver", Multilingual.GetWord("main_silver")), Multilingual.GetWord("main_silver"), DataGridViewContentAlignment.MiddleRight);
                this.gridDetails.Setup("Bronze", pos++, this.GetDataGridViewColumnWidth("Bronze", Multilingual.GetWord("main_bronze")), Multilingual.GetWord("main_bronze"), DataGridViewContentAlignment.MiddleRight);
                this.gridDetails.Setup("Kudos", pos++, this.GetDataGridViewColumnWidth("Kudos", Multilingual.GetWord("main_kudos")), Multilingual.GetWord("main_kudos"), DataGridViewContentAlignment.MiddleRight);
                this.gridDetails.Setup("Fastest", pos++, this.GetDataGridViewColumnWidth("Fastest", Multilingual.GetWord("main_best")), Multilingual.GetWord("main_best"), DataGridViewContentAlignment.MiddleRight);
                this.gridDetails.Setup("Longest", pos++, this.GetDataGridViewColumnWidth("Longest", Multilingual.GetWord("main_longest")), Multilingual.GetWord("main_longest"), DataGridViewContentAlignment.MiddleRight);
                this.gridDetails.Setup("AveFinish", pos, this.GetDataGridViewColumnWidth("AveFinish", Multilingual.GetWord("main_ave_finish")), Multilingual.GetWord("main_ave_finish"), DataGridViewContentAlignment.MiddleRight);
            } catch (Exception ex) {
                MessageBox.Show(this, ex.ToString(), $"{Multilingual.GetWord("message_program_error_caption")}",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void SetMainDataGridViewOrder() {
            int pos = 0;
            this.gridDetails.Columns["RoundIcon"].DisplayIndex = pos++;
            this.gridDetails.Columns["Name"].DisplayIndex = pos++;
            this.gridDetails.Columns["Played"].DisplayIndex = pos++;
            this.gridDetails.Columns["Qualified"].DisplayIndex = pos++;
            this.gridDetails.Columns["Gold"].DisplayIndex = pos++;
            this.gridDetails.Columns["Silver"].DisplayIndex = pos++;
            this.gridDetails.Columns["Bronze"].DisplayIndex = pos++;
            this.gridDetails.Columns["Kudos"].DisplayIndex = pos++;
            this.gridDetails.Columns["Fastest"].DisplayIndex = pos++;
            this.gridDetails.Columns["Longest"].DisplayIndex = pos++;
            this.gridDetails.Columns["AveFinish"].DisplayIndex = pos;
        }
        private void GridDetails_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e) {
            try {
                if (e.RowIndex < 0) { return; }

                LevelStats levelStats = this.gridDetails.Rows[e.RowIndex].DataBoundItem as LevelStats;
                float fBrightness = 0.7F;
                switch (this.gridDetails.Columns[e.ColumnIndex].Name) {
                    case "RoundIcon":
                        if (levelStats.IsFinal && levelStats.Type != LevelType.Invisibeans) {
                            e.CellStyle.BackColor = this.Theme == MetroThemeStyle.Light
                                ? Color.FromArgb(255, 245, 205)
                                : Color.FromArgb((int)(255 * fBrightness), (int)(245 * fBrightness), (int)(205 * fBrightness));
                            break;
                        }
                        switch (levelStats.Type) {
                            case LevelType.CreativeRace:
                            case LevelType.Race:
                                e.CellStyle.BackColor = this.Theme == MetroThemeStyle.Light
                                    ? Color.FromArgb(205, 255, 225)
                                    : Color.FromArgb((int)(205 * fBrightness), (int)(255 * fBrightness), (int)(225 * fBrightness));
                                break;
                            case LevelType.Survival:
                                e.CellStyle.BackColor = this.Theme == MetroThemeStyle.Light
                                    ? Color.FromArgb(250, 205, 255)
                                    : Color.FromArgb((int)(250 * fBrightness), (int)(205 * fBrightness), (int)(255 * fBrightness));
                                break;
                            case LevelType.Hunt:
                                e.CellStyle.BackColor = this.Theme == MetroThemeStyle.Light
                                    ? Color.FromArgb(205, 225, 255)
                                    : Color.FromArgb((int)(205 * fBrightness), (int)(225 * fBrightness), (int)(255 * fBrightness));
                                break;
                            case LevelType.Logic:
                                e.CellStyle.BackColor = this.Theme == MetroThemeStyle.Light
                                    ? Color.FromArgb(205, 250, 255)
                                    : Color.FromArgb((int)(205 * fBrightness), (int)(250 * fBrightness), (int)(255 * fBrightness));
                                break;
                            case LevelType.Team:
                                e.CellStyle.BackColor = this.Theme == MetroThemeStyle.Light
                                    ? Color.FromArgb(255, 220, 205)
                                    : Color.FromArgb((int)(255 * fBrightness), (int)(220 * fBrightness), (int)(205 * fBrightness));
                                break;
                            case LevelType.Invisibeans:
                                e.CellStyle.BackColor = this.Theme == MetroThemeStyle.Light
                                    ? Color.FromArgb(255, 255, 255)
                                    : Color.FromArgb((int)(255 * fBrightness), (int)(255 * fBrightness), (int)(255 * fBrightness));
                                break;
                            case LevelType.Creative:
                                e.CellStyle.BackColor = this.Theme == MetroThemeStyle.Light
                                    ? Color.FromArgb(255, 205, 235)
                                    : Color.FromArgb((int)(255 * fBrightness), (int)(205 * fBrightness), (int)(235 * fBrightness));
                                break;
                            case LevelType.Unknown:
                                e.CellStyle.BackColor = this.Theme == MetroThemeStyle.Light
                                    ? Color.LightGray
                                    : Color.DarkGray;
                                break;
                        }
                        break;
                    case "Name":
                        e.CellStyle.ForeColor = Color.Black;
                        this.gridDetails.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText = Multilingual.GetWord("level_detail_tooltiptext");
                        if (levelStats.IsFinal && levelStats.Type != LevelType.Invisibeans) {
                            e.CellStyle.BackColor = this.Theme == MetroThemeStyle.Light
                                ? Color.FromArgb(255, 245, 205)
                                : Color.FromArgb((int)(255 * fBrightness), (int)(245 * fBrightness), (int)(205 * fBrightness));
                            break;
                        }
                        switch (levelStats.Type) {
                            case LevelType.CreativeRace:
                            case LevelType.Race:
                                e.CellStyle.BackColor = this.Theme == MetroThemeStyle.Light
                                    ? Color.FromArgb(205, 255, 225)
                                    : Color.FromArgb((int)(205 * fBrightness), (int)(255 * fBrightness), (int)(225 * fBrightness));
                                break;
                            case LevelType.Survival:
                                e.CellStyle.BackColor = this.Theme == MetroThemeStyle.Light
                                    ? Color.FromArgb(250, 205, 255)
                                    : Color.FromArgb((int)(250 * fBrightness), (int)(205 * fBrightness), (int)(255 * fBrightness));
                                break;
                            case LevelType.Hunt:
                                e.CellStyle.BackColor = this.Theme == MetroThemeStyle.Light
                                    ? Color.FromArgb(205, 225, 255)
                                    : Color.FromArgb((int)(205 * fBrightness), (int)(225 * fBrightness), (int)(255 * fBrightness));
                                break;
                            case LevelType.Logic:
                                e.CellStyle.BackColor = this.Theme == MetroThemeStyle.Light
                                    ? Color.FromArgb(205, 250, 255)
                                    : Color.FromArgb((int)(205 * fBrightness), (int)(250 * fBrightness), (int)(255 * fBrightness));
                                break;
                            case LevelType.Team:
                                e.CellStyle.BackColor = this.Theme == MetroThemeStyle.Light
                                    ? Color.FromArgb(255, 220, 205)
                                    : Color.FromArgb((int)(255 * fBrightness), (int)(220 * fBrightness), (int)(205 * fBrightness));
                                break;
                            case LevelType.Invisibeans:
                                e.CellStyle.BackColor = this.Theme == MetroThemeStyle.Light
                                    ? Color.FromArgb(255, 255, 255)
                                    : Color.FromArgb((int)(255 * fBrightness), (int)(255 * fBrightness), (int)(255 * fBrightness));
                                break;
                            case LevelType.Creative:
                                e.CellStyle.BackColor = this.Theme == MetroThemeStyle.Light
                                    ? Color.FromArgb(255, 205, 235)
                                    : Color.FromArgb((int)(255 * fBrightness), (int)(205 * fBrightness), (int)(235 * fBrightness));
                                break;
                            case LevelType.Unknown:
                                e.CellStyle.BackColor = this.Theme == MetroThemeStyle.Light
                                    ? Color.LightGray
                                    : Color.DarkGray;
                                break;
                        }
                        break;
                    case "Qualified": {
                            float qualifyChance = levelStats.Qualified * 100f / (levelStats.Played == 0 ? 1 : levelStats.Played);
                            if (this.CurrentSettings.ShowPercentages) {
                                e.Value = $"{Math.Truncate(qualifyChance * 10) / 10}%";
                                this.gridDetails.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText = $"{levelStats.Qualified}";
                            } else {
                                e.Value = levelStats.Qualified;
                                this.gridDetails.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText = $"{Math.Truncate(qualifyChance * 10) / 10}%";
                            }
                            break;
                        }
                    case "Gold": {
                            float goldChance = levelStats.Gold * 100f / (levelStats.Played == 0 ? 1 : levelStats.Played);
                            if (this.CurrentSettings.ShowPercentages) {
                                e.Value = $"{Math.Truncate(goldChance * 10) / 10}%";
                                this.gridDetails.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText = $"{levelStats.Gold}";
                            } else {
                                e.Value = levelStats.Gold;
                                this.gridDetails.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText = $"{Math.Truncate(goldChance * 10) / 10}%";
                            }
                            break;
                        }
                    case "Silver": {
                            float silverChance = levelStats.Silver * 100f / (levelStats.Played == 0 ? 1 : levelStats.Played);
                            if (this.CurrentSettings.ShowPercentages) {
                                e.Value = $"{Math.Truncate(silverChance * 10) / 10}%";
                                this.gridDetails.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText = $"{levelStats.Silver}";
                            } else {
                                e.Value = levelStats.Silver;
                                this.gridDetails.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText = $"{Math.Truncate(silverChance * 10) / 10}%";
                            }
                            break;
                        }
                    case "Bronze": {
                            float bronzeChance = levelStats.Bronze * 100f / (levelStats.Played == 0 ? 1 : levelStats.Played);
                            if (this.CurrentSettings.ShowPercentages) {
                                e.Value = $"{Math.Truncate(bronzeChance * 10) / 10}%";
                                this.gridDetails.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText = $"{levelStats.Bronze}";
                            } else {
                                e.Value = levelStats.Bronze;
                                this.gridDetails.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText = $"{Math.Truncate(bronzeChance * 10) / 10}%";
                            }
                            break;
                        }
                    case "AveFinish":
                        e.Value = levelStats.AveFinish.ToString("m\\:ss\\.ff");
                        break;
                    case "Fastest":
                        e.Value = levelStats.Fastest.ToString("m\\:ss\\.ff");
                        break;
                    case "Longest":
                        e.Value = levelStats.Longest.ToString("m\\:ss\\.ff");
                        break;
                }
            } catch (Exception ex) {
                MessageBox.Show(this, ex.ToString(), $"{Multilingual.GetWord("message_program_error_caption")}",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void GridDetails_CellMouseLeave(object sender, DataGridViewCellEventArgs e) {
            this.gridDetails.Cursor = Cursors.Default;
        }
        private void GridDetails_CellMouseEnter(object sender, DataGridViewCellEventArgs e) {
            try {
                if (e.RowIndex >= 0 && (this.gridDetails.Columns[e.ColumnIndex].Name == "Name" || this.gridDetails.Columns[e.ColumnIndex].Name == "RoundIcon")) {
                    this.gridDetails.Cursor = Cursors.Hand;
                } else {
                    this.gridDetails.Cursor = e.RowIndex >= 0 && !(this.gridDetails.Columns[e.ColumnIndex].Name == "Name" || this.gridDetails.Columns[e.ColumnIndex].Name == "RoundIcon")
                        ? this.Theme == MetroThemeStyle.Light ? new Cursor(Properties.Resources.transform_icon.GetHicon()) : new Cursor(Properties.Resources.transform_gray_icon.GetHicon())
                        : Cursors.Default;
                }
            } catch (Exception ex) {
                MessageBox.Show(this, ex.ToString(), $"{Multilingual.GetWord("message_program_error_caption")}",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void GridDetails_CellClick(object sender, DataGridViewCellEventArgs e) {
            try {
                if (e.RowIndex < 0) { return; }
                if (this.gridDetails.Columns[e.ColumnIndex].Name == "Name" || this.gridDetails.Columns[e.ColumnIndex].Name == "RoundIcon") {
                    LevelStats stats = this.gridDetails.Rows[e.RowIndex].DataBoundItem as LevelStats;
                    using (LevelDetails levelDetails = new LevelDetails {
                        LevelName = stats.Name,
                        RoundIcon = stats.RoundBigIcon,
                        StatsForm = this
                    }) {
                        List<RoundInfo> rounds = stats.Stats;
                        rounds.Sort();
                        levelDetails.RoundDetails = rounds;
                        this.EnableInfoStrip(false);
                        this.EnableMainMenu(false);
                        levelDetails.ShowDialog(this);
                        this.EnableInfoStrip(true);
                        this.EnableMainMenu(true);
                    }
                } else {
                    this.ToggleWinPercentageDisplay();
                }
            } catch (Exception ex) {
                MessageBox.Show(this, ex.ToString(), $"{Multilingual.GetWord("message_program_error_caption")}",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.EnableInfoStrip(true);
                this.EnableMainMenu(true);
            }
        }
        private void SortGridDetails(int columnIndex, bool isInitialize) {
            string columnName = this.gridDetails.Columns[columnIndex].Name;
            SortOrder sortOrder = isInitialize ? SortOrder.None : this.gridDetails.GetSortOrder(columnName);

            this.StatDetails.Sort((one, two) => {
                LevelType oneType = one.IsFinal && one.Type != LevelType.Invisibeans ? LevelType.Final : one.Type;
                LevelType twoType = two.IsFinal && two.Type != LevelType.Invisibeans ? LevelType.Final : two.Type;

                int typeCompare = this.CurrentSettings.IgnoreLevelTypeWhenSorting && sortOrder != SortOrder.None ? 0 : ((int)oneType).CompareTo((int)twoType);

                if (sortOrder == SortOrder.Descending) {
                    (one, two) = (two, one);
                }

                int nameCompare = $"{(one.IsCreative ? "#" : "")}{one.Name}".CompareTo($"{(two.IsCreative ? "#" : "")}{two.Name}");
                bool percents = this.CurrentSettings.ShowPercentages;
                if (typeCompare == 0 && sortOrder != SortOrder.None) {
                    switch (columnName) {
                        case "Played": typeCompare = one.Played.CompareTo(two.Played); break;
                        case "Qualified": typeCompare = ((double)one.Qualified / (one.Played > 0 && percents ? one.Played : 1)).CompareTo((double)two.Qualified / (two.Played > 0 && percents ? two.Played : 1)); break;
                        case "Gold": typeCompare = ((double)one.Gold / (one.Played > 0 && percents ? one.Played : 1)).CompareTo((double)two.Gold / (two.Played > 0 && percents ? two.Played : 1)); break;
                        case "Silver": typeCompare = ((double)one.Silver / (one.Played > 0 && percents ? one.Played : 1)).CompareTo((double)two.Silver / (two.Played > 0 && percents ? two.Played : 1)); break;
                        case "Bronze": typeCompare = ((double)one.Bronze / (one.Played > 0 && percents ? one.Played : 1)).CompareTo((double)two.Bronze / (two.Played > 0 && percents ? two.Played : 1)); break;
                        case "Kudos": typeCompare = one.Kudos.CompareTo(two.Kudos); break;
                        case "Fastest": typeCompare = one.Fastest.CompareTo(two.Fastest); break;
                        case "Longest": typeCompare = one.Longest.CompareTo(two.Longest); break;
                        case "AveFinish": typeCompare = one.AveFinish.CompareTo(two.AveFinish); break;
                        case "AveKudos": typeCompare = one.AveKudos.CompareTo(two.AveKudos); break;
                        default: typeCompare = nameCompare; break;
                    }
                }

                if (typeCompare == 0) {
                    typeCompare = nameCompare;
                }

                return typeCompare;
            });

            this.gridDetails.DataSource = null;
            this.gridDetails.DataSource = this.StatDetails;
            this.gridDetails.Columns[columnName].HeaderCell.SortGlyphDirection = sortOrder;
        }
        private void GridDetails_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e) {
            this.SortGridDetails(e.ColumnIndex, false);
        }
        private void GridDetails_SelectionChanged(object sender, EventArgs e) {
            if (this.gridDetails.SelectedCells.Count > 0) {
                this.gridDetails.ClearSelection();
            }
        }
        private void ToggleWinPercentageDisplay() {
            this.CurrentSettings.ShowPercentages = !this.CurrentSettings.ShowPercentages;
            this.SaveUserSettings();
            this.gridDetails.Invalidate();
        }
        private void ShowShows() {
            using (LevelDetails levelDetails = new LevelDetails()) {
                levelDetails.LevelName = "Shows";
                List<RoundInfo> rounds = new List<RoundInfo>();
                for (int i = 0; i < StatDetails.Count; i++) {
                    rounds.AddRange(StatDetails[i].Stats);
                }

                rounds.Sort();

                List<RoundInfo> shows = new List<RoundInfo>();
                int roundCount = 0;
                int kudosTotal = 0;
                bool won = false;
                bool isFinal = false;
                DateTime endDate = DateTime.MinValue;

                for (int i = rounds.Count - 1; i >= 0; i--) {
                    RoundInfo info = rounds[i];
                    if (roundCount == 0) {
                        endDate = info.End;
                        won = info.Qualified;
                        isFinal = info.IsFinal || info.Crown;
                    }

                    roundCount++;
                    kudosTotal += info.Kudos;
                    if (info.Round == 1) {
                        shows.Insert(0,
                            new RoundInfo {
                                Name = isFinal ? "Final" : string.Empty,
                                ShowNameId = info.ShowNameId,
                                IsFinal = isFinal,
                                End = endDate,
                                Start = info.Start,
                                StartLocal = info.StartLocal,
                                Kudos = kudosTotal,
                                Qualified = won,
                                AbandonShow = info.AbandonShow,
                                Round = roundCount,
                                ShowID = info.ShowID,
                                Tier = won ? 1 : 0,
                                PrivateLobby = info.PrivateLobby,
                                UseShareCode = info.UseShareCode,
                                CreativeAuthor = info.CreativeAuthor,
                                CreativeOnlinePlatformId = info.CreativeOnlinePlatformId,
                                CreativeShareCode = info.CreativeShareCode,
                                CreativeTitle = info.CreativeTitle,
                                CreativeDescription = info.CreativeDescription,
                                CreativeVersion = info.CreativeVersion,
                                CreativeMaxPlayer = info.CreativeMaxPlayer,
                                CreativePlatformId = info.CreativePlatformId,
                                CreativePlayCount = info.CreativePlayCount,
                                CreativeLastModifiedDate = info.CreativeLastModifiedDate
                            });
                        roundCount = 0;
                        kudosTotal = 0;
                    }
                }

                levelDetails.RoundDetails = shows;
                levelDetails.StatsForm = this;

                levelDetails.ShowDialog(this);
            }
        }
        private void ShowRounds() {
            using (LevelDetails levelDetails = new LevelDetails()) {
                levelDetails.LevelName = "Rounds";
                List<RoundInfo> rounds = new List<RoundInfo>();
                for (int i = 0; i < StatDetails.Count; i++) {
                    rounds.AddRange(StatDetails[i].Stats);
                }
                rounds.Sort();
                levelDetails.RoundDetails = rounds;
                levelDetails.StatsForm = this;

                levelDetails.ShowDialog(this);
            }
        }
        private void ShowFinals() {
            using (LevelDetails levelDetails = new LevelDetails()) {
                levelDetails.LevelName = "Finals";
                List<RoundInfo> rounds = new List<RoundInfo>();
                for (int i = 0; i < StatDetails.Count; i++) {
                    rounds.AddRange(StatDetails[i].Stats);
                }

                rounds.Sort();

                int keepShow = -1;
                for (int i = rounds.Count - 1; i >= 0; i--) {
                    RoundInfo info = rounds[i];
                    if (info.ShowID != keepShow && (info.Crown || info.IsFinal)) {
                        keepShow = info.ShowID;
                    } else if (info.ShowID != keepShow) {
                        rounds.RemoveAt(i);
                    }
                }

                levelDetails.RoundDetails = rounds;
                levelDetails.StatsForm = this;

                levelDetails.ShowDialog(this);
            }
        }
        private void ShowWinGraph() {
            List<RoundInfo> rounds = new List<RoundInfo>();
            for (int i = 0; i < StatDetails.Count; i++) {
                rounds.AddRange(StatDetails[i].Stats);
            }
            rounds.Sort();

            using (WinStatsDisplay display = new WinStatsDisplay {
                StatsForm = this,
                Text = $@"     {Multilingual.GetWord("level_detail_wins_per_day")} - {this.GetCurrentProfileName()} ({this.GetCurrentFilterName()})",
                BackImage = Properties.Resources.crown_icon,
                BackMaxSize = 32,
                BackImagePadding = new Padding(20, 20, 0, 0)
            }) {
                ArrayList dates = new ArrayList();
                ArrayList shows = new ArrayList();
                ArrayList finals = new ArrayList();
                ArrayList wins = new ArrayList();
                if (rounds.Count > 0) {
                    DateTime start = rounds[0].StartLocal;
                    int currentShows = 0;
                    int currentFinals = 0;
                    int currentWins = 0;
                    bool incrementedShows = false;
                    bool incrementedFinals = false;
                    bool incrementedWins = false;
                    foreach (RoundInfo info in rounds.Where(info => !info.PrivateLobby)) {
                        if (info.Round == 1) {
                            currentShows++;
                            incrementedShows = true;
                        }

                        if (info.Crown || info.IsFinal) {
                            currentFinals++;
                            incrementedFinals = true;
                            if (info.Qualified) {
                                currentWins++;
                                incrementedWins = true;
                            }
                        }

                        if (info.StartLocal.Date > start.Date && (incrementedShows || incrementedFinals)) {
                            dates.Add(start.Date.ToOADate());
                            shows.Add(Convert.ToDouble(incrementedShows ? --currentShows : currentShows));
                            finals.Add(Convert.ToDouble(incrementedFinals ? --currentFinals : currentFinals));
                            wins.Add(Convert.ToDouble(incrementedWins ? --currentWins : currentWins));

                            int daysWithoutStats = (int)(info.StartLocal.Date - start.Date).TotalDays - 1;
                            while (daysWithoutStats > 0) {
                                daysWithoutStats--;
                                start = start.Date.AddDays(1);
                                dates.Add(start.ToOADate());
                                shows.Add(0D);
                                finals.Add(0D);
                                wins.Add(0D);
                            }

                            currentShows = incrementedShows ? 1 : 0;
                            currentFinals = incrementedFinals ? 1 : 0;
                            currentWins = incrementedWins ? 1 : 0;
                            start = info.StartLocal;
                        }

                        incrementedShows = false;
                        incrementedFinals = false;
                        incrementedWins = false;
                    }

                    dates.Add(start.Date.ToOADate());
                    shows.Add(Convert.ToDouble(currentShows));
                    finals.Add(Convert.ToDouble(currentFinals));
                    wins.Add(Convert.ToDouble(currentWins));

                    display.manualSpacing = (int)Math.Ceiling(dates.Count / 28D);
                    display.dates = (double[])dates.ToArray(typeof(double));
                    display.shows = (double[])shows.ToArray(typeof(double));
                    display.finals = (double[])finals.ToArray(typeof(double));
                    display.wins = (double[])wins.ToArray(typeof(double));
                } else {
                    dates.Add(DateTime.Now.Date.ToOADate());
                    shows.Add(0D);
                    finals.Add(0D);
                    wins.Add(0D);

                    display.manualSpacing = 1;
                    display.dates = (double[])dates.ToArray(typeof(double));
                    display.shows = (double[])shows.ToArray(typeof(double));
                    display.finals = (double[])finals.ToArray(typeof(double));
                    display.wins = (double[])wins.ToArray(typeof(double));
                }

                display.ShowDialog(this);
            }
        }
        private void ShowRoundGraph() {
            List<RoundInfo> rounds = new List<RoundInfo>();
            if (this.menuCustomRangeStats.Checked) {
                rounds = this.AllStats.Where(roundInfo => {
                    return roundInfo.Start >= this.customfilterRangeStart &&
                           roundInfo.Start <= this.customfilterRangeEnd &&
                           roundInfo.Profile == this.GetCurrentProfileId() && this.IsInPartyFilter(roundInfo);
                }).OrderBy(r => r.Name).ToList();
            } else {
                DateTime compareDate = this.menuAllStats.Checked ? DateTime.MinValue :
                    this.menuSeasonStats.Checked ? SeasonStart :
                    this.menuWeekStats.Checked ? WeekStart :
                    this.menuDayStats.Checked ? DayStart : SessionStart;
                rounds = this.AllStats.Where(roundInfo => {
                    return roundInfo.Start > compareDate && roundInfo.Profile == this.GetCurrentProfileId() && this.IsInPartyFilter(roundInfo);
                }).OrderBy(r => r.Name).ToList();
            }
            if (rounds.Count == 0) { return; }

            using (RoundStatsDisplay roundStatsDisplay = new RoundStatsDisplay {
                StatsForm = this,
                Text = $@"     {Multilingual.GetWord("level_detail_stats_by_round")} - {this.GetCurrentProfileName()} ({this.GetCurrentFilterName()})",
                BackImage = this.Theme == MetroThemeStyle.Light ? Properties.Resources.round_icon : Properties.Resources.round_gray_icon,
                BackMaxSize = 32,
                BackImagePadding = new Padding(20, 20, 0, 0)
            }) {
                Dictionary<string, double[]> roundGraphData = new Dictionary<string, double[]>();
                Dictionary<string, TimeSpan> roundDurationData = new Dictionary<string, TimeSpan>();
                //Dictionary<string, double[]> roundRecordData = new Dictionary<string, double[]>();
                Dictionary<string, int[]> roundScoreData = new Dictionary<string, int[]>();
                Dictionary<string, string> roundList = new Dictionary<string, string>();
                double p = 0, gm = 0, sm = 0, bm = 0, pm = 0, em = 0;
                //TimeSpan ft = TimeSpan.Zero, lt = TimeSpan.Zero;
                int hs = 0;
                int ls = 0;
                TimeSpan d = TimeSpan.Zero;
                for (int i = 0; i < rounds.Count; i++) {
                    if (i > 0 && !rounds[i].Name.Equals(rounds[i - 1].Name)) {
                        roundDurationData.Add(rounds[i - 1].Name, d);
                        roundGraphData.Add(rounds[i - 1].Name, new[] { p, gm, sm, bm, pm, em });
                        //roundRecordData.Add(rounds[i - 1].Name, new[] { ft.TotalSeconds, lt.TotalSeconds });
                        roundScoreData.Add(rounds[i - 1].Name, new[] { hs, ls });
                        roundList.Add(rounds[i - 1].Name, Multilingual.GetRoundName(rounds[i - 1].Name).Replace("&", "&&"));
                        d = TimeSpan.Zero;
                        //ft = TimeSpan.Zero; lt = TimeSpan.Zero;
                        hs = 0; ls = 0;
                        p = 0; gm = 0; sm = 0; bm = 0; pm = 0; em = 0;
                    }
                    //ft = (rounds[i].End - rounds[i].Start) < ft ? (rounds[i].End - rounds[i].Start) : ft;
                    //lt = (rounds[i].End - rounds[i].Start) > lt ? (rounds[i].End - rounds[i].Start) : lt;
                    hs = (int)(rounds[i].Score > hs ? rounds[i].Score : hs);
                    ls = (int)(rounds[i].Score < ls ? rounds[i].Score : ls);

                    d += rounds[i].End - rounds[i].Start;
                    p++;
                    if (rounds[i].Qualified) {
                        switch (rounds[i].Tier) {
                            case (int)QualifyTier.Pink:
                                pm++; break;
                            case (int)QualifyTier.Gold:
                                gm++; break;
                            case (int)QualifyTier.Silver:
                                sm++; break;
                            case (int)QualifyTier.Bronze:
                                bm++; break;
                        }
                    } else {
                        em++;
                    }

                    if (i == rounds.Count - 1) {
                        roundDurationData.Add(rounds[i].Name, d);
                        roundGraphData.Add(rounds[i].Name, new[] { p, gm, sm, bm, pm, em });
                        //roundRecordData.Add(rounds[i].Name, new[] { ft.TotalSeconds, lt.TotalSeconds });
                        roundScoreData.Add(rounds[i].Name, new[] { hs, ls });
                        roundList.Add(rounds[i].Name, Multilingual.GetRoundName(rounds[i].Name));
                    }
                }

                roundStatsDisplay.roundList = from pair in roundList orderby pair.Value ascending select pair;
                roundStatsDisplay.roundDurationData = roundDurationData;
                //roundStatsDisplay.roundRecordData = roundRecordData;
                roundStatsDisplay.roundScoreData = roundScoreData;
                roundStatsDisplay.roundGraphData = roundGraphData;

                roundStatsDisplay.ShowDialog(this);
            }
        }
        private void LaunchHelpInBrowser() {
            try {
                Process.Start(@"https://github.com/Micdu70/FallGuysStats#sommaire");
            } catch (Exception ex) {
                MessageBox.Show(this, ex.ToString(), $"{Multilingual.GetWord("message_program_error_caption")}",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void LaunchGame(bool ignoreExisting) {
            try {
                //this.UpdateGameExeLocation();
                if (this.CurrentSettings.LaunchPlatform == 0) {
                    if (!string.IsNullOrEmpty(this.CurrentSettings.GameShortcutLocation)) {
                        Process[] processes = Process.GetProcesses();
                        string fallGuysProcessName = "FallGuys_client_game";
                        for (int i = 0; i < processes.Length; i++) {
                            string name = processes[i].ProcessName;
                            if (name.IndexOf(fallGuysProcessName, StringComparison.OrdinalIgnoreCase) >= 0) {
                                if (!ignoreExisting) {
                                    MessageBox.Show(this, Multilingual.GetWord("message_fall_guys_already_running"), Multilingual.GetWord("message_already_running_caption"),
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                                return;
                            }
                        }

                        if (MessageBox.Show(this, $"{Multilingual.GetWord("message_execution_question")}",
                                $"[{Multilingual.GetWord("level_detail_online_platform_eos")}] {Multilingual.GetWord("message_execution_caption")}",
                                MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK) {
                            Process.Start(this.CurrentSettings.GameShortcutLocation);
                            if (!ignoreExisting) {
                                this.WindowState = FormWindowState.Minimized;
                            } else {
                                this.minimizeAfterGameLaunch = true;
                            }
                        }
                    } else {
                        MessageBox.Show(this, Multilingual.GetWord("message_register_shortcut"), Multilingual.GetWord("message_register_shortcut_caption"),
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                } else {
                    if (!string.IsNullOrEmpty(this.CurrentSettings.GameExeLocation) && File.Exists(this.CurrentSettings.GameExeLocation)) {
                        Process[] processes = Process.GetProcesses();
                        string fallGuysProcessName = Path.GetFileNameWithoutExtension(this.CurrentSettings.GameExeLocation);
                        for (int i = 0; i < processes.Length; i++) {
                            string name = processes[i].ProcessName;
                            if (name.IndexOf(fallGuysProcessName, StringComparison.OrdinalIgnoreCase) >= 0) {
                                if (!ignoreExisting) {
                                    MessageBox.Show(this, Multilingual.GetWord("message_fall_guys_already_running"), Multilingual.GetWord("message_already_running_caption"),
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                                return;
                            }
                        }

                        if (MessageBox.Show(this, $"{Multilingual.GetWord("message_execution_question")}",
                                $"[{Multilingual.GetWord("level_detail_online_platform_steam")}] {Multilingual.GetWord("message_execution_caption")}",
                                MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK) {
                            Process.Start(this.CurrentSettings.GameExeLocation);
                            if (!ignoreExisting) {
                                this.WindowState = FormWindowState.Minimized;
                            } else {
                                this.minimizeAfterGameLaunch = true;
                            }
                        }
                    } else {
                        MessageBox.Show(this, Multilingual.GetWord("message_register_exe"), Multilingual.GetWord("message_register_exe_caption"),
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            } catch (Exception ex) {
                MessageBox.Show(this, ex.ToString(), $"{Multilingual.GetWord("message_program_error_caption")}",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void UpdateGameExeLocation() {
            string fallGuysShortcutLocation = this.FindEpicGamesShortcutLocation();
            string fallGuysExeLocation = this.FindSteamExeLocation();

            if (string.IsNullOrEmpty(fallGuysShortcutLocation) && !string.IsNullOrEmpty(fallGuysExeLocation)) {
                this.menuLaunchFallGuys.Image = Properties.Resources.steam_main_icon;
                this.CurrentSettings.LaunchPlatform = 1;
            } else if (!string.IsNullOrEmpty(fallGuysShortcutLocation) && !string.IsNullOrEmpty(fallGuysExeLocation)) {
                this.menuLaunchFallGuys.Image = this.CurrentSettings.LaunchPlatform == 0 ? Properties.Resources.epic_main_icon : Properties.Resources.steam_main_icon;
            } else {
                this.menuLaunchFallGuys.Image = Properties.Resources.epic_main_icon;
                this.CurrentSettings.LaunchPlatform = 0;
            }

            this.CurrentSettings.GameShortcutLocation = fallGuysShortcutLocation;
            this.CurrentSettings.GameExeLocation = fallGuysExeLocation;
        }
        public string FindEpicGamesShortcutLocation() {
            try {
                object regValue = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\Epic Games\\EpicGamesLauncher", "AppDataPath", null);
                if (regValue == null) {
                    return string.Empty;
                }
                string epicGamesPath = Path.Combine((string)regValue, "Manifests");

                if (Directory.Exists(epicGamesPath)) {
                    DirectoryInfo di = new DirectoryInfo(epicGamesPath);
                    foreach (FileInfo file in di.GetFiles()) {
                        if (!".item".Equals(file.Extension)) continue;
                        JsonClass json = Json.Read(File.ReadAllText(file.FullName)) as JsonClass;
                        string displayName = json["DisplayName"].AsString();
                        if ("Fall Guys".Equals(displayName)) {
                            return "com.epicgames.launcher://apps/50118b7f954e450f8823df1614b24e80%3A38ec4849ea4f4de6aa7b6fb0f2d278e1%3A0a2d9f6403244d12969e11da6713137b?action=launch&silent=true";
                        }
                    }
                }
            } catch (Exception ex) {
                MessageBox.Show(this, ex.ToString(), $"{Multilingual.GetWord("message_program_error_caption")}",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return string.Empty;
        }
        public string FindSteamExeLocation() {
            try {
                // get steam install folder
                object regValue = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\Valve\\Steam", "InstallPath", null);
                if (regValue == null) {
                    return string.Empty;
                }
                string steamPath = (string)regValue;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                    string userName = Environment.UserName;
                    steamPath = Path.Combine("/", "home", userName, ".local", "share", "Steam");
                }

                string fallGuysSteamPath = Path.Combine(steamPath, "steamapps", "common", "Fall Guys", "FallGuys_client_game.exe");

                if (File.Exists(fallGuysSteamPath)) { return fallGuysSteamPath; }
                // read libraryfolders.vdf from install folder to get games installation folder
                // note: this parsing is terrible, but does technically work fine. There's a better way by specifying a schema and
                // fully parsing the file or something like that. This is quick and dirty, for sure.
                FileInfo libraryFoldersFile = new FileInfo(Path.Combine(steamPath, "steamapps", "libraryfolders.vdf"));
                if (libraryFoldersFile.Exists) {
                    JsonClass json = Json.Read(File.ReadAllText(libraryFoldersFile.FullName)) as JsonClass;
                    foreach (JsonObject obj in json) {
                        if (obj is JsonClass library) {
                            string libraryPath = library["path"].AsString();
                            if (!string.IsNullOrEmpty(libraryPath)) {
                                // look for exe in standard location under library
                                fallGuysSteamPath = Path.Combine(libraryPath, "steamapps", "common", "Fall Guys", "FallGuys_client_game.exe");

                                if (File.Exists(fallGuysSteamPath)) { return fallGuysSteamPath; }
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                MessageBox.Show(this, ex.ToString(), $"{Multilingual.GetWord("message_program_error_caption")}",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return string.Empty;
        }
        private void EnableMainMenu(bool enable) {
            this.menuSettings.Enabled = enable;
            this.menuFilters.Enabled = enable;
            this.menuProfile.Enabled = enable;
            if (enable) {
                this.menuSettings.ForeColor = this.Theme == MetroThemeStyle.Light ? Color.Black : Color.DarkGray;
                this.menuSettings.Image = this.Theme == MetroThemeStyle.Light ? Properties.Resources.setting_icon : Properties.Resources.setting_gray_icon;
            }
        }
        private void EnableInfoStrip(bool enable) {
            this.infoStrip.Enabled = enable;
            this.infoStrip2.Enabled = enable;
            this.lblTotalTime.Enabled = enable;
            if (enable) this.lblTotalTime.ForeColor = this.Theme == MetroThemeStyle.Light ? Color.Blue : Color.Orange;
            foreach (var tsi in this.infoStrip.Items) {
                if (tsi is ToolStripLabel tsl) {
                    tsl.Enabled = enable;
                    if (enable) {
                        this.Cursor = Cursors.Default;
                        tsl.ForeColor = tsl.Name.Equals("lblCurrentProfile")
                                        ? this.Theme == MetroThemeStyle.Light ? Color.Red : Color.FromArgb(0, 192, 192)
                                        : this.Theme == MetroThemeStyle.Light ? Color.Blue : Color.Orange;
                    }
                }
            }
        }
        private void Stats_KeyUp(object sender, KeyEventArgs e) {
            switch (e.KeyCode) {
                case Keys.ShiftKey:
                    this.shiftKeyToggle = false;
                    break;
                case Keys.ControlKey:
                    // this.ctrlKeyToggle = false;
                    break;
            }
        }
        private void Stats_KeyDown(object sender, KeyEventArgs e) {
            switch (e.KeyCode) {
                case Keys.ShiftKey:
                    this.shiftKeyToggle = true;
                    break;
                case Keys.ControlKey:
                    // this.ctrlKeyToggle = true;
                    break;
            }
        }
        private void LblCurrentProfile_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                for (int i = 0; i < this.ProfileMenuItems.Count; i++) {
                    if (!(this.ProfileMenuItems[i] is ToolStripMenuItem menuItem)) { continue; }
                    if (this.shiftKeyToggle) {
                        if (menuItem.Checked && i - 1 >= 0) {
                            this.MenuStats_Click(this.ProfileMenuItems[i - 1], EventArgs.Empty);
                            break;
                        }
                        if (menuItem.Checked && i - 1 < 0) {
                            this.MenuStats_Click(this.ProfileMenuItems[this.ProfileMenuItems.Count - 1], EventArgs.Empty);
                            break;
                        }
                    } else {
                        if (menuItem.Checked && i + 1 < this.ProfileMenuItems.Count) {
                            this.MenuStats_Click(this.ProfileMenuItems[i + 1], EventArgs.Empty);
                            break;
                        }
                        if (menuItem.Checked && i + 1 >= this.ProfileMenuItems.Count) {
                            this.MenuStats_Click(this.ProfileMenuItems[0], EventArgs.Empty);
                            break;
                        }
                    }
                }
            } else if (e.Button == MouseButtons.Right) {
                for (int i = 0; i < this.ProfileMenuItems.Count; i++) {
                    if (!(this.ProfileMenuItems[i] is ToolStripMenuItem menuItem)) { continue; }
                    if (menuItem.Checked && i - 1 >= 0) {
                        this.MenuStats_Click(this.ProfileMenuItems[i - 1], EventArgs.Empty);
                        break;
                    }
                    if (menuItem.Checked && i - 1 < 0) {
                        this.MenuStats_Click(this.ProfileMenuItems[this.ProfileMenuItems.Count - 1], EventArgs.Empty);
                        break;
                    }
                }
            }
        }
        private void LblTotalTime_Click(object sender, EventArgs e) {
            try {
                this.EnableInfoStrip(false);
                this.EnableMainMenu(false);
                this.ShowRoundGraph();
                this.EnableInfoStrip(true);
                this.EnableMainMenu(true);
            } catch (Exception ex) {
                this.EnableInfoStrip(true);
                this.EnableMainMenu(true);
                MessageBox.Show(this, ex.ToString(), $"{Multilingual.GetWord("message_program_error_caption")}",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void LblTotalFinals_Click(object sender, EventArgs e) {
            try {
                this.EnableInfoStrip(false);
                this.EnableMainMenu(false);
                this.ShowFinals();
                this.EnableInfoStrip(true);
                this.EnableMainMenu(true);
            } catch (Exception ex) {
                this.EnableInfoStrip(true);
                this.EnableMainMenu(true);
                MessageBox.Show(this, ex.ToString(), $"{Multilingual.GetWord("message_program_error_caption")}",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void LblTotalShows_Click(object sender, EventArgs e) {
            try {
                this.EnableInfoStrip(false);
                this.EnableMainMenu(false);
                this.ShowShows();
                this.EnableInfoStrip(true);
                this.EnableMainMenu(true);
            } catch (Exception ex) {
                this.EnableInfoStrip(true);
                this.EnableMainMenu(true);
                MessageBox.Show(this, ex.ToString(), $"{Multilingual.GetWord("message_program_error_caption")}",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void LblTotalRounds_Click(object sender, EventArgs e) {
            try {
                this.EnableInfoStrip(false);
                this.EnableMainMenu(false);
                this.ShowRounds();
                this.EnableInfoStrip(true);
                this.EnableMainMenu(true);
            } catch (Exception ex) {
                this.EnableInfoStrip(true);
                this.EnableMainMenu(true);
                MessageBox.Show(this, ex.ToString(), $"{Multilingual.GetWord("message_program_error_caption")}",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void LblTotalWins_Click(object sender, EventArgs e) {
            try {
                this.EnableInfoStrip(false);
                this.EnableMainMenu(false);
                this.ShowWinGraph();
                this.EnableInfoStrip(true);
                this.EnableMainMenu(true);
            } catch (Exception ex) {
                this.EnableInfoStrip(true);
                this.EnableMainMenu(true);
                MessageBox.Show(this, ex.ToString(), $"{Multilingual.GetWord("message_program_error_caption")}",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void MenuStats_Click(object sender, EventArgs e) {
            try {
                ToolStripMenuItem button = sender as ToolStripMenuItem;
                if (button == this.menuCustomRangeStats) {
                    if (this.isStartingUp && !this.onlyRefreshFilter) {
                        this.updateFilterRange = true;
                    } else if (!this.onlyRefreshFilter) {
                        using (FilterCustomRange filterCustomRange = new FilterCustomRange()) {
                            //filterCustomRange.Icon = this.Icon;
                            filterCustomRange.StatsForm = this;
                            filterCustomRange.startDate = this.customfilterRangeStart;
                            filterCustomRange.endDate = this.customfilterRangeEnd;
                            filterCustomRange.selectedCustomTemplateSeason = this.selectedCustomTemplateSeason;
                            this.EnableInfoStrip(false);
                            this.EnableMainMenu(false);
                            if (filterCustomRange.ShowDialog(this) == DialogResult.OK) {
                                this.menuCustomRangeStats.Checked = true;
                                this.menuAllStats.Checked = false;
                                this.menuSeasonStats.Checked = false;
                                this.menuWeekStats.Checked = false;
                                this.menuDayStats.Checked = false;
                                this.menuSessionStats.Checked = false;
                                this.selectedCustomTemplateSeason = filterCustomRange.selectedCustomTemplateSeason;
                                this.customfilterRangeStart = filterCustomRange.startDate;
                                this.customfilterRangeEnd = filterCustomRange.endDate;
                                this.updateFilterRange = true;
                            } else {
                                this.EnableInfoStrip(true);
                                this.EnableMainMenu(true);
                                return;
                            }
                            this.EnableInfoStrip(true);
                            this.EnableMainMenu(true);
                        }
                    }
                } else if (button == this.menuAllStats || button == this.menuSeasonStats || button == this.menuWeekStats || button == this.menuDayStats || button == this.menuSessionStats) {
                    if (!this.menuAllStats.Checked && !this.menuSeasonStats.Checked && !this.menuWeekStats.Checked && !this.menuDayStats.Checked && !this.menuSessionStats.Checked) {
                        button.Checked = true;
                        return;
                    }

                    if (!this.onlyRefreshFilter) {
                        this.updateFilterType = true;
                        this.updateFilterRange = false;
                    }

                    foreach (ToolStripItem item in this.menuStatsFilter.DropDownItems) {
                        if (item is ToolStripMenuItem menuItem && menuItem.Checked && menuItem != button) {
                            menuItem.Checked = false;
                        }
                    }
                } else if (button == this.menuAllPartyStats || button == this.menuSoloStats || button == this.menuPartyStats) {
                    if (!this.menuAllPartyStats.Checked && !this.menuSoloStats.Checked && !this.menuPartyStats.Checked) {
                        button.Checked = true;
                        return;
                    }

                    foreach (ToolStripItem item in this.menuPartyFilter.DropDownItems) {
                        if (item is ToolStripMenuItem menuItem && menuItem.Checked && menuItem != button) {
                            menuItem.Checked = false;
                        }
                    }
                } else if (this.ProfileMenuItems.Contains(button)) {
                    for (int i = this.ProfileMenuItems.Count - 1; i >= 0; i--) {
                        if (this.ProfileMenuItems[i].Name == button.Name) {
                            this.SetCurrentProfileIcon(this.AllProfiles.FindIndex(p => {
                                return p.ProfileName == this.ProfileMenuItems[i].Text && !string.IsNullOrEmpty(p.LinkedShowId);
                            }) != -1);
                        }
                        this.ProfileMenuItems[i].Checked = this.ProfileMenuItems[i].Name == button.Name;
                    }

                    this.currentProfile = this.GetProfileIdFromName(button.Text);
                    this.updateSelectedProfile = true;
                }

                for (int i = 0; i < this.StatDetails.Count; i++) {
                    LevelStats calculator = this.StatDetails[i];
                    calculator.Clear();
                }

                this.ClearTotals();

                int profile = this.currentProfile;

                List<RoundInfo> rounds;
                if (this.menuCustomRangeStats.Checked) {
                    rounds = this.AllStats.Where(roundInfo => {
                        return roundInfo.Start >= this.customfilterRangeStart &&
                               roundInfo.Start <= this.customfilterRangeEnd &&
                               roundInfo.Profile == profile && this.IsInPartyFilter(roundInfo);
                    }).ToList();
                } else {
                    DateTime compareDate = this.menuAllStats.Checked ? DateTime.MinValue :
                                            this.menuSeasonStats.Checked ? SeasonStart :
                                            this.menuWeekStats.Checked ? WeekStart :
                                            this.menuDayStats.Checked ? DayStart : SessionStart;
                    rounds = this.AllStats.Where(roundInfo => {
                        return roundInfo.Start > compareDate && roundInfo.Profile == profile && this.IsInPartyFilter(roundInfo);
                    }).ToList();
                }

                rounds.Sort();

                if (!this.isStartingUp && this.updateFilterType) {
                    this.updateFilterType = false;
                    this.CurrentSettings.FilterType = this.menuSeasonStats.Checked ? 2 :
                                                        this.menuWeekStats.Checked ? 3 :
                                                        this.menuDayStats.Checked ? 4 :
                                                        this.menuSessionStats.Checked ? 5 : 1;
                    this.SaveUserSettings();
                } else if (!this.isStartingUp && this.updateFilterRange) {
                    this.updateFilterRange = false;
                    this.CurrentSettings.FilterType = 0;
                    this.CurrentSettings.SelectedCustomTemplateSeason = this.selectedCustomTemplateSeason;
                    this.CurrentSettings.CustomFilterRangeStart = this.customfilterRangeStart;
                    this.CurrentSettings.CustomFilterRangeEnd = this.customfilterRangeEnd;
                    this.SaveUserSettings();
                } else if (!this.isStartingUp && this.updateSelectedProfile) {
                    this.updateSelectedProfile = false;
                    this.CurrentSettings.SelectedProfile = profile;
                    this.SaveUserSettings();
                }

                this.loadingExisting = true;
                this.LogFile_OnParsedLogLines(rounds);
                this.loadingExisting = false;
            } catch (Exception ex) {
                MessageBox.Show(this, ex.ToString(), $"{Multilingual.GetWord("message_program_error_caption")}",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void MenuUpdate_Click(object sender, EventArgs e) {
#if AllowUpdate
            try {
                this.CheckForUpdate(false);
            } catch {
                MessageBox.Show(this, $"{Multilingual.GetWord("message_update_error")}", $"{Multilingual.GetWord("message_update_error_caption")}",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
#else
            try {
                // Process.Start(@"https://github.com/Micdu70/FallGuysStats#t%C3%A9l%C3%A9chargement");
                Process.Start(@"https://github.com/ShootMe/FallGuysStats/#readme");
            } catch (Exception ex) {
                MessageBox.Show(this, ex.ToString(), $"{Multilingual.GetWord("message_program_error_caption")}",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
#endif
        }
        public bool IsCreativeShow(string showId) {
            return showId.StartsWith("show_wle_s10_") ||
                   showId.StartsWith("event_wle_s10_") ||
                   showId.IndexOf("wle_s10_player_round_", StringComparison.OrdinalIgnoreCase) != -1 ||
                   showId.Equals("wle_mrs_bagel") ||
                   showId.Equals("wle_mrs_shuffle_show") ||
                   showId.Equals("wle_shuffle_discover") ||
                   showId.StartsWith("current_wle_fp") ||
                   showId.StartsWith("wle_s10_cf_round_");
        }
        public string GetRoundIdFromShareCode(string shareCode) {
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
                // case "6748-6192-9739": return "wle_s10_orig_round_045";
                case "9641-5398-7416": return "wle_s10_orig_round_046";
                case "7895-4812-3429": return "wle_s10_orig_round_047";
                case "1468-0990-4257": return "wle_s10_orig_round_048";
                case "6748-6192-9739": return "wle_s10_orig_round_045_long";
                case "3145-1396-6644": return "wle_s10_long_round_003";
                case "9194-9593-3605": return "wle_s10_long_round_004";
                case "1226-0563-3570": return "wle_s10_long_round_005";
                // 43

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
                // 53

                case "0024-8840-0043": return "wle_s10_player_round_wk3_01";
                case "0700-1924-4824": return "wle_s10_player_round_wk3_02";
                case "1600-7003-9946": return "wle_s10_player_round_wk3_03";
                case "1689-7568-3407": return "wle_s10_player_round_wk3_04";
                case "1998-0819-8328": return "wle_s10_player_round_wk3_06";
                case "2087-8833-2659": return "wle_s10_player_round_wk3_07";
                case "2489-4644-1812": return "wle_s10_player_round_wk3_08";
                case "3338-3252-7707": return "wle_s10_player_round_wk3_09";
                case "4030-1420-2157": return "wle_s10_player_round_wk3_10";
                case "4928-7457-4026": return "wle_s10_player_round_wk3_11";
                case "5021-4188-7293": return "wle_s10_player_round_wk3_12";
                case "5320-7960-5930": return "wle_fp2_wk6_01";
                case "6771-8467-2061": return "wle_s10_player_round_wk3_14";
                case "8700-2575-3311": return "wle_s10_player_round_wk3_15";
                case "8766-7319-7097": return "wle_s10_player_round_wk3_16";
                case "8872-5222-3800": return "wle_s10_player_round_wk3_17";
                case "9694-1257-5407": return "wle_s10_player_round_wk3_18";
                case "2437-7699-1837": return "wle_s10_player_round_wk3_19";
                case "5151-1811-0629": return "wle_s10_player_round_wk3_20";
                // 72

                case "1031-4769-0669": return "wle_s10_player_round_wk4_01";
                case "2378-2762-3285": return "wle_s10_player_round_wk4_02";
                case "4951-7804-0209": return "wle_s10_player_round_wk4_03";
                case "5033-3246-9786": return "wle_s10_player_round_wk4_05";
                case "6368-6082-3506": return "wle_s10_player_round_wk4_06";
                case "6372-5154-6625": return "wle_s10_player_round_wk4_07";
                case "6522-0063-8509": return "wle_s10_player_round_wk4_08";
                case "7660-3559-9068": return "wle_s10_player_round_wk4_09";
                case "7671-4178-2989": return "wle_s10_player_round_wk4_10";
                case "8403-4697-3155": return "wle_s10_player_round_wk4_11";
                case "8453-0623-7193": return "wle_s10_player_round_wk4_12";
                case "8453-2843-0803": return "wle_s10_player_round_wk4_13";
                case "0630-0991-7090": return "wle_s10_player_round_wk4_15";
                case "5186-5622-9808": return "wle_s10_player_round_wk4_18";
                case "6444-4459-5271": return "wle_s10_player_round_wk4_19";
                case "9541-0497-1157": return "wle_s10_player_round_wk4_20";
                case "5022-2434-4735": return "wle_s10_player_round_wk4_21";
                // case "0653-2576-5236": return "wle_s10_player_round_wk4_22";
                // 89

                case "0379-8536-2130": return "wle_s10_player_round_wk5_01";
                case "4492-3335-5236": return "wle_s10_player_round_wk5_02";
                case "4962-7093-2509": return "wle_s10_player_round_wk5_03";
                case "5030-9762-5784": return "wle_s10_player_round_wk5_04";
                case "7013-0309-9834": return "wle_s10_player_round_wk5_05";
                case "9107-2619-3629": return "wle_s10_player_round_wk5_06";
                case "9630-3128-3650": return "wle_s10_player_round_wk5_07";
                case "0016-0725-5194": return "wle_s10_player_round_wk5_08";
                case "5151-2766-7273": return "wle_s10_player_round_wk5_10";
                case "5152-0450-7240": return "wle_s10_player_round_wk5_11";
                case "5683-5861-3450": return "wle_s10_player_round_wk5_12";
                case "6068-9732-2265": return "wle_s10_player_round_wk5_13";
                case "7079-6897-9253": return "wle_s10_player_round_wk5_14";
                case "7176-9635-8973": return "wle_s10_player_round_wk5_15";
                case "7385-4032-4473": return "wle_s10_player_round_wk5_16";
                case "8696-0154-7735": return "wle_s10_player_round_wk5_17";
                case "1514-8743-1209": return "wle_s10_player_round_wk5_18";
                // 106

                case "0029-7642-8120": return "wle_s10_player_round_wk6_01";
                case "0369-9367-0762": return "wle_s10_player_round_wk6_02";
                case "0677-7276-6505": return "wle_s10_player_round_wk6_03";
                case "1299-3796-8185": return "wle_s10_player_round_wk6_04";
                case "2205-4708-9826": return "wle_s10_player_round_wk6_05";
                case "2535-8666-2604": return "wle_s10_player_round_wk6_06";
                case "3009-9034-1901": return "wle_s10_player_round_wk6_08";
                case "3256-6219-9927": return "wle_s10_player_round_wk6_09";
                case "4815-5486-6368": return "wle_s10_player_round_wk6_10";
                // case "6594-9005-1010": return "wle_s10_player_round_wk6_12";
                case "1467-7300-1979": return "wle_s10_player_round_wk6_13";
                case "2734-5123-1618": return "wle_s10_player_round_wk6_14";
                case "2998-8626-6987": return "wle_s10_player_round_wk6_15";
                case "7202-9028-9879": return "wle_s10_player_round_wk6_17";
                case "7768-6392-9185": return "wle_s10_player_round_wk6_18";
                case "8641-8055-4328": return "wle_s10_player_round_wk6_19";
                case "9573-2809-5705": return "wle_s10_player_round_wk6_20";
                // 122

                case "8870-4742-9307": return "current_wle_fp3_07_01";
                case "8808-3436-4387": return "current_wle_fp3_07_02";
                case "6362-1646-7291": return "current_wle_fp3_07_03";
                case "5440-8703-2155": return "current_wle_fp3_07_04";
                case "3748-0695-7721": return "current_wle_fp3_07_05";
                case "9499-5510-3164": return "current_wle_fp3_07_0_01";
                case "9388-0404-7294": return "current_wle_fp3_07_0_02";
                case "8664-7885-4449": return "current_wle_fp3_07_0_03";
                // 130

                case "9381-5029-9669": return "current_wle_fp3_08_01";
                case "7363-9687-8489": return "current_wle_fp3_08_02";
                case "9586-9081-9196": return "current_wle_fp3_08_03";
                case "0612-0621-9941": return "current_wle_fp3_08_04";
                case "6482-3882-6886": return "current_wle_fp3_08_05";
                case "7709-5968-3616": return "current_wle_fp3_08_06";
                case "7235-2067-5458": return "current_wle_fp3_08_09";
                case "4297-5620-3088": return "current_wle_fp3_08_10";
                case "0359-4156-7328": return "current_wle_fp3_08_13";
                case "3405-3399-9164": return "current_wle_fp3_08_14";
                case "3477-1619-9087": return "current_wle_fp3_08_15";
                case "8365-4077-8899": return "current_wle_fp3_08_16";
                case "6132-6752-3094": return "current_wle_fp3_08_17";
                case "0248-4849-5920": return "current_wle_fp3_08_18";
                case "9773-2054-9828": return "current_wle_fp3_08_19";
                // 145

                case "2156-3451-2947": return "current_wle_fp3_09_01";
                case "2100-4019-7383": return "current_wle_fp3_09_02";
                case "2036-5667-2913": return "current_wle_fp3_09_03";
                case "2027-4003-0769": return "current_wle_fp3_09_04";
                case "1973-5335-7173": return "current_wle_fp3_09_05";
                case "1831-0720-1570": return "current_wle_fp3_09_06";
                case "1732-5441-1486": return "current_wle_fp3_09_07";
                case "1643-1632-1937": return "current_wle_fp3_09_08";
                case "1574-5745-8580": return "current_wle_fp3_09_09";
                case "7686-0651-7262": return "current_wle_fp3_09_0_01";
                case "6971-2125-2835": return "current_wle_fp3_09_0_02";
                case "2960-9271-4092": return "current_wle_fp3_09_0_03";
                case "2553-0095-7180": return "current_wle_fp3_09_0_04";
                case "2348-8341-9475": return "current_wle_fp3_09_0_05";
                case "1442-7149-6884": return "current_wle_fp3_09_0_06";
                case "0841-8832-8969": return "current_wle_fp3_09_0_0_01";
                // 161

                case "8126-9165-2616": return "current_wle_fp3_10_01";
                case "7929-7987-1262": return "current_wle_fp3_10_02";
                case "9268-5264-7225": return "current_wle_fp3_10_03";
                case "9914-5338-1978": return "current_wle_fp3_10_04";
                case "7630-6136-4887": return "current_wle_fp3_10_05";
                case "7510-0097-4635": return "current_wle_fp3_10_06";
                case "6472-0781-0296": return "current_wle_fp3_10_07";
                case "6255-9803-7844": return "current_wle_fp3_10_08";
                case "5483-7429-8172": return "current_wle_fp3_10_09";
                case "4670-0464-0152": return "current_wle_fp3_10_10";
                case "4045-0177-4638": return "current_wle_fp3_10_11";
                case "0632-7452-5630": return "current_wle_fp3_10_12";
                case "3501-1066-4405": return "current_wle_fp3_10_13";
                case "2532-0388-3114": return "current_wle_fp3_10_14";
                case "1685-0828-8429": return "current_wle_fp3_10_15";
                case "1478-3445-2708": return "current_wle_fp3_10_16";
                case "1374-0626-0437": return "current_wle_fp3_10_17";
                case "1369-3476-7041": return "current_wle_fp3_10_18";
                case "1265-8247-0490": return "current_wle_fp3_10_19";
                case "1247-0981-9943": return "current_wle_fp3_10_20";
                case "1236-0266-0278": return "current_wle_fp3_10_21";
                case "1046-1529-5825": return "current_wle_fp3_10_22";
                case "0953-7809-6137": return "current_wle_fp3_10_23";
                case "0832-8811-0923": return "current_wle_fp3_10_24";
                case "0523-0589-6057": return "current_wle_fp3_10_25";
                case "0312-1221-9928": return "current_wle_fp3_10_26";
                case "0209-4801-4109": return "current_wle_fp3_10_27";
                case "0147-5332-8225": return "current_wle_fp3_10_28";
                case "0026-7621-3553": return "current_wle_fp3_10_29";
                // 190

                case "4044-7301-7052": return "current_wle_fp4_05_01_01";
                case "4164-0494-9264": return "current_wle_fp4_05_01_02";
                case "4166-2880-4137": return "current_wle_fp4_05_01_03";
                case "4222-3581-1925": return "current_wle_fp4_05_01_04";
                // case "4398-7555-7009": return "current_wle_fp4_05_01_05";
                case "0510-4142-2680": return "current_wle_fp4_05_01";
                case "0665-1582-4029": return "current_wle_fp4_05_02";
                case "9117-2376-3176": return "current_wle_fp4_05_03_01";
                case "0474-0978-5834": return "current_wle_fp4_05_03_02";
                case "1736-4813-5491": return "current_wle_fp4_05_03";
                case "1862-2511-5334": return "current_wle_fp4_05_04";
                case "2412-2785-7810": return "current_wle_fp4_05_05";
                case "6595-2972-5515": return "current_wle_fp4_05_2_01";
                case "7696-3433-8713": return "current_wle_fp4_05_2_02";
                case "9212-2213-6190": return "current_wle_fp4_05_2_03";
                // 203

                case "7421-3397-8915": return "current_wle_fp4_06_01";
                case "0897-2632-3022": return "current_wle_fp4_06_02";
                case "4837-5178-6772": return "current_wle_fp4_06_0_01";
                case "9883-1664-6105": return "current_wle_fp4_06_0_02";
                case "1587-8887-3763": return "current_wle_fp4_06_0_03";
                case "0581-9229-6109": return "current_wle_fp4_06_0_04";
                case "7710-6656-7617": return "current_wle_fp4_06_0_05";
                case "6594-9005-1010": return "current_wle_fp4_06_0_10_01";
                case "8549-0392-8062": return "current_wle_fp4_06_1_01";
                case "8929-4807-4981": return "current_wle_fp4_06_1_02";
                case "8598-1284-1406": return "current_wle_fp4_06_1_03";
                case "0226-5184-6884": return "current_wle_fp4_06_1_04";
                case "9410-7962-1207": return "current_wle_fp4_06_1_05";
                case "6002-7812-7859": return "current_wle_fp4_06_1_06";
                // 218

                case "9414-6564-4192": return "current_wle_fp4_07_01";
                case "7909-3465-4598": return "current_wle_fp4_07_02";
                case "7342-4432-1991": return "current_wle_fp4_07_03";
                case "6385-6125-4124": return "current_wle_fp4_07_04";
                case "1831-8990-4701": return "current_wle_fp4_07_05";
                case "0513-9569-5079": return "current_wle_fp4_07_06";
                case "9825-0849-5439": return "current_wle_fp4_07_07";
                case "9963-3475-5634": return "current_wle_fp4_07_0_01";
                // 226

                case "9795-5277-9654": return "current_wle_fp4_08_01";
                case "7200-6187-0157": return "current_wle_fp4_08_0_01";
                case "7302-1231-7183": return "current_wle_fp4_08_0_02";
                case "6960-3324-8416": return "current_wle_fp4_08_0_03";
                case "6834-1823-0867": return "current_wle_fp4_08_0_04";
                case "6535-8427-4425": return "current_wle_fp4_08_0_05";
                case "6032-0510-0146": return "current_wle_fp4_08_0_06";
                case "5466-6380-0679": return "current_wle_fp4_08_0_07";
                case "1875-3024-7900": return "current_wle_fp4_08_1_01";
                case "3012-7317-9706": return "current_wle_fp4_08_1_02";
                case "3514-5844-7262": return "current_wle_fp4_08_1_03";
                case "4045-6389-6161": return "current_wle_fp4_08_1_04";
                case "0873-3500-8955": return "current_wle_fp4_08_2_01";
                case "1364-9647-0717": return "current_wle_fp4_08_3_01";
                // 240

                case "9687-4558-9242": return "current_wle_fp4_09_01";
                case "8824-3354-1460": return "current_wle_fp4_09_02";
                case "2646-4033-5577": return "current_wle_fp4_09_03";
                case "8671-3513-6156": return "current_wle_fp4_09_04";
                case "5440-2255-9842": return "current_wle_fp4_09_05";
                case "1738-4626-1757": return "current_wle_fp4_09_06";
                case "0796-0036-7637": return "current_wle_fp4_09_0_01";
                case "0090-3881-4137": return "current_wle_fp4_09_1_01";
                case "0996-7164-5913": return "current_wle_fp4_09_1_02";
                case "3953-9880-4802": return "current_wle_fp4_09_2_01";
                // 250

                case "0994-7335-6422": return "current_wle_fp4_10_01";
                case "1943-4681-1638": return "current_wle_fp4_10_02";
                case "6362-2965-5352": return "current_wle_fp4_10_03";
                case "0135-0293-3689": return "current_wle_fp4_10_04";
                case "9803-3074-8419": return "current_wle_fp4_10_05";
                case "1503-2277-4955": return "current_wle_fp4_10_06";
                case "6929-6036-1695": return "current_wle_fp4_10_07";
                case "1654-1285-7119": return "current_wle_fp4_10_08_m";
                case "9420-4455-9184": return "current_wle_fp4_10_08";
                case "1632-2850-0932": return "current_wle_fp4_10_11";
                case "9536-3101-2748": return "current_wle_fp4_10_12";
                case "6242-6736-1505": return "current_wle_fp4_10_20";
                case "2742-4277-9915": return "current_wle_fp4_10_21";
                case "6377-3695-4327": return "current_wle_fp4_10_0_01";
                case "7833-7019-9317": return "current_wle_fp4_10_0_02";
                // 265

                case "3825-5284-9253": return "current_wle_fp5_2_01_01";
                case "3790-8589-1520": return "current_wle_fp5_2_01_02";
                case "3737-3279-8628": return "current_wle_fp5_2_01_03";
                case "3514-5956-8272": return "current_wle_fp5_2_01_04";
                case "9148-5289-0823": return "current_wle_fp5_2_01";
                case "2108-0722-2123": return "current_wle_fp5_2_02_01";
                case "1963-5709-2926": return "current_wle_fp5_2_02_02";
                case "1809-4502-9184": return "current_wle_fp5_2_02_03";
                case "1789-1894-0232": return "current_wle_fp5_2_02_04";
                case "0763-8856-0715": return "current_wle_fp5_2_02_05";
                case "0567-7490-3659": return "current_wle_fp5_2_02_06";
                case "9103-2158-7081": return "current_wle_fp5_2_02";
                case "8411-4776-5845": return "current_wle_fp5_2_03";
                case "7572-4671-3585": return "current_wle_fp5_2_04_01";
                case "3869-1711-4431": return "current_wle_fp5_2_04_02";
                case "6142-2413-9656": return "current_wle_fp5_2_04_03";
                case "8800-7141-3616": return "current_wle_fp5_2_04_04";
                case "7572-2277-3000": return "current_wle_fp5_2_04";
                // case "8800-7141-3616": return "current_wle_fp5_2_05_01";
                // case "7572-4671-3585": return "current_wle_fp5_2_05_02";
                // case "6142-2413-9656": return "current_wle_fp5_2_05_03";
                // case "3869-1711-4431": return "current_wle_fp5_2_05_04";
                case "7110-3642-5704": return "current_wle_fp5_2_05";
                case "6376-5036-1691": return "current_wle_fp5_2_06";
                case "6010-5013-1494": return "current_wle_fp5_2_07";
                // 286

                case "1721-2831-3075": return "current_wle_fp5_3_05_01";
                case "0986-9263-8815": return "current_wle_fp5_3_05_02_01";
                case "9211-4929-5701": return "current_wle_fp5_4_01_01";
                case "9018-1364-7923": return "current_wle_fp5_4_01_02";
                case "8567-8219-8609": return "current_wle_fp5_4_01_03";
                case "5880-2278-9229": return "current_wle_fp5_4_01_04";
                case "2319-3617-4913": return "current_wle_fp5_4_01_05";
                // 293

                case "4063-2641-0298": return "current_wle_fp5_10_01";
                case "8299-0006-6070": return "current_wle_fp5_10_0_01";
                case "3428-8531-9101": return "current_wle_fp5_10_0_02";
                case "4040-5736-4713": return "current_wle_fp5_10_0_03";
                // case "9212-2213-6190": return "current_wle_fp5_10_1_01";
                // case "6595-2972-5515": return "current_wle_fp5_10_1_02";
                // case "9212-2213-6190": return "current_wle_fp5_10_2_01";
                // case "6595-2972-5515": return "current_wle_fp5_10_2_02";
                case "9343-2310-4786": return "current_wle_fp5_10_2_03";
                case "8853-7641-5813": return "current_wle_fp5_10_2_04";
                case "7495-6827-7086": return "current_wle_fp5_10_2_05";
                case "7154-6896-8797": return "current_wle_fp5_10_2_06";
                case "6043-6557-4039": return "current_wle_fp5_10_2_07";
                case "5608-7529-2030": return "current_wle_fp5_10_2_08";
                case "3906-5688-1706": return "current_wle_fp5_10_2_09";
                case "0358-6970-6379": return "current_wle_fp5_10_2_10";
                case "3969-7959-8728": return "current_wle_fp5_10_2_11";
                // 306

                case "0321-9239-5763": return "current_wle_fp5_wk3_1_01";
                case "5272-3542-2094": return "current_wle_fp5_wk3_1_02";
                // case "0986-9263-8815": return "current_wle_fp5_wk3_1_03";
                case "0936-8338-2826": return "current_wle_fp5_wk3_1_04";
                case "3851-6647-1283": return "current_wle_fp5_wk3_1_05";
                // case "2046-9186-5835": return "current_wle_fp5_wk3_1_06";
                case "3705-0014-9536": return "current_wle_fp5_wk3_2_01";
                case "9323-9497-7975": return "current_wle_fp5_wk3_2_02";
                case "5732-7608-2538": return "current_wle_fp5_wk3_2_03";
                case "7367-3353-4703": return "current_wle_fp5_wk3_2_04";
                case "1287-5978-1559": return "current_wle_fp5_wk3_2_05";
                case "5195-8821-0000": return "current_wle_fp5_wk3_3_01";
                case "2240-5868-2714": return "current_wle_fp5_wk3_3_02";
                case "8497-6718-8016": return "current_wle_fp5_wk3_3_03";
                case "4915-3461-7638": return "current_wle_fp5_wk3_3_04";
                // 319

                case "8402-8758-0032": return "current_wle_fp5_falloween_1_01";
                case "6240-3462-3481": return "current_wle_fp5_falloween_1_02";
                case "6121-7790-1504": return "current_wle_fp5_falloween_1_03";
                case "6067-2941-9905": return "current_wle_fp5_falloween_1_04";
                case "5651-2398-0054": return "current_wle_fp5_falloween_1_05";
                case "5024-3460-6816": return "current_wle_fp5_falloween_1_06";
                case "4849-8243-4656": return "current_wle_fp5_falloween_1_07";
                case "4488-9051-9724": return "current_wle_fp5_falloween_1_08";
                case "4218-2924-3419": return "current_wle_fp5_falloween_1_09";
                case "4176-6208-9294": return "current_wle_fp5_falloween_1_10";
                case "4084-9651-9519": return "current_wle_fp5_falloween_1_11";
                case "2735-9945-7419": return "current_wle_fp5_falloween_1_12";
                case "2306-3017-3066": return "current_wle_fp5_falloween_1_13";
                case "1401-4773-3736": return "current_wle_fp5_falloween_1_14";
                case "5664-5599-7711": return "current_wle_fp5_falloween_2_01";
                case "1799-5939-4570": return "current_wle_fp5_falloween_2_02";
                case "5484-7576-2527": return "current_wle_fp5_falloween_2_03_01";
                case "7175-6459-2516": return "current_wle_fp5_falloween_2_03_02";
                case "7190-1032-9714": return "current_wle_fp5_falloween_2_03_03";
                case "8090-4478-3076": return "current_wle_fp5_falloween_2_03_04";
                case "3613-2575-7647": return "current_wle_fp5_falloween_2_03_05";
                case "0755-6177-0051": return "current_wle_fp5_falloween_2_03_06";
                case "0655-0213-7224": return "current_wle_fp5_falloween_2_03";
                case "8631-7291-8107": return "current_wle_fp5_falloween_3_01";
                case "4501-8526-4946": return "current_wle_fp5_falloween_3_02";
                case "1100-1065-3063": return "current_wle_fp5_falloween_3_03";
                case "0069-7749-5727": return "current_wle_fp5_falloween_3_04";
                case "9994-8017-8706": return "current_wle_fp5_falloween_4_01";
                case "9657-6107-5958": return "current_wle_fp5_falloween_4_02";
                case "9368-3971-3271": return "current_wle_fp5_falloween_4_03";
                case "9258-3697-5491": return "current_wle_fp5_falloween_4_04";
                case "7928-7118-4810": return "current_wle_fp5_falloween_4_05";
                case "7120-7897-2692": return "current_wle_fp5_falloween_4_06";
                case "6533-6356-0472": return "current_wle_fp5_falloween_4_07";
                case "6396-1389-0514": return "current_wle_fp5_falloween_4_08";
                case "5741-2031-8043": return "current_wle_fp5_falloween_4_09";
                case "5476-2662-9927": return "current_wle_fp5_falloween_4_10";
                case "4430-7014-2805": return "current_wle_fp5_falloween_4_11";
                case "4382-9645-4115": return "current_wle_fp5_falloween_4_12";
                case "3624-9104-8937": return "current_wle_fp5_falloween_4_13";
                case "2864-3013-2704": return "current_wle_fp5_falloween_4_14";
                case "1448-1995-9095": return "current_wle_fp5_falloween_4_15";
                case "0797-7676-9358": return "current_wle_fp5_falloween_4_16";
                case "0788-2763-9253": return "current_wle_fp5_falloween_4_17";
                case "0634-3389-9948": return "current_wle_fp5_falloween_4_18";
                case "0216-4798-7097": return "current_wle_fp5_falloween_4_19";
                case "9266-0794-7999": return "current_wle_fp5_falloween_5_01";
                case "8757-8658-1590": return "current_wle_fp5_falloween_5_02";
                case "5608-5038-4603": return "current_wle_fp5_falloween_5_03";
                case "4310-0672-7447": return "current_wle_fp5_falloween_5_04";
                case "2573-8965-6602": return "current_wle_fp5_falloween_5_05";
                case "1793-2460-5757": return "current_wle_fp5_falloween_5_06";
                case "1029-3640-0683": return "current_wle_fp5_falloween_5_07";
                case "1885-0687-3381": return "current_wle_fp5_falloween_6_01";
                case "2046-9186-5835": return "current_wle_fp5_falloween_6_02";
                case "9944-2261-7096": return "current_wle_fp5_falloween_6_03";
                case "8245-2536-0397": return "current_wle_fp5_falloween_7_01_01";
                case "5120-5302-3364": return "current_wle_fp5_falloween_7_01_02";
                case "2005-7268-3932": return "current_wle_fp5_falloween_7_01_03";
                case "9744-9959-6162": return "current_wle_fp5_falloween_7_01_04";
                case "6044-9178-1352": return "current_wle_fp5_falloween_7_01_05";
                case "7336-7844-4312": return "current_wle_fp5_falloween_7_01_06";
                case "7153-1520-5779": return "current_wle_fp5_falloween_7_01_07";
                case "5105-6932-4910": return "current_wle_fp5_falloween_7_01_08";
                case "0003-1341-4714": return "current_wle_fp5_falloween_7_01_09";
                case "3499-1607-5514": return "current_wle_fp5_falloween_7_02_01";
                case "5031-9941-3572": return "current_wle_fp5_falloween_7_03_01";
                case "9256-6209-5924": return "current_wle_fp5_falloween_7_04_01";
                case "0368-4641-9952": return "current_wle_fp5_falloween_7_04_02";
                case "3357-4326-2937": return "current_wle_fp5_falloween_7_04_03";
                case "4095-3239-4107": return "current_wle_fp5_falloween_7_04_04";
                case "2438-2715-1526": return "current_wle_fp5_falloween_7_05_01";
                case "2247-7408-5638": return "current_wle_fp5_falloween_7_05_02";
                case "2525-3294-8408": return "current_wle_fp5_falloween_7_05_03";
                case "8138-0025-5565": return "current_wle_fp5_falloween_7_05_04";
                case "2322-8913-6415": return "current_wle_fp5_falloween_7_06_01";
                case "5658-3251-2803": return "current_wle_fp5_falloween_7_06_02";
                // case "1885-0687-3381": return "current_wle_fp5_falloween_7_06_03";
                case "7399-1384-6084": return "current_wle_fp5_falloween_7_06_04";
                case "6733-6429-1092": return "current_wle_fp5_falloween_7_06_05";
                case "1511-8704-2066": return "current_wle_fp5_falloween_7_08_01";
                case "0851-5131-6366": return "current_wle_fp5_falloween_7_10_01";
                case "4157-3882-8459": return "current_wle_fp5_falloween_7_10_02";
                case "9477-8846-0602": return "current_wle_fp5_falloween_9_01";
                case "8003-0564-1815": return "current_wle_fp5_falloween_9_02";
                case "4672-3819-0320": return "current_wle_fp5_falloween_9_03";
                case "0453-7588-2618": return "current_wle_fp5_falloween_9_04";
                case "6237-3643-0250": return "current_wle_fp5_falloween_9_05";
                case "8245-1512-8620": return "current_wle_fp5_falloween_10_01";
                case "0592-4433-7070": return "current_wle_fp5_falloween_10_02";
                case "2919-6281-3490": return "current_wle_fp5_falloween_11_01";
                case "0389-3465-9605": return "current_wle_fp5_falloween_12_01";
                case "1843-2013-5149": return "current_wle_fp5_falloween_12_02";
                case "0970-9754-5183": return "current_wle_fp5_falloween_13_01";
                case "2532-1174-5060": return "current_wle_fp5_falloween_14_01";
                case "0697-1204-1880": return "current_wle_fp5_falloween_15_01";
                // 414

                case "3916-1541-0243": return "current_wle_fp6_1_01";
                case "3948-0002-3038": return "current_wle_fp6_1_02";
                case "5043-4340-1476": return "current_wle_fp6_1_03";
                case "5047-6237-6739": return "current_wle_fp6_1_04";
                case "6277-7822-1251": return "current_wle_fp6_1_05";
                case "3604-8131-3419": return "current_wle_fp6_1_06";
                case "6765-5208-2758": return "current_wle_fp6_1_07";
                case "7320-9822-7602": return "current_wle_fp6_1_08";
                case "7861-4822-7936": return "current_wle_fp6_1_09";
                case "8180-8790-0009": return "current_wle_fp6_1_10";
                case "8620-2155-3954": return "current_wle_fp6_2_01";
                case "8991-8326-4093": return "current_wle_fp6_2_02";
                case "9288-2697-0408": return "current_wle_fp6_2_03";
                case "9377-9029-3657": return "current_wle_fp6_2_04";
                case "9542-1740-6424": return "current_wle_fp6_2_05";
                case "6893-9005-0968": return "current_wle_fp6_2_06";
                case "9678-6212-2370": return "current_wle_fp6_2_07";
                case "3523-6255-6638": return "current_wle_fp6_2_08";
                case "3224-9787-0835": return "current_wle_fp6_2_09";
                case "3071-2833-2289": return "current_wle_fp6_3_01";
                case "2569-1984-5105": return "current_wle_fp6_3_02";
                case "2324-3489-1583": return "current_wle_fp6_3_03";
                // case "2306-9699-8428": return "current_wle_fp6_3_04";
                case "1964-6897-7083": return "current_wle_fp6_3_05";
                case "1076-4735-0083": return "current_wle_fp6_3_06";
                case "0255-4663-0790": return "current_wle_fp6_3_07";
                case "8297-7587-2491": return "current_wle_fp6_3_08";
                // 440

                case "0676-8023-9854": return "current_wle_fp6_wk2_01";
                case "8224-4301-6958": return "current_wle_fp6_wk2_02";
                case "8703-7887-6944": return "current_wle_fp6_wk2_03";
                case "7110-8808-3745": return "current_wle_fp6_wk2_04";
                case "9710-6876-4207": return "current_wle_fp6_wk2_05";
                case "8354-6671-5374": return "current_wle_fp6_wk2_06";
                case "9565-7750-0483": return "current_wle_fp6_wk2_07";
                case "6719-0148-0904": return "current_wle_fp6_wk2_08";
                case "5275-2973-1284": return "current_wle_fp6_wk2_09";
                case "6099-8747-6282": return "current_wle_fp6_wk2_10";
                case "5550-6909-7405": return "current_wle_fp6_wk2_11";
                case "8568-9006-6425": return "current_wle_fp6_wk2_1_01";
                case "8249-0033-6623": return "current_wle_fp6_wk2_1_02";
                case "7780-1414-4490": return "current_wle_fp6_wk2_1_03";
                case "9303-5896-3150": return "current_wle_fp6_wk2_1_04";
                case "4115-7916-9074": return "current_wle_fp6_wk2_1_05";
                case "7122-5244-2992": return "current_wle_fp6_wk2_1_06";
                case "1588-3248-4201": return "current_wle_fp6_wk2_1_07";
                case "8574-2024-6514": return "current_wle_fp6_wk2_1_08";
                case "3750-6876-3150": return "current_wle_fp6_wk2_1_09";
                // 460

                case "9038-5995-0613": return "wle_discover_level_wk2_001";
                case "4904-4223-0526": return "wle_discover_level_wk2_002";
                case "5275-3298-6913": return "wle_discover_level_wk2_003";
                case "4398-7555-7009": return "wle_discover_level_wk2_004";
                case "5362-9288-2858": return "wle_discover_level_wk2_005";
                case "3291-2140-7680": return "wle_discover_level_wk2_006";
                case "8237-7159-3977": return "wle_discover_level_wk2_007";
                case "2964-4225-1009": return "wle_discover_level_wk2_008";
                case "6421-3169-3667": return "wle_discover_level_wk2_009";
                case "1796-2312-9416": return "wle_discover_level_wk2_010";
                case "3533-8159-6854": return "wle_discover_level_wk2_011";
                case "5086-5243-9680": return "wle_discover_level_wk2_012";
                case "1389-8884-2061": return "wle_discover_level_wk2_013";
                case "9324-7797-9946": return "wle_discover_level_wk2_014";
                case "8128-3640-3460": return "wle_discover_level_wk2_015";
                case "1544-6104-2453": return "wle_discover_level_wk2_016";
                case "5441-5981-3430": return "wle_discover_level_wk2_017";
                case "0089-2044-0703": return "wle_discover_level_wk2_018";
                case "5121-0443-6799": return "wle_discover_level_wk2_019";
                case "7571-2950-1286": return "wle_discover_level_wk2_020";
                case "2246-3279-7448": return "wle_discover_level_wk2_021";
                case "8787-5820-1328": return "wle_discover_level_wk2_022";
                case "8257-1760-0020": return "wle_discover_level_wk2_023";
                case "8207-5307-7171": return "wle_discover_level_wk2_024";
                case "2722-3817-6628": return "wle_discover_level_wk2_025";
                case "0777-3024-0228": return "wle_discover_level_wk2_026";
                case "7721-7953-1307": return "wle_discover_level_wk2_027";
                case "8266-7988-9989": return "wle_discover_level_wk2_028";
                case "8696-7950-5029": return "wle_discover_level_wk2_029";
                case "0707-0032-0823": return "wle_discover_level_wk2_030";
                case "9015-3378-5798": return "wle_discover_level_wk2_031";
                case "1907-7914-9110": return "wle_discover_level_wk2_032";
                case "7244-3167-1509": return "wle_discover_level_wk2_033";
                case "1002-4872-3000": return "wle_discover_level_wk2_034";
                case "9410-3554-3998": return "wle_discover_level_wk2_035";
                case "5353-5711-2408": return "wle_discover_level_wk2_036";
                case "2339-7794-6638": return "wle_discover_level_wk2_037";
                case "7984-3310-2143": return "wle_discover_level_wk2_038";
                case "2118-2980-8240": return "wle_discover_level_wk2_039";
                case "3936-9524-9756": return "wle_discover_level_wk2_040";
                case "1893-8327-1728": return "wle_discover_level_wk2_041";
                case "1225-4872-0162": return "wle_discover_level_wk2_042";
                case "2910-3365-2494": return "wle_discover_level_wk2_043";
                case "5244-0881-7559": return "wle_discover_level_wk2_044";
                case "2306-9699-8428": return "wle_discover_level_wk2_045";
                case "3317-4973-3298": return "wle_discover_level_wk2_046";
                case "4916-2277-5636": return "wle_discover_level_wk2_047";
                case "0755-3864-7172": return "wle_discover_level_wk2_048";
                case "4750-3581-7195": return "wle_discover_level_wk2_049";
                case "3874-0613-2428": return "wle_discover_level_wk2_050";
                // 510

                case "9334-9348-1212": return "wle_s10_bt_round_001";
                case "9596-6865-9561": return "wle_s10_bt_round_002";
                case "1575-3397-3154": return "wle_s10_bt_round_003";
                case "4676-0921-9816": return "wle_s10_bt_round_004";
                // 514

                case "5065-6325-4646": return "wle_s10_cf_round_001";
                case "6581-1890-6801": return "wle_s10_cf_round_002";
                case "6656-2033-4324": return "wle_s10_cf_round_003";
                case "7277-7302-9886": return "wle_s10_cf_round_004";
                // 518

                case "9100-1195-6052": return "wle_mrs_bagel_opener_1";
                case "9299-0471-0746": return "wle_mrs_bagel_opener_2";
                case "4805-7882-8305": return "wle_mrs_bagel_opener_3";
                case "9840-2154-6787": return "wle_mrs_bagel_opener_4";
                case "8176-1884-5016": return "wle_mrs_bagel_filler_1";
                case "1008-8179-1628": return "wle_mrs_bagel_filler_2";
                case "3947-7683-6106": return "wle_mrs_bagel_filler_3";
                case "8489-4249-0438": return "wle_mrs_bagel_filler_4";
                case "7476-9346-3120": return "wle_mrs_bagel_final_1";
                case "9201-4959-0276": return "wle_mrs_bagel_final_2";
                // 528

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
                // 538

                default: return shareCode;
            }
        }
        public string[] FindCreativeAuthor(JsonElement authorData) {
            string[] creativeAuthorInfo = { "N/A", "N/A" };
            string onlinePlatformId = string.Empty;
            string onlinePlatformNickname = string.Empty;
            string[] onlinePlatformIds = { "eos", "steam", "psn", "xbl", "nso" }; // NOTE: "nso" may not be used
            foreach (string onlinePlatformIdInfo in onlinePlatformIds) {
                if (authorData.TryGetProperty(onlinePlatformIdInfo, out JsonElement onlinePlatformNicknameInfo)) {
                    if (!string.IsNullOrEmpty(onlinePlatformId)) { onlinePlatformId += ";"; }
                    onlinePlatformId += onlinePlatformIdInfo;
                    if (!string.IsNullOrEmpty(onlinePlatformNickname)) { onlinePlatformNickname += ";"; }
                    onlinePlatformNickname += onlinePlatformNicknameInfo.GetString();
                }
            }
            if (string.IsNullOrEmpty(onlinePlatformId)) { return creativeAuthorInfo; }

            creativeAuthorInfo[0] = onlinePlatformId;
            creativeAuthorInfo[1] = onlinePlatformNickname;
            return creativeAuthorInfo;
        }
        public string GetCountryCode(string dbFile, string ip) {
            if (!File.Exists(dbFile)) { return string.Empty; }

            try {
                Reader reader = new Reader(dbFile);
                IPAddress parsedIp = IPAddress.Parse(ip);
                Dictionary<string, string> ipData = reader.Find<Dictionary<string, string>>(parsedIp);
                return $"{ipData["country"]}";
            } catch {
                return string.Empty;
            }
        }
        public JsonElement GetApiData(string apiUrl, string apiEndPoint) {
            JsonElement resJroot;
            using (ApiWebClient web = new ApiWebClient()) {
                string responseJsonString = web.DownloadString($"{apiUrl}{apiEndPoint}");
                JsonDocument jdom = JsonDocument.Parse(responseJsonString);
                resJroot = jdom.RootElement;
            }
            return resJroot;
        }
#if AllowUpdate
        private bool CheckForUpdate(bool silent) {
            using (ZipWebClient web = new ZipWebClient()) {
                // string assemblyInfo = web.DownloadString(@"https://raw.githubusercontent.com/Micdu70/FallGuysStats/master/Properties/AssemblyInfo.cs");
                string assemblyInfo = web.DownloadString(@"https://raw.githubusercontent.com/ShootMe/FallGuysStats/master/Properties/AssemblyInfo.cs");
                int index = assemblyInfo.IndexOf("AssemblyVersion(");
                if (index > 0) {
                    int indexEnd = assemblyInfo.IndexOf("\")", index);
                    Version currentVersion = Assembly.GetEntryAssembly().GetName().Version;
                    Version newVersion = new Version(assemblyInfo.Substring(index + 17, indexEnd - index - 17));
                    if (newVersion > currentVersion) {
                        if (MessageBox.Show(this,
                                $"{Multilingual.GetWord("message_update_question_prefix")} ( v{newVersion.ToString(2)} ) {Multilingual.GetWord("message_update_question_suffix")}",
                                $"{Multilingual.GetWord("message_update_question_caption")}",
                                MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK) {
                            this.SaveWindowState();
                            this.Hide();
                            this.overlay.Hide();
                            this.DownloadNewVersion(web);
                            this.isUpdate = true;
                            this.Close();
                            return true;
                        }
                    } else if (!silent) {
                        MessageBox.Show(this,
                            $"{Multilingual.GetWord("message_update_latest_version")}",
                            $"{Multilingual.GetWord("message_update_question_caption")}",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                } else if (!silent) {
                    MessageBox.Show(this,
                        $"{Multilingual.GetWord("message_update_not_determine_version")}",
                        $"{Multilingual.GetWord("message_update_error_caption")}",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            return false;
        }
        public void DownloadNewVersion(ZipWebClient web) {
            // byte[] data = web.DownloadData($"https://raw.githubusercontent.com/Micdu70/FallGuysStats/master/FallGuysStats.zip");
            byte[] data = web.DownloadData($"https://github.com/ShootMe/FallGuysStats/releases/latest/download/FallGuysStats.zip");
            string exeName = null;
            using (MemoryStream ms = new MemoryStream(data)) {
                using (ZipArchive zipFile = new ZipArchive(ms, ZipArchiveMode.Read)) {
                    foreach (var entry in zipFile.Entries) {
                        if (entry.Name.IndexOf(".exe", StringComparison.OrdinalIgnoreCase) > 0) {
                            exeName = entry.Name;
                        }
                        if (File.Exists(entry.Name)) { File.Move(entry.Name, $"{entry.Name}.bak"); }
                        entry.ExtractToFile(entry.Name, true);
                    }
                }
            }
            Process.Start(new ProcessStartInfo(exeName));
        }
#endif
        private async void MenuSettings_Click(object sender, EventArgs e) {
            try {
                using (Settings settings = new Settings()) {
                    //settings.Icon = this.Icon;
                    settings.CurrentSettings = this.CurrentSettings;
                    settings.BackMaxSize = 32;
                    settings.BackImagePadding = new Padding(20, 19, 0, 0);
                    settings.StatsForm = this;
                    settings.Overlay = this.overlay;
                    string lastLogPath = this.CurrentSettings.LogPath;
                    this.EnableInfoStrip(false);
                    this.EnableMainMenu(false);
                    if (settings.ShowDialog(this) == DialogResult.OK) {
                        this.CurrentSettings = settings.CurrentSettings;
                        this.SetTheme(CurrentTheme);
                        this.SaveUserSettings();
                        if (this.currentLanguage != CurrentLanguage) {
                            this.ChangeMainLanguage();
                            this.UpdateTotals();
                            this.gridDetails.ChangeContextMenuLanguage();
                            this.UpdateGridRoundName();
                            this.overlay.ChangeLanguage();
                        }
                        this.ChangeLaunchPlatformLogo(this.CurrentSettings.LaunchPlatform);
                        this.UpdateHoopsieLegends();
                        this.overlay.Opacity = this.CurrentSettings.OverlayBackgroundOpacity / 100D;
                        this.overlay.SetBackgroundResourcesName(this.CurrentSettings.OverlayBackgroundResourceName, this.CurrentSettings.OverlayTabResourceName);
                        this.SetCurrentProfileIcon(this.AllProfiles.FindIndex(p => p.ProfileId == this.GetCurrentProfileId() && !string.IsNullOrEmpty(p.LinkedShowId)) != -1);
                        this.Refresh();
                        this.logFile.autoChangeProfile = this.CurrentSettings.AutoChangeProfile;
                        this.logFile.preventOverlayMouseClicks = this.CurrentSettings.PreventOverlayMouseClicks;

                        IsOverlayPingVisible = this.CurrentSettings.OverlayVisible && !this.CurrentSettings.HideRoundInfo && (this.CurrentSettings.SwitchBetweenPlayers || this.CurrentSettings.OnlyShowPing);

                        if (string.IsNullOrEmpty(lastLogPath) != string.IsNullOrEmpty(this.CurrentSettings.LogPath) ||
                            (!string.IsNullOrEmpty(lastLogPath) && lastLogPath.Equals(this.CurrentSettings.LogPath, StringComparison.OrdinalIgnoreCase))) {
                            await this.logFile.Stop();
                            string logFilePath = Path.Combine($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}Low", "Mediatonic", "FallGuys_client");
                            if (!string.IsNullOrEmpty(this.CurrentSettings.LogPath)) {
                                logFilePath = this.CurrentSettings.LogPath;
                            }
                            this.logFile.Start(logFilePath, LOGFILENAME);
                        }

                        this.overlay.ArrangeDisplay(string.IsNullOrEmpty(this.CurrentSettings.OverlayFixedPosition) ? this.CurrentSettings.FlippedDisplay : this.CurrentSettings.FixedFlippedDisplay, this.CurrentSettings.ShowOverlayTabs,
                            this.CurrentSettings.HideWinsInfo, this.CurrentSettings.HideRoundInfo, this.CurrentSettings.HideTimeInfo,
                            this.CurrentSettings.OverlayColor, string.IsNullOrEmpty(this.CurrentSettings.OverlayFixedPosition) ? this.CurrentSettings.OverlayWidth : this.CurrentSettings.OverlayFixedWidth, string.IsNullOrEmpty(this.CurrentSettings.OverlayFixedPosition) ? this.CurrentSettings.OverlayHeight : this.CurrentSettings.OverlayFixedHeight,
                            this.CurrentSettings.OverlayFontSerialized, this.CurrentSettings.OverlayFontColorSerialized);
                    } else {
                        this.overlay.Opacity = this.CurrentSettings.OverlayBackgroundOpacity / 100D;
                    }
                    this.EnableInfoStrip(true);
                    this.EnableMainMenu(true);
                }
            } catch (Exception ex) {
                MessageBox.Show(this, ex.ToString(), $"{Multilingual.GetWord("message_program_error_caption")}",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.EnableInfoStrip(true);
                this.EnableMainMenu(true);
            }
        }
        private void MenuOverlay_Click(object sender, EventArgs e) {
            this.ToggleOverlay(overlay);
        }
        private void ToggleOverlay(Overlay overlay) {
            if (overlay.Visible) {
                IsOverlayPingVisible = false;
                overlay.Hide();
                this.menuOverlay.Image = Properties.Resources.stat_gray_icon;
                this.menuOverlay.Text = $"{Multilingual.GetWord("main_show_overlay")}";
                if (!overlay.IsFixed()) {
                    this.CurrentSettings.OverlayLocationX = overlay.Location.X;
                    this.CurrentSettings.OverlayLocationY = overlay.Location.Y;
                    this.CurrentSettings.OverlayWidth = overlay.Width;
                    this.CurrentSettings.OverlayHeight = overlay.Height;
                }
                this.CurrentSettings.OverlayVisible = false;
                this.SaveUserSettings();
            } else {
                IsOverlayPingVisible = !this.CurrentSettings.HideRoundInfo && (this.CurrentSettings.SwitchBetweenPlayers || this.CurrentSettings.OnlyShowPing);
                overlay.TopMost = !this.CurrentSettings.OverlayNotOnTop;
                overlay.Show();
                this.menuOverlay.Image = Properties.Resources.stat_icon;
                this.menuOverlay.Text = $"{Multilingual.GetWord("main_hide_overlay")}";
                this.CurrentSettings.OverlayVisible = true;
                this.SaveUserSettings();

                if (overlay.IsFixed()) {
                    if (this.CurrentSettings.OverlayFixedPositionX.HasValue &&
                        this.IsOnScreen(this.CurrentSettings.OverlayFixedPositionX.Value, this.CurrentSettings.OverlayFixedPositionY.Value, overlay.Width, overlay.Height)) {
                        overlay.FlipDisplay(this.CurrentSettings.FixedFlippedDisplay);
                        overlay.Location = new Point(this.CurrentSettings.OverlayFixedPositionX.Value, this.CurrentSettings.OverlayFixedPositionY.Value);
                    } else {
                        Screen screen = this.GetCurrentScreen(this.Location);
                        if (this.CurrentSettings.FlippedDisplay) {
                            overlay.Location = new Point(screen.WorkingArea.Right - overlay.Width - screen.WorkingArea.Right + overlay.Width, screen.WorkingArea.Top);
                        } else {
                            overlay.Location = new Point(screen.WorkingArea.Right - overlay.Width, screen.WorkingArea.Top);
                        }
                    }
                } else {
                    overlay.Location = this.CurrentSettings.OverlayLocationX.HasValue && this.IsOnScreen(this.CurrentSettings.OverlayLocationX.Value, this.CurrentSettings.OverlayLocationY.Value, overlay.Width, overlay.Height)
                                       ? new Point(this.CurrentSettings.OverlayLocationX.Value, this.CurrentSettings.OverlayLocationY.Value)
                                       : this.Location;
                }
            }
        }
        private void MenuHelp_Click(object sender, EventArgs e) {
            this.LaunchHelpInBrowser();
        }
        private void MenuEditProfiles_Click(object sender, EventArgs e) {
            try {
                using (EditProfiles editProfiles = new EditProfiles()) {
                    //editProfiles.Icon = this.Icon;
                    editProfiles.StatsForm = this;
                    editProfiles.Profiles = this.AllProfiles;
                    editProfiles.AllStats = this.RoundDetails.FindAll().ToList();
                    this.EnableInfoStrip(false);
                    this.EnableMainMenu(false);
                    editProfiles.ShowDialog(this);
                    this.EnableInfoStrip(true);
                    this.EnableMainMenu(true);
                    lock (this.StatsDB) {
                        this.StatsDB.BeginTrans();
                        this.AllProfiles = editProfiles.Profiles;
                        this.Profiles.DeleteAll();
                        this.Profiles.InsertBulk(this.AllProfiles);
                        if (editProfiles.AllStats.Count != this.RoundDetails.Count()) {
                            this.AllStats = editProfiles.AllStats;
                            this.RoundDetails.DeleteAll();
                            this.RoundDetails.InsertBulk(this.AllStats);
                            this.AllStats.Clear();
                        }
                        this.StatsDB.Commit();
                    }
                    this.ReloadProfileMenuItems();
                }
            } catch (Exception ex) {
                MessageBox.Show(this, ex.ToString(), $"{Multilingual.GetWord("message_program_error_caption")}",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.EnableInfoStrip(true);
                this.EnableMainMenu(true);
            }
        }
        private void MenuLaunchFallGuys_Click(object sender, EventArgs e) {
            try {
                this.LaunchGame(false);
            } catch (Exception ex) {
                MessageBox.Show(this, ex.ToString(), $"{Multilingual.GetWord("message_program_error_caption")}",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void MenuTodaysShow_Click(object sender, EventArgs e) {
            try {
                Process.Start(@"https://fallguys-db.pages.dev/upcoming_shows");
            } catch (Exception ex) {
                MessageBox.Show(this, ex.ToString(), $"{Multilingual.GetWord("message_program_error_caption")}",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void LinkToIPinfoWebsite_Click(object sender, EventArgs e) {
            try {
                Process.Start("https://ipinfo.io");
            } catch (Exception ex) {
                MessageBox.Show(this, ex.ToString(), $"{Multilingual.GetWord("message_program_error_caption")}",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public bool IsOnScreen(int x, int y, int w, int h) {
            Screen[] screens = Screen.AllScreens;
            foreach (Screen screen in screens) {
                if (screen.WorkingArea.Contains(new Point(x, y)) || screen.WorkingArea.Contains(new Point(x + w, y + h))) {
                    return true;
                }
            }
            return false;
        }
        public Screen GetCurrentScreen(Point location) {
            Screen[] scr = Screen.AllScreens;
            Screen screen = Screen.PrimaryScreen;
            foreach (Screen s in scr) {
                if (s.WorkingArea.Contains(location)) {
                    screen = s;
                }
            }
            return screen;
        }
        private void ChangeLaunchPlatformLogo(int launchPlatform) {
            this.menuLaunchFallGuys.Image = launchPlatform == 0
                                            ? Properties.Resources.epic_main_icon
                                            : Properties.Resources.steam_main_icon;
        }
        private void ChangeMainLanguage() {
            this.currentLanguage = CurrentLanguage;
            this.Text = $"{Multilingual.GetWord("main_fall_guys_stats")} v{Assembly.GetExecutingAssembly().GetName().Version.ToString(2)}";
            int TextWidth = TextRenderer.MeasureText(this.Text, Overlay.GetDefaultFont(18, this.currentLanguage)).Width;
            this.BackImagePadding = new Padding(TextWidth + (this.currentLanguage == 2 ? 100 : this.currentLanguage == 3 ? 70 : 45), 8, 0, 0);

            this.lblManualUpdateVersion.Text = Multilingual.GetWord("main_subtitle");

            this.menu.Font = Overlay.GetMainFont(12);
            this.infoStrip.Font = Overlay.GetMainFont(13);
            this.infoStrip2.Font = Overlay.GetMainFont(13);

            this.dataGridViewCellStyle1.Font = Overlay.GetMainFont(10);
            this.dataGridViewCellStyle2.Font = Overlay.GetMainFont(12);
            //this.SetMainDataGridView();

            this.menuSettings.Text = Multilingual.GetWord("main_settings");
            this.menuFilters.Text = Multilingual.GetWord("main_filters");
            this.menuStatsFilter.Text = Multilingual.GetWord("main_stats");
            this.menuCustomRangeStats.Text = Multilingual.GetWord("main_custom_range");
            this.menuAllStats.Text = Multilingual.GetWord("main_all");
            this.menuSeasonStats.Text = Multilingual.GetWord("main_season");
            this.menuWeekStats.Text = Multilingual.GetWord("main_week");
            this.menuDayStats.Text = Multilingual.GetWord("main_day");
            this.menuSessionStats.Text = Multilingual.GetWord("main_session");
            this.menuPartyFilter.Text = Multilingual.GetWord("main_party_type");
            this.menuAllPartyStats.Text = Multilingual.GetWord("main_all");
            this.menuSoloStats.Text = Multilingual.GetWord("main_solo");
            this.menuPartyStats.Text = Multilingual.GetWord("main_party");
            this.menuProfile.Text = Multilingual.GetWord("main_profile");
            this.menuEditProfiles.Text = Multilingual.GetWord("main_profile_setting");
            this.menuOverlay.Text = !CurrentSettings.OverlayVisible
                                    ? Multilingual.GetWord("main_show_overlay")
                                    : Multilingual.GetWord("main_hide_overlay");
            // this.menuOverlay.ToolTipText = $"{Multilingual.GetWord("main_overlay_tooltip")}";
            this.menuUpdate.Text = Multilingual.GetWord("main_update");
            this.menuHelp.Text = Multilingual.GetWord("main_help");
            this.menuLaunchFallGuys.Text = Multilingual.GetWord("main_launch_fall_guys");
            this.menuTodaysShow.Text = $"{Multilingual.GetWord("main_todays_show")}";
            this.menuTodaysShow.ToolTipText = $"{Multilingual.GetWord("main_todays_show_tooltip")}";
        }
    }
}
