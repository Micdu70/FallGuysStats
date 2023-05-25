using System;
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
using System.Threading;
using System.Windows.Forms;
using LiteDB;
using Microsoft.Win32;
using MetroFramework;
using System.Text.RegularExpressions;
using MetroFramework.Components;

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
                string bugFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "bug.txt");
#if AllowUpdate
                bool isAppUpdatedOrRestarted = false;
                if (File.Exists(Path.GetFileName(Assembly.GetEntryAssembly().Location) + ".bak")
                    || File.Exists(bugFile)) {
                    isAppUpdatedOrRestarted = true;
                    if (File.Exists(bugFile)) {
                        try {
                            File.SetAttributes(bugFile, FileAttributes.Normal);
                            File.Delete(bugFile);
                        } catch { }
                    }
                }
                if (isAppUpdatedOrRestarted || !IsAlreadyRunning(CultureInfo.CurrentUICulture.Name.Substring(0, 2))) {
#else
                bool isAppRestarted = false;
                if (File.Exists(bugFile)) {
                    isAppRestarted = true;
                    try {
                        File.SetAttributes(bugFile, FileAttributes.Normal);
                        File.Delete(bugFile);
                    } catch { }
                }
                if (isAppRestarted || !IsAlreadyRunning(CultureInfo.CurrentUICulture.Name.Substring(0, 2))) {
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
        private static readonly string LOGNAME = "Player.log";
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

        public static bool IsGameHasBeenClosed = false;

        public static bool InShow = false;
        public static bool EndedShow = false;
        public static int LastServerPing = 0;

        public static List<string> succeededPlayerIds = new List<string>();

        public static int SavedRoundCount { get; set; }
        public static int NumPlayersSucceeded { get; set; }
        public static bool IsLastRoundRunning { get; set; }
        public static bool IsLastPlayedRoundStillPlaying { get; set; }

        public static DateTime LastRoundStart { get; set; } = DateTime.MinValue;
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
        public readonly DateTime startupTime = DateTime.UtcNow;
        private DateTime lastAddedShow = DateTime.MinValue;
        private int askedPreviousShows = 0;
        private readonly TextInfo textInfo;
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

        //private Point screenCenter;

        private bool shiftKeyToggle;//, ctrlKeyToggle;

        private readonly MetroToolTip mtt = new MetroToolTip();
        private readonly MetroToolTip cmtt = new MetroToolTip();

        private readonly DWM_WINDOW_CORNER_PREFERENCE windowConerPreference = DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUNDSMALL;

        private bool isStartUp = true;

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
            "event_only_slime_climb",
            "event_only_jump_club_template",
            "event_walnut_template",
            "event_only_survival_ss2_3009_0210_2022",
            "show_robotrampage_ss2_show1_template",
            "event_le_anchovy_template",
            "event_pixel_palooza_template",
            "xtreme_party",
            "fall_guys_creative_mode",
            "private_lobbies"
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
                    if (this.CurrentSettings.FrenchyEditionDB <= 3) {
                        this.CurrentSettings.Theme = 1;
                        this.CurrentSettings.OverlayBackgroundOpacity = 100;
                        this.CurrentSettings.HideOverlayPercentages = true;
                        this.CurrentSettings.WinsFilter = 1;
                        this.CurrentSettings.QualifyFilter = 1;
                        this.CurrentSettings.FastestFilter = 0;
                        this.CurrentSettings.OverlayColor = 0;
                        this.CurrentSettings.OverlayVisible = true;
                        this.CurrentSettings.SwitchBetweenPlayers = false;
                        this.CurrentSettings.PlayerByConsoleType = true;
                        this.CurrentSettings.ColorByRoundType = true;
                        this.CurrentSettings.AutoChangeProfile = false;
                        this.CurrentSettings.AutoUpdate = true;
                        this.CurrentSettings.Version = 28;
                        this.CurrentSettings.FrenchyEditionDB = 4;
                        this.UserSettings.Upsert(this.CurrentSettings);
                    }
                    if (this.CurrentSettings.FrenchyEditionDB == 4) {
                        this.CurrentSettings.WinPerDayGraphStyle = 1;
                        this.CurrentSettings.FrenchyEditionDB = 5;
                        this.UserSettings.Upsert(this.CurrentSettings);
                    }
                    if (this.CurrentSettings.FrenchyEditionDB == 5) {
                        this.CurrentSettings.EnableFallalyticsReporting = true;
                        this.CurrentSettings.Version = 29;
                        this.CurrentSettings.FrenchyEditionDB = 6;
                        this.UserSettings.Upsert(this.CurrentSettings);
                    }
                    if (this.CurrentSettings.FrenchyEditionDB == 6) {
                        this.CurrentSettings.SystemTrayIcon = false;
                        this.CurrentSettings.Version = 30;
                        this.CurrentSettings.FrenchyEditionDB = 7;
                        this.UserSettings.Upsert(this.CurrentSettings);
                    }
                    if (this.CurrentSettings.FrenchyEditionDB == 7) {
                        this.CurrentSettings.SwitchBetweenLongest = false;
                        this.CurrentSettings.OnlyShowLongest = false;
                        this.CurrentSettings.Version = 35;
                        this.CurrentSettings.FrenchyEditionDB = 8;
                        this.UserSettings.Upsert(this.CurrentSettings);
                    }
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

            this.InitializeComponent();

            this.textInfo = Thread.CurrentThread.CurrentCulture.TextInfo;

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
                        Overlay.SetDefaultFont(CurrentLanguage, 18);
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

            foreach (KeyValuePair<string, LevelStats> entry in LevelStats.ALL) {
                this.StatLookup.Add(entry.Key, entry.Value);
                this.StatDetails.Add(entry.Value);
            }

            this.RoundDetails.EnsureIndex(x => x.Name);
            this.RoundDetails.EnsureIndex(x => x.ShowID);
            this.RoundDetails.EnsureIndex(x => x.Round);
            this.RoundDetails.EnsureIndex(x => x.Start);
            this.RoundDetails.EnsureIndex(x => x.InParty);
            this.StatsDB.Commit();

            this.UpdateDatabaseVersion();

            this.InitMainDataGridView();

            this.ChangeMainLanguage();

            this.UpdateGridRoundName();

            this.UpdateHoopsieLegends();

            this.CurrentRound = new List<RoundInfo>();

            this.overlay = new Overlay { Text = @"Fall Guys Stats Overlay", StatsForm = this, Icon = this.Icon, ShowIcon = true, BackgroundResourceName = this.CurrentSettings.OverlayBackgroundResourceName, TabResourceName = this.CurrentSettings.OverlayTabResourceName };

            //Screen screen = this.GetCurrentScreen(this.overlay.Location);
            //Point screenLocation = screen != null ? screen.Bounds.Location : Screen.PrimaryScreen.Bounds.Location;
            //Size screenSize = screen != null ? screen.Bounds.Size : Screen.PrimaryScreen.Bounds.Size;
            //this.screenCenter = new Point(screenLocation.X + (screenSize.Width / 2), screenLocation.Y + (screenSize.Height / 2));

            this.logFile.OnParsedLogLines += this.LogFile_OnParsedLogLines;
            this.logFile.OnNewLogFileDate += this.LogFile_OnNewLogFileDate;
            this.logFile.OnError += this.LogFile_OnError;
            this.logFile.OnParsedLogLinesCurrent += this.LogFile_OnParsedLogLinesCurrent;
            this.logFile.StatsForm = this;
            this.logFile.SetAutoChangeProfile(this.CurrentSettings.AutoChangeProfile);

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

            this.UpdateGameExeLocation();
            this.ChangeLaunchPlatformImage(this.CurrentSettings.LaunchPlatform);

            this.RemoveUpdateFiles();
            this.ReloadProfileMenuItems();

            this.SortGridDetails(0, true);

            this.SuspendLayout();
            this.SetTheme(this.CurrentSettings.Theme == 0 ? MetroThemeStyle.Light : this.CurrentSettings.Theme == 1 ? MetroThemeStyle.Dark : MetroThemeStyle.Default);
            this.ResumeLayout(false);

            this.cmtt.OwnerDraw = true;
            this.cmtt.Draw += this.Cmtt_Draw;
            //this.mtt.OwnerDraw = true;
            //this.mtt.Draw += this.mtt_Draw;

            this.infoStrip.Renderer = new CustomToolStripSystemRenderer();
            this.infoStrip2.Renderer = new CustomToolStripSystemRenderer();
            DwmSetWindowAttribute(this.menu.Handle, DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE, ref windowConerPreference, sizeof(uint));
            DwmSetWindowAttribute(this.menuFilters.DropDown.Handle, DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE, ref windowConerPreference, sizeof(uint));
            DwmSetWindowAttribute(this.menuStatsFilter.DropDown.Handle, DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE, ref windowConerPreference, sizeof(uint));
            DwmSetWindowAttribute(this.menuPartyFilter.DropDown.Handle, DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE, ref windowConerPreference, sizeof(uint));
            DwmSetWindowAttribute(this.menuProfile.DropDown.Handle, DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE, ref windowConerPreference, sizeof(uint));
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

            this.Theme = theme;
            this.mtt.Theme = theme;
            this.menu.Renderer = this.Theme == MetroThemeStyle.Light ? (ToolStripRenderer)new CustomLightArrowRenderer() : new CustomDarkArrowRenderer();
            this.BackMaxSize = 56;
            this.BackImage = this.Icon.ToBitmap();
            foreach (Control c1 in Controls) {
                if (c1 is MenuStrip ms1) {
                    foreach (ToolStripMenuItem tsmi1 in ms1.Items) {
                        switch (tsmi1.Name) {
                            case "menuSettings":
                                tsmi1.Image = this.Theme == MetroThemeStyle.Light ? Properties.Resources.setting_icon : Properties.Resources.setting_gray_icon;
                                break;
                            case "menuFilters":
                                tsmi1.Image = this.Theme == MetroThemeStyle.Light ? Properties.Resources.filter_icon : Properties.Resources.filter_gray_icon;
                                break;
                            case "menuProfile":
                                tsmi1.Image = this.Theme == MetroThemeStyle.Light ? Properties.Resources.profile_icon : Properties.Resources.profile_gray_icon;
                                break;
                            //case "menuOverlay": break;
                            case "menuUpdate":
                            case "menuHelp":
                                tsmi1.Image = this.Theme == MetroThemeStyle.Light ? Properties.Resources.github_icon : Properties.Resources.github_gray_icon;
                                break;
                                //case "menuLaunchFallGuys": break;
                        }
                        tsmi1.ForeColor = this.Theme == MetroThemeStyle.Light ? Color.Black : Color.DarkGray;
                        tsmi1.MouseEnter += this.Menu_MouseEnter;
                        tsmi1.MouseLeave += this.Menu_MouseLeave;
                        foreach (ToolStripMenuItem tsmi2 in tsmi1.DropDownItems) {
                            if (tsmi2.Name.Equals("menuEditProfiles")) { tsmi2.Image = this.Theme == MetroThemeStyle.Light ? Properties.Resources.setting_icon : Properties.Resources.setting_gray_icon; }
                            tsmi2.ForeColor = this.Theme == MetroThemeStyle.Light ? Color.Black : Color.DarkGray;
                            tsmi2.BackColor = this.Theme == MetroThemeStyle.Light ? Color.White : Color.FromArgb(17, 17, 17);
                            tsmi2.MouseEnter += this.Menu_MouseEnter;
                            tsmi2.MouseLeave += this.Menu_MouseLeave;
                            foreach (ToolStripMenuItem tsmi3 in tsmi2.DropDownItems) {
                                if (tsmi3.Name.Equals("menuCustomRangeStats")) { tsmi3.Image = this.Theme == MetroThemeStyle.Light ? Properties.Resources.calendar_icon : Properties.Resources.calendar_gray_icon; }
                                tsmi3.ForeColor = this.Theme == MetroThemeStyle.Light ? Color.Black : Color.DarkGray;
                                tsmi3.BackColor = this.Theme == MetroThemeStyle.Light ? Color.White : Color.FromArgb(17, 17, 17);
                                tsmi3.MouseEnter += this.Menu_MouseEnter;
                                tsmi3.MouseLeave += this.Menu_MouseLeave;
                            }
                        }
                    }
                } else if (c1 is ToolStrip ts1) {
                    ts1.BackColor = Color.Transparent;
                    foreach (ToolStripLabel tsl1 in ts1.Items) {
                        switch (tsl1.Name) {
                            case "lblCurrentProfile":
                                tsl1.ForeColor = this.Theme == MetroThemeStyle.Light ? Color.Red : Color.FromArgb(0, 192, 192);
                                break;
                            case "lblTotalTime":
                                tsl1.Image = this.Theme == MetroThemeStyle.Light ? Properties.Resources.clock_icon : Properties.Resources.clock_gray_icon;
                                tsl1.ForeColor = this.Theme == MetroThemeStyle.Light ? Color.DarkSlateGray : Color.DarkGray;
                                break;
                            case "lblTotalShows":
                            case "lblTotalWins":
                                tsl1.ForeColor = this.Theme == MetroThemeStyle.Light ? Color.Blue : Color.Orange;
                                break;
                            case "lblTotalRounds":
                                tsl1.Image = this.Theme == MetroThemeStyle.Light ? Properties.Resources.round_icon : Properties.Resources.round_gray_icon;
                                tsl1.ForeColor = this.Theme == MetroThemeStyle.Light ? Color.Blue : Color.Orange;
                                break;
                            case "lblTotalFinals":
                                tsl1.Image = this.Theme == MetroThemeStyle.Light ? Properties.Resources.final_icon : Properties.Resources.final_gray_icon;
                                tsl1.ForeColor = this.Theme == MetroThemeStyle.Light ? Color.Blue : Color.Orange;
                                break;
                            case "lblGoldMedal":
                            case "lblSilverMedal":
                            case "lblBronzeMedal":
                            case "lblPinkMedal":
                            case "lblEliminatedMedal":
                            case "lblKudos":
                                tsl1.ForeColor = this.Theme == MetroThemeStyle.Light ? Color.DarkSlateGray : Color.DarkGray;
                                break;
                        }
                    }
                }
            }

            foreach (object item in this.gridDetails.CMenu.Items) {
                if (item is ToolStripMenuItem tsi) {
                    tsi.BackColor = this.Theme == MetroThemeStyle.Light ? Color.White : Color.FromArgb(17, 17, 17);
                    tsi.ForeColor = this.Theme == MetroThemeStyle.Light ? Color.Black : Color.DarkGray;
                    tsi.MouseEnter += this.CMenu_MouseEnter;
                    tsi.MouseLeave += this.CMenu_MouseLeave;
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

            this.dataGridViewCellStyle1.BackColor = this.Theme == MetroThemeStyle.Light ? Color.LightGray : Color.FromArgb(2, 2, 2);
            this.dataGridViewCellStyle1.ForeColor = this.Theme == MetroThemeStyle.Light ? Color.Black : Color.DarkGray;
            this.dataGridViewCellStyle1.SelectionBackColor = this.Theme == MetroThemeStyle.Light ? Color.Cyan : Color.DarkSlateBlue;
            //this.dataGridViewCellStyle1.SelectionForeColor = Color.Black;
            this.dataGridViewCellStyle2.BackColor = this.Theme == MetroThemeStyle.Light ? Color.White : Color.FromArgb(49, 51, 56);
            this.dataGridViewCellStyle2.ForeColor = this.Theme == MetroThemeStyle.Light ? Color.Black : Color.WhiteSmoke;
            this.dataGridViewCellStyle2.SelectionBackColor = this.Theme == MetroThemeStyle.Light ? Color.DeepSkyBlue : Color.PaleGreen;
            this.dataGridViewCellStyle2.SelectionForeColor = Color.Black;

            this.Refresh();
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
            this.AllProfiles = this.Profiles.FindAll().ToList();
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
                //((ToolStripDropDownMenu)menuProfile.DropDown).ShowCheckMargin = true;
                //((ToolStripDropDownMenu)menuProfile.DropDown).ShowImageMargin = true;
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
                e.Graphics.DrawImage(
                    this.CurrentSettings.AutoChangeProfile ? Properties.Resources.link_on_icon :
                    this.Theme == MetroThemeStyle.Light ? Properties.Resources.link_icon :
                    Properties.Resources.link_gray_icon, 20, 4, 13, 13);
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
                this.CurrentSettings.FastestFilter = 0;
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
                //this.CurrentSettings.PreventMouseCursorBugs = false;
                this.CurrentSettings.Version = 28;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.Version == 28) {
                this.CurrentSettings.EnableFallalyticsReporting = true;
                this.CurrentSettings.FallalyticsAPIKey = string.Empty;
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
                this.CurrentSettings.Version = 34;
                this.SaveUserSettings();
            }

            if (this.CurrentSettings.Version == 34) {
                this.AllStats.AddRange(this.RoundDetails.FindAll());
                this.StatsDB.BeginTrans();
                for (int i = this.AllStats.Count - 1; i >= 0; i--) {
                    RoundInfo info = this.AllStats[i];
                    if (info.UseShareCode && info.CreativeLastModifiedDate != DateTime.MinValue && string.IsNullOrEmpty(info.CreativeOnlinePlatformId)) {
                        info.CreativeOnlinePlatformId = "eos";
                        this.RoundDetails.Update(info);
                    }
                }
                this.CurrentSettings.Version = 35;
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
                SwitchBetweenPlayers = false,
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
                //PreventMouseCursorBugs = false,
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
                UpdatedDateFormat = true,
                WinPerDayGraphStyle = 1,
                Visible = true,
                Version = 35,
                FrenchyEditionDB = 8
            };
        }
        private void UpdateHoopsieLegends() {
            LevelStats level = this.StatLookup["round_hoops_blockade_solo"];
            string newName = this.CurrentSettings.HoopsieHeros ? Multilingual.GetWord("main_hoopsie_heroes") : Multilingual.GetWord("main_hoopsie_legends");
            if (level.Name != newName) {
                level.Name = newName;
            }
        }
        private void UpdateGridRoundName() {
            foreach (KeyValuePair<string, string> item in Multilingual.GetRoundsDictionary()) {
                LevelStats level = StatLookup[item.Key];
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
        private void SaveWindowState() {
            if (this.overlay.Visible) {
                if (!this.overlay.IsFixed()) {
                    this.CurrentSettings.OverlayLocationX = this.overlay.Location.X;
                    this.CurrentSettings.OverlayLocationY = this.overlay.Location.Y;
                    this.CurrentSettings.OverlayWidth = this.overlay.Width;
                    this.CurrentSettings.OverlayHeight = this.overlay.Height;
                }
            }

            if (this.WindowState != FormWindowState.Normal) {
                this.CurrentSettings.FormLocationX = RestoreBounds.Location.X;
                this.CurrentSettings.FormLocationY = RestoreBounds.Location.Y;
                this.CurrentSettings.FormWidth = RestoreBounds.Size.Width;
                this.CurrentSettings.FormHeight = RestoreBounds.Size.Height;
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
                if (this.CurrentSettings.AutoUpdate && this.CheckForUpdate(true)) {
                    return;
                }
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
                if (this.CurrentSettings.FormWidth.HasValue) {
                    this.Size = new Size(this.CurrentSettings.FormWidth.Value, this.CurrentSettings.FormHeight.Value);
                }
                if (this.CurrentSettings.FormLocationX.HasValue && IsOnScreen(this.CurrentSettings.FormLocationX.Value, this.CurrentSettings.FormLocationY.Value, this.Width)) {
                    this.Location = new Point(this.CurrentSettings.FormLocationX.Value, this.CurrentSettings.FormLocationY.Value);
                }

                string logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "Low", "Mediatonic", "FallGuys_client");
                if (!string.IsNullOrEmpty(this.CurrentSettings.LogPath)) {
                    logPath = this.CurrentSettings.LogPath;
                }
                this.logFile.Start(logPath, LOGNAME);

                this.overlay.ArrangeDisplay(this.CurrentSettings.FlippedDisplay, this.CurrentSettings.ShowOverlayTabs,
                    this.CurrentSettings.HideWinsInfo, this.CurrentSettings.HideRoundInfo, this.CurrentSettings.HideTimeInfo,
                    this.CurrentSettings.OverlayColor, this.CurrentSettings.OverlayWidth, this.CurrentSettings.OverlayHeight,
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
                }

                if (this.WindowState != FormWindowState.Minimized) {
                    this.WindowState = this.CurrentSettings.MaximizedWindowState ? FormWindowState.Maximized : FormWindowState.Normal;
                }
                if (this.CurrentSettings.StartMinimized || this.minimizeAfterGameLaunch) {
                    this.WindowState = FormWindowState.Minimized;
                }

                this.isStartUp = false;
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
                                                this.CurrentSettings.SelectedProfile = profile;
                                                //this.ReloadProfileMenuItems();
                                                this.SetProfileMenu(profile);
                                            }
                                        } else {
                                            this.askedPreviousShows = 2;
                                        }
                                        this.EnableInfoStrip(true);
                                        this.EnableMainMenu(true);
                                    }
                                }


                                if (stat.ShowEnd < this.startupTime && this.askedPreviousShows == 2) {
                                    continue;
                                }

                                if (stat.ShowEnd < this.startupTime && this.useLinkedProfiles) {
                                    profile = this.GetLinkedProfileId(stat.ShowNameId, stat.PrivateLobby, stat.ShowNameId.StartsWith("show_wle_s10"));
                                    this.CurrentSettings.SelectedProfile = profile;
                                    //this.ReloadProfileMenuItems();
                                    this.SetProfileMenu(profile);
                                }

                                if (stat.Round == 1) {
                                    this.nextShowID++;
                                    this.lastAddedShow = stat.Start;
                                }
                                stat.ShowID = nextShowID;
                                stat.Profile = profile;

                                if (stat.UseShareCode && !stat.Name.StartsWith("wle_s10_")) {
                                    try {
                                        JsonElement resData = this.GetApiData(this.FALLGUYSDB_API_URL, $"creative/{stat.ShowNameId}.json").GetProperty("data").GetProperty("snapshot");
                                        string[] onlinePlatformInfo = this.FindCreativeAuthor(resData.GetProperty("author").GetProperty("name_per_platform"));
                                        stat.CreativeShareCode = resData.GetProperty("share_code").GetString();
                                        stat.CreativeAuthor = onlinePlatformInfo[0];
                                        stat.CreativeOnlinePlatformId = onlinePlatformInfo[1];
                                        stat.CreativeVersion = resData.GetProperty("version_metadata").GetProperty("version").GetInt32();
                                        stat.CreativeStatus = resData.GetProperty("version_metadata").GetProperty("status").GetString();
                                        stat.CreativeTitle = this.textInfo.ToTitleCase(resData.GetProperty("version_metadata").GetProperty("title").GetString());
                                        stat.CreativeDescription = resData.GetProperty("version_metadata").GetProperty("description").GetString();
                                        stat.CreativeMaxPlayer = resData.GetProperty("version_metadata").GetProperty("max_player_count").GetInt32();
                                        stat.CreativePlatformId = resData.GetProperty("version_metadata").GetProperty("platform_id").GetString();
                                        stat.CreativeLastModifiedDate = resData.GetProperty("version_metadata").GetProperty("last_modified_date").GetDateTime();
                                        stat.CreativePlayCount = resData.GetProperty("play_count").GetInt32();
                                    } catch {
                                        // ignore
                                    }
                                }

                                this.RoundDetails.Insert(stat);
                                this.AllStats.Add(stat);

                                //Below is where reporting to fallaytics happen
                                //Must have enabled the setting to enable tracking
                                //Must not be a private lobby
                                //Must be a game that is played after FallGuysStats started
                                if (this.CurrentSettings.EnableFallalyticsReporting && !stat.PrivateLobby && stat.ShowEnd > this.startupTime) {
                                    FallalyticsReporter.Report(stat, this.CurrentSettings.FallalyticsAPIKey);
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
                            bool isCreative = false;
                            string roundName = stat.Name;
                            if (Regex.IsMatch(roundName, @"^\d{4}-\d{4}-\d{4}$")) {
                                isCreative = true;
                            } else if (roundName.StartsWith("round_", StringComparison.OrdinalIgnoreCase)) {
                                roundName = roundName.Substring(6).Replace('_', ' ');
                            } else {
                                roundName = roundName.Replace('_', ' ');
                            }

                            LevelStats newLevel = stat.UseShareCode || isCreative
                                                  ? new LevelStats(roundName, LevelType.Creative, true, false, 0, Properties.Resources.round_creative_icon)
                                                  : new LevelStats(this.textInfo.ToTitleCase(roundName), LevelType.Unknown, false, false, 0, null);

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

                        if (stat.Round == round.Count && !this.loadingExisting) {
                            if (this.menuSoloStats.Checked && stat.InParty) {
                                MenuStats_Click(this.menuSoloStats, null);
                            } else if (this.menuPartyStats.Checked && !stat.InParty) {
                                MenuStats_Click(this.menuPartyStats, null);
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
            return this.AllProfiles.Find(p => p.ProfileId == this.GetCurrentProfileId()).ProfileName;
        }
        public int GetCurrentProfileId() {
            return this.currentProfile;
        }
        private int GetCurrentProfileId(string profileName) {
            return this.AllProfiles.Find(p => p.ProfileName.Equals(profileName)).ProfileId;
        }
        private string GetCurrentProfileLinkedShowId() {
            string currentProfileLinkedShowId = this.AllProfiles.Find(p => p.ProfileId == this.GetCurrentProfileId()).LinkedShowId;
            return !string.IsNullOrEmpty(currentProfileLinkedShowId) ? currentProfileLinkedShowId : string.Empty;
        }
        private int GetLinkedProfileId(string showId, bool isPrivateLobbies, bool isCreativeShow) {
            if (string.IsNullOrEmpty(showId)) { return 0; }

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
            if (string.IsNullOrEmpty(showId) || this.GetCurrentProfileLinkedShowId().Equals(showId)) { return; }

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
            if (this.GetCurrentProfileId() == profile) return;
            this.MenuStats_Click(this.menuProfile.DropDownItems[this.AllProfiles.Find(p => p.ProfileId == profile).ProfileOrder], EventArgs.Empty);
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
        public StatSummary GetLevelInfo(string name, int levelException) {
            StatSummary summary = new StatSummary {
                AllWins = 0,
                TotalShows = 0,
                TotalPlays = 0,
                TotalWins = 0,
                TotalFinals = 0
            };

            int lastShow = -1;

            if (!this.StatLookup.TryGetValue(name, out LevelStats currentLevel)) {
                if (levelException >= 3) {
                    currentLevel = new LevelStats(name, LevelType.Creative, true, false, 0, Properties.Resources.round_creative_icon);
                } else {
                    currentLevel = new LevelStats(name, LevelType.Unknown, false, false, 0, null);
                }
            }

            for (int i = 0; i < this.AllStats.Count; i++) {
                RoundInfo info = this.AllStats[i];
                if (info.Profile != this.currentProfile) { continue; }

                TimeSpan finishTime = info.Finish.GetValueOrDefault(info.Start) - info.Start;
                bool hasFinishTime = finishTime.TotalSeconds > 1.1 ? true : false;
                bool hasLevelDetails = StatLookup.TryGetValue(info.Name, out LevelStats levelDetails);
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
                bool isInQualifyFilter = (!endRound.PrivateLobby || (endRound.UseShareCode && !endRound.Name.StartsWith("wle_s10_"))) &&
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
                        if ((!hasLevelDetails || levelDetails.Type == LevelType.Team || levelException == 2)
                            && info.Score.HasValue && (!summary.BestScore.HasValue || info.Score.Value > summary.BestScore.Value)) {
                            summary.BestScore = info.Score;
                        }
                    }
                }

                if (info == endRound && ((hasLevelDetails && levelDetails.IsFinal) || info.Crown) && !endRound.PrivateLobby) {
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
                float winChance = (float)this.Wins * 100 / (this.Shows == 0 ? 1 : this.Shows);
                this.lblTotalWins.Text = $"{this.Wins}{Multilingual.GetWord("main_win")} ({winChance:0.0} %)";
                this.lblTotalWins.ToolTipText = $"{Multilingual.GetWord("wins_detail_tooltiptext")}";
                float finalChance = (float)this.Finals * 100 / (this.Shows == 0 ? 1 : this.Shows);
                this.lblTotalFinals.Text = $"{this.Finals}{Multilingual.GetWord("main_inning")} ({finalChance:0.0} %)";
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
        private void GridDetails_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e) {
            try {
                if (e.RowIndex < 0) { return; }

                LevelStats levelStats = this.gridDetails.Rows[e.RowIndex].DataBoundItem as LevelStats;
                float fBrightness = 0.7F;
                switch (this.gridDetails.Columns[e.ColumnIndex].Name) {
                    case "RoundIcon":
                        if (levelStats.IsFinal) {
                            e.CellStyle.BackColor = this.Theme == MetroThemeStyle.Light
                                ? Color.FromArgb(255, 245, 205)
                                : Color.FromArgb((int)(255 * fBrightness), (int)(245 * fBrightness), (int)(205 * fBrightness));
                            break;
                        }
                        switch (levelStats.Type) {
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
                        if (levelStats.IsFinal) {
                            e.CellStyle.BackColor = this.Theme == MetroThemeStyle.Light
                                ? Color.FromArgb(255, 245, 205)
                                : Color.FromArgb((int)(255 * fBrightness), (int)(245 * fBrightness), (int)(205 * fBrightness));
                            break;
                        }
                        switch (levelStats.Type) {
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
                                e.Value = $"{qualifyChance:0.0}%";
                                this.gridDetails.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText = $"{levelStats.Qualified}";
                            } else {
                                e.Value = levelStats.Qualified;
                                this.gridDetails.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText = $"{qualifyChance:0.0}%";
                            }
                            break;
                        }
                    case "Gold": {
                            float qualifyChance = levelStats.Gold * 100f / (levelStats.Played == 0 ? 1 : levelStats.Played);
                            if (this.CurrentSettings.ShowPercentages) {
                                e.Value = $"{qualifyChance:0.0}%";
                                this.gridDetails.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText = $"{levelStats.Gold}";
                            } else {
                                e.Value = levelStats.Gold;
                                this.gridDetails.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText = $"{qualifyChance:0.0}%";
                            }
                            break;
                        }
                    case "Silver": {
                            float qualifyChance = levelStats.Silver * 100f / (levelStats.Played == 0 ? 1 : levelStats.Played);
                            if (this.CurrentSettings.ShowPercentages) {
                                e.Value = $"{qualifyChance:0.0}%";
                                this.gridDetails.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText = $"{levelStats.Silver}";
                            } else {
                                e.Value = levelStats.Silver;
                                this.gridDetails.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText = $"{qualifyChance:0.0}%";
                            }
                            break;
                        }
                    case "Bronze": {
                            float qualifyChance = levelStats.Bronze * 100f / (levelStats.Played == 0 ? 1 : levelStats.Played);
                            if (this.CurrentSettings.ShowPercentages) {
                                e.Value = $"{qualifyChance:0.0}%";
                                this.gridDetails.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText = $"{levelStats.Bronze}";
                            } else {
                                e.Value = levelStats.Bronze;
                                this.gridDetails.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText = $"{qualifyChance:0.0}%";
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
                        RoundIcon = stats.RoundIcon,
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

            this.StatDetails.Sort(delegate (LevelStats one, LevelStats two) {
                LevelType oneType = one.IsFinal ? LevelType.Final : one.Type;
                LevelType twoType = two.IsFinal ? LevelType.Final : two.Type;

                int typeCompare = this.CurrentSettings.IgnoreLevelTypeWhenSorting && sortOrder != SortOrder.None ? 0 : ((int)oneType).CompareTo((int)twoType);

                if (sortOrder == SortOrder.Descending) {
                    (one, two) = (two, one);
                }

                int nameCompare = one.Name.CompareTo(two.Name);
                bool percents = this.CurrentSettings.ShowPercentages;
                if (typeCompare == 0 && sortOrder != SortOrder.None) {
                    switch (columnName) {
                        case "Gold": typeCompare = ((double)one.Gold / (one.Played > 0 && percents ? one.Played : 1)).CompareTo((double)two.Gold / (two.Played > 0 && percents ? two.Played : 1)); break;
                        case "Silver": typeCompare = ((double)one.Silver / (one.Played > 0 && percents ? one.Played : 1)).CompareTo((double)two.Silver / (two.Played > 0 && percents ? two.Played : 1)); break;
                        case "Bronze": typeCompare = ((double)one.Bronze / (one.Played > 0 && percents ? one.Played : 1)).CompareTo((double)two.Bronze / (two.Played > 0 && percents ? two.Played : 1)); break;
                        case "Played": typeCompare = one.Played.CompareTo(two.Played); break;
                        case "Qualified": typeCompare = ((double)one.Qualified / (one.Played > 0 && percents ? one.Played : 1)).CompareTo((double)two.Qualified / (two.Played > 0 && percents ? two.Played : 1)); break;
                        case "Kudos": typeCompare = one.Kudos.CompareTo(two.Kudos); break;
                        case "AveKudos": typeCompare = one.AveKudos.CompareTo(two.AveKudos); break;
                        case "AveFinish": typeCompare = one.AveFinish.CompareTo(two.AveFinish); break;
                        case "Fastest": typeCompare = one.Fastest.CompareTo(two.Fastest); break;
                        case "Longest": typeCompare = one.Longest.CompareTo(two.Longest); break;
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
                this.EnableInfoStrip(false);
                this.EnableMainMenu(false);
                levelDetails.ShowDialog(this);
                this.EnableInfoStrip(true);
                this.EnableMainMenu(true);
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
                this.EnableInfoStrip(false);
                this.EnableMainMenu(false);
                levelDetails.ShowDialog(this);
                this.EnableInfoStrip(true);
                this.EnableMainMenu(true);
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
                this.EnableInfoStrip(false);
                this.EnableMainMenu(false);
                levelDetails.ShowDialog(this);
                this.EnableInfoStrip(true);
                this.EnableMainMenu(true);
            }
        }
        private void ShowWinGraph() {
            List<RoundInfo> rounds = new List<RoundInfo>();
            for (int i = 0; i < StatDetails.Count; i++) {
                rounds.AddRange(StatDetails[i].Stats);
            }
            rounds.Sort();

            using (StatsDisplay display = new StatsDisplay {
                StatsForm = this,
                Text = $@"     {Multilingual.GetWord("level_detail_wins_per_day")} - {this.GetCurrentProfileName()}",
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

                this.EnableInfoStrip(false);
                this.EnableMainMenu(false);
                display.ShowDialog(this);
                this.EnableInfoStrip(true);
                this.EnableMainMenu(true);
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
                if (CurrentSettings.LaunchPlatform == 0) {
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

                        if (MessageBox.Show(this, $"{Multilingual.GetWord("message_execution_question")}", Multilingual.GetWord("message_execution_caption"),
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
                    this.UpdateGameExeLocation();
                    if (!string.IsNullOrEmpty(CurrentSettings.GameExeLocation) && File.Exists(CurrentSettings.GameExeLocation)) {
                        Process[] processes = Process.GetProcesses();
                        string fallGuysProcessName = Path.GetFileNameWithoutExtension(CurrentSettings.GameExeLocation);
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

                        if (MessageBox.Show(this, $"{Multilingual.GetWord("message_execution_question")}", Multilingual.GetWord("message_execution_caption"),
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
            if (!string.IsNullOrEmpty(this.CurrentSettings.GameExeLocation) ||
                !string.IsNullOrEmpty(this.CurrentSettings.GameShortcutLocation)) { return; }

            string fallGuysExeLocation = this.FindGameExeLocation();
            if (!string.IsNullOrEmpty(fallGuysExeLocation)) {
                this.CurrentSettings.LaunchPlatform = 1;
                this.CurrentSettings.GameExeLocation = fallGuysExeLocation;
                this.SaveUserSettings();
            }
        }
        private string FindGameExeLocation() {
            try {
                // get steam install folder
                object regValue = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\Valve\\Steam", "InstallPath", null);
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
                    //this.ctrlKeyToggle = false;
                    break;
            }
        }
        private void Stats_KeyDown(object sender, KeyEventArgs e) {
            switch (e.KeyCode) {
                case Keys.ShiftKey:
                    this.shiftKeyToggle = true;
                    break;
                case Keys.ControlKey:
                    //this.ctrlKeyToggle = true;
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
        private void LblTotalFinals_Click(object sender, EventArgs e) {
            try {
                this.ShowFinals();
            } catch (Exception ex) {
                MessageBox.Show(this, ex.ToString(), $"{Multilingual.GetWord("message_program_error_caption")}",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void LblTotalShows_Click(object sender, EventArgs e) {
            try {
                this.ShowShows();
            } catch (Exception ex) {
                MessageBox.Show(this, ex.ToString(), $"{Multilingual.GetWord("message_program_error_caption")}",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void LblTotalRounds_Click(object sender, EventArgs e) {
            try {
                this.ShowRounds();
            } catch (Exception ex) {
                MessageBox.Show(this, ex.ToString(), $"{Multilingual.GetWord("message_program_error_caption")}",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void LblTotalWins_Click(object sender, EventArgs e) {
            try {
                this.ShowWinGraph();
            } catch (Exception ex) {
                MessageBox.Show(this, ex.ToString(), $"{Multilingual.GetWord("message_program_error_caption")}",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void MenuStats_Click(object sender, EventArgs e) {
            try {
                ToolStripMenuItem button = sender as ToolStripMenuItem;

                if (button == this.menuCustomRangeStats) {
                    if (this.isStartUp) {
                        this.updateFilterRange = true;
                    } else {
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

                    this.updateFilterType = true;
                    this.updateFilterRange = false;

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

                    this.currentProfile = this.GetCurrentProfileId(button.Text);
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

                if (!this.isStartUp && this.updateFilterType) {
                    this.updateFilterType = false;
                    this.CurrentSettings.FilterType = this.menuSeasonStats.Checked ? 2 :
                                                        this.menuWeekStats.Checked ? 3 :
                                                        this.menuDayStats.Checked ? 4 :
                                                        this.menuSessionStats.Checked ? 5 : 1;
                    this.SaveUserSettings();
                } else if (!this.isStartUp && this.updateFilterRange) {
                    this.updateFilterRange = false;
                    this.CurrentSettings.FilterType = 0;
                    this.CurrentSettings.SelectedCustomTemplateSeason = this.selectedCustomTemplateSeason;
                    this.CurrentSettings.CustomFilterRangeStart = this.customfilterRangeStart;
                    this.CurrentSettings.CustomFilterRangeEnd = this.customfilterRangeEnd;
                    this.SaveUserSettings();
                } else if (!this.isStartUp && this.updateSelectedProfile) {
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
            try {
#if AllowUpdate
                this.CheckForUpdate(false);
#else
                Process.Start(@"https://github.com/Micdu70/FallGuysStats#t%C3%A9l%C3%A9chargement");
#endif
            } catch (Exception ex) {
                MessageBox.Show(this, ex.ToString(), $"{Multilingual.GetWord("message_update_error_caption")}",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
        public string RenameCreativePlatformId(string platform) {
            switch (platform) {
                case "ps4": return Multilingual.GetWord("level_detail_playersPs4");
                case "ps5": return Multilingual.GetWord("level_detail_playersPs5");
                case "xb1": return Multilingual.GetWord("level_detail_playersXb1");
                case "xsx": return Multilingual.GetWord("level_detail_playersXsx");
                case "switch": return Multilingual.GetWord("level_detail_playersSw");
                case "win": return Multilingual.GetWord("level_detail_playersPc");
            }
            return platform;
        }
        public string[] FindCreativeAuthor(JsonElement authorData) {
            string[] validKeys = { "eos", "steam", "psn", "xbl", "nso" };
            string[] onlinePlatformInfo = { "N/A", "" };
            foreach (string validKey in validKeys) {
                if (authorData.TryGetProperty(validKey, out JsonElement authorInfo)) {
                    onlinePlatformInfo[0] = authorInfo.GetString();
                    onlinePlatformInfo[1] = validKey;
                    return onlinePlatformInfo;
                }
            }
            return onlinePlatformInfo;
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
                string assemblyInfo = web.DownloadString(@"https://raw.githubusercontent.com/Micdu70/FallGuysStats/master/Properties/AssemblyInfo.cs");
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
            byte[] data = web.DownloadData($"https://raw.githubusercontent.com/Micdu70/FallGuysStats/master/FallGuysStats.zip");
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
                    settings.StatsForm = this;
                    settings.Overlay = this.overlay;
                    string lastLogPath = this.CurrentSettings.LogPath;
                    this.EnableInfoStrip(false);
                    this.EnableMainMenu(false);
                    if (settings.ShowDialog(this) == DialogResult.OK) {
                        this.CurrentSettings = settings.CurrentSettings;
                        this.SetTheme(this.CurrentSettings.Theme == 0 ? MetroThemeStyle.Light :
                            this.CurrentSettings.Theme == 1 ? MetroThemeStyle.Dark : MetroThemeStyle.Default);
                        this.SaveUserSettings();
                        if (this.currentLanguage != CurrentLanguage) {
                            this.ChangeMainLanguage();
                            this.UpdateTotals();
                            this.gridDetails.ChangeContextMenuLanguage();
                            this.UpdateGridRoundName();
                            this.overlay.ChangeLanguage();
                        }
                        this.ChangeLaunchPlatformImage(this.CurrentSettings.LaunchPlatform);
                        this.UpdateHoopsieLegends();
                        this.overlay.Opacity = this.CurrentSettings.OverlayBackgroundOpacity / 100D;
                        this.overlay.SetBackgroundResourcesName(this.CurrentSettings.OverlayBackgroundResourceName, this.CurrentSettings.OverlayTabResourceName);
                        this.SetCurrentProfileIcon(this.AllProfiles.FindIndex(p => p.ProfileId == this.GetCurrentProfileId() && !string.IsNullOrEmpty(p.LinkedShowId)) != -1);
                        this.Refresh();
                        this.logFile.SetAutoChangeProfile(this.CurrentSettings.AutoChangeProfile);

                        if (string.IsNullOrEmpty(lastLogPath) != string.IsNullOrEmpty(this.CurrentSettings.LogPath) ||
                            (!string.IsNullOrEmpty(lastLogPath) && lastLogPath.Equals(this.CurrentSettings.LogPath, StringComparison.OrdinalIgnoreCase))) {
                            await this.logFile.Stop();

                            string logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "Low", "Mediatonic", "FallGuys_client");
                            if (!string.IsNullOrEmpty(this.CurrentSettings.LogPath)) {
                                logPath = this.CurrentSettings.LogPath;
                            }
                            this.logFile.Start(logPath, LOGNAME);
                        }

                        this.overlay.ArrangeDisplay(this.CurrentSettings.FlippedDisplay, this.CurrentSettings.ShowOverlayTabs,
                            this.CurrentSettings.HideWinsInfo, this.CurrentSettings.HideRoundInfo, this.CurrentSettings.HideTimeInfo,
                            this.CurrentSettings.OverlayColor, this.CurrentSettings.OverlayWidth, this.CurrentSettings.OverlayHeight,
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
                overlay.TopMost = !this.CurrentSettings.OverlayNotOnTop;
                overlay.Show();
                this.menuOverlay.Image = Properties.Resources.stat_icon;
                this.menuOverlay.Text = $"{Multilingual.GetWord("main_hide_overlay")}";
                this.CurrentSettings.OverlayVisible = true;
                this.SaveUserSettings();

                if (overlay.IsFixed()) {
                    if (this.CurrentSettings.OverlayFixedPositionX.HasValue &&
                        this.IsOnScreen(this.CurrentSettings.OverlayFixedPositionX.Value, this.CurrentSettings.OverlayFixedPositionY.Value, overlay.Width)) {
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
                    if (this.CurrentSettings.OverlayLocationX.HasValue && this.IsOnScreen(this.CurrentSettings.OverlayLocationX.Value, this.CurrentSettings.OverlayLocationY.Value, overlay.Width)) {
                        overlay.Location = new Point(this.CurrentSettings.OverlayLocationX.Value, this.CurrentSettings.OverlayLocationY.Value);
                    } else {
                        Screen screen = this.GetCurrentScreen(this.Location);
                        if (this.CurrentSettings.FlippedDisplay) {
                            overlay.Location = new Point(screen.WorkingArea.Right - overlay.Width - screen.WorkingArea.Right + overlay.Width, screen.WorkingArea.Top);
                        } else {
                            overlay.Location = new Point(screen.WorkingArea.Right - overlay.Width, screen.WorkingArea.Top);
                        }
                    }
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
                        this.AllStats = editProfiles.AllStats;
                        this.RoundDetails.DeleteAll();
                        this.RoundDetails.InsertBulk(this.AllStats);
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
        public bool IsOnScreen(int x, int y, int w) {
            Screen[] screens = Screen.AllScreens;
            foreach (Screen screen in screens) {
                if (screen.WorkingArea.Contains(new Point(x, y)) || screen.WorkingArea.Contains(new Point(x + w, y))) {
                    return true;
                }
            }
            return false;
        }
        public Screen GetCurrentScreen(Point location) {
            Screen[] scr = Screen.AllScreens;
            Screen screen = null;
            for (int i = 0; i < scr.Length; i++) {
                if (scr[i].WorkingArea.Contains(location)) {
                    screen = scr[i];
                    break;
                }
            }
            return screen;
        }
        private void ChangeLaunchPlatformImage(int launchPlatform) {
            this.menuLaunchFallGuys.Image = launchPlatform == 0
                                            ? Properties.Resources.epic_main_icon
                                            : Properties.Resources.steam_main_icon;
        }
        private void ChangeMainLanguage() {
            this.currentLanguage = CurrentLanguage;
            this.Text = $"{Multilingual.GetWord("main_fall_guys_stats")} v{Assembly.GetExecutingAssembly().GetName().Version.ToString(2)} {Multilingual.GetWord("main_title_suffix")}";

            int TextWidth = TextRenderer.MeasureText(this.Text, Overlay.GetDefaultFont(CurrentLanguage, 20)).Width;
#if AllowUpdate
            this.BackImagePadding = new Padding(TextWidth + (CurrentLanguage == 2 ? 100 : CurrentLanguage == 3 ? 40 : CurrentLanguage == 4 ? 14 : 8), 8, 0, 0);
#else
            this.BackImagePadding = new Padding(TextWidth + (CurrentLanguage == 2 ? 185 : CurrentLanguage == 3 ? 130 : CurrentLanguage == 4 ? 50 : 0), 8, 0, 0);
#endif
            this.menu.Font = Overlay.GetMainFont(12);
            this.menuLaunchFallGuys.Font = Overlay.GetMainFont(12);
            this.infoStrip.Font = Overlay.GetMainFont(13);
            this.infoStrip2.Font = Overlay.GetMainFont(13, FontStyle.Bold);

            this.dataGridViewCellStyle1.Font = Overlay.GetMainFont(10);
            this.dataGridViewCellStyle2.Font = Overlay.GetMainFont(12);
            this.SetMainDataGridView();

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
            this.menuUpdate.Text = Multilingual.GetWord("main_update");
            this.menuHelp.Text = Multilingual.GetWord("main_help");
            this.menuLaunchFallGuys.Text = Multilingual.GetWord("main_launch_fall_guys");
        }
    }
}