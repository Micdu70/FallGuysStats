﻿using System;
using System.Windows.Controls;
using System.Windows.Forms;

namespace FallGuysStats {
    partial class Stats {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Stats));
            this.menu = new System.Windows.Forms.MenuStrip();
            this.menuSettings = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFilters = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStatsFilter = new System.Windows.Forms.ToolStripMenuItem();
            this.menuCustomRangeStats = new System.Windows.Forms.ToolStripMenuItem();
            this.menuSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.menuAllStats = new System.Windows.Forms.ToolStripMenuItem();
            this.menuSeasonStats = new System.Windows.Forms.ToolStripMenuItem();
            this.menuWeekStats = new System.Windows.Forms.ToolStripMenuItem();
            this.menuDayStats = new System.Windows.Forms.ToolStripMenuItem();
            this.menuSessionStats = new System.Windows.Forms.ToolStripMenuItem();
            this.menuPartyFilter = new System.Windows.Forms.ToolStripMenuItem();
            this.menuAllPartyStats = new System.Windows.Forms.ToolStripMenuItem();
            this.menuSoloStats = new System.Windows.Forms.ToolStripMenuItem();
            this.menuPartyStats = new System.Windows.Forms.ToolStripMenuItem();
            this.menuProfile = new System.Windows.Forms.ToolStripMenuItem();
            this.menuEditProfiles = new System.Windows.Forms.ToolStripMenuItem();
            this.menuOverlay = new System.Windows.Forms.ToolStripMenuItem();
            this.menuUpdate = new System.Windows.Forms.ToolStripMenuItem();
            this.menuHelp = new System.Windows.Forms.ToolStripMenuItem();
            this.menuLaunchFallGuys = new System.Windows.Forms.ToolStripMenuItem();
            this.menuTodaysShow = new System.Windows.Forms.ToolStripMenuItem();
            this.infoStrip = new System.Windows.Forms.ToolStrip();
            this.lblCurrentProfile = new System.Windows.Forms.ToolStripLabel();
            this.lblTotalShows = new System.Windows.Forms.ToolStripLabel();
            this.lblTotalRounds = new System.Windows.Forms.ToolStripLabel();
            this.lblTotalFinals = new System.Windows.Forms.ToolStripLabel();
            this.lblTotalWins = new System.Windows.Forms.ToolStripLabel();
            this.infoStrip2 = new System.Windows.Forms.ToolStrip();
            this.lblTotalTime = new System.Windows.Forms.ToolStripLabel();
            this.lblGoldMedal = new System.Windows.Forms.ToolStripLabel();
            this.lblSilverMedal = new System.Windows.Forms.ToolStripLabel();
            this.lblBronzeMedal = new System.Windows.Forms.ToolStripLabel();
            this.lblPinkMedal = new System.Windows.Forms.ToolStripLabel();
            this.lblEliminatedMedal = new System.Windows.Forms.ToolStripLabel();
            this.lblKudos = new System.Windows.Forms.ToolStripLabel();
            this.lblManualUpdateVersion = new MetroFramework.Controls.MetroLabel();
            this.linkToIPinfoWebsite = new MetroFramework.Controls.MetroLabel();
            this.gridDetails = new FallGuysStats.Grid();
            this.menu.SuspendLayout();
            this.infoStrip.SuspendLayout();
            this.infoStrip2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridDetails)).BeginInit();
            this.SuspendLayout();
            // 
            // menu
            // 
            this.menu.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.menu.AutoSize = false;
            this.menu.BackColor = System.Drawing.Color.Transparent;
            this.menu.Dock = System.Windows.Forms.DockStyle.None;
            this.menu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuSettings,
            this.menuFilters,
            this.menuProfile,
            this.menuOverlay,
            this.menuUpdate,
            this.menuHelp,
            this.menuLaunchFallGuys,
            this.menuTodaysShow});
            this.menu.Location = new System.Drawing.Point(0, 65);
            this.menu.Name = "menu";
            this.menu.ShowItemToolTips = true;
            this.menu.Size = new System.Drawing.Size(880, 27);
            this.menu.TabIndex = 0;
            this.menu.TabStop = true;
            this.menu.Text = "menuStrip1";
            // 
            // menuSettings
            // 
            this.menuSettings.Image = global::FallGuysStats.Properties.Resources.setting_icon;
            this.menuSettings.Name = "menuSettings";
            this.menuSettings.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.menuSettings.Size = new System.Drawing.Size(77, 23);
            this.menuSettings.Text = "Settings";
            this.menuSettings.Click += new System.EventHandler(this.MenuSettings_Click);
            this.menuSettings.MouseLeave += new System.EventHandler(this.SetCursor_MouseLeave);
            this.menuSettings.MouseMove += new System.Windows.Forms.MouseEventHandler(this.SetCursor_MouseMove);
            // 
            // menuFilters
            // 
            this.menuFilters.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuStatsFilter,
            this.menuPartyFilter});
            this.menuFilters.Image = global::FallGuysStats.Properties.Resources.filter_icon;
            this.menuFilters.Name = "menuFilters";
            this.menuFilters.Size = new System.Drawing.Size(66, 23);
            this.menuFilters.Text = "Filters";
            // 
            // menuStatsFilter
            // 
            this.menuStatsFilter.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuCustomRangeStats,
            this.menuSeparator1,
            this.menuAllStats,
            this.menuSeasonStats,
            this.menuWeekStats,
            this.menuDayStats,
            this.menuSessionStats});
            this.menuStatsFilter.Image = global::FallGuysStats.Properties.Resources.stat_icon;
            this.menuStatsFilter.Name = "menuStatsFilter";
            this.menuStatsFilter.Size = new System.Drawing.Size(101, 22);
            this.menuStatsFilter.Text = "Stats";
            // 
            // menuCustomRangeStats
            // 
            this.menuCustomRangeStats.Image = global::FallGuysStats.Properties.Resources.calendar_icon;
            this.menuCustomRangeStats.Name = "menuCustomRangeStats";
            this.menuCustomRangeStats.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.Q)));
            this.menuCustomRangeStats.Size = new System.Drawing.Size(227, 22);
            this.menuCustomRangeStats.Text = "Custom Range";
            this.menuCustomRangeStats.Click += new System.EventHandler(this.MenuStats_Click);
            this.menuCustomRangeStats.MouseLeave += new System.EventHandler(this.SetCursor_MouseLeave);
            this.menuCustomRangeStats.MouseMove += new System.Windows.Forms.MouseEventHandler(this.SetCursor_MouseMove);
            // 
            // menuSeparator1
            // 
            this.menuSeparator1.Name = "menuSeparator1";
            this.menuSeparator1.Size = new System.Drawing.Size(226, 6);
            // 
            // menuAllStats
            // 
            this.menuAllStats.Checked = true;
            this.menuAllStats.CheckOnClick = true;
            this.menuAllStats.CheckState = System.Windows.Forms.CheckState.Checked;
            this.menuAllStats.Name = "menuAllStats";
            this.menuAllStats.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.A)));
            this.menuAllStats.Size = new System.Drawing.Size(227, 22);
            this.menuAllStats.Text = "All";
            this.menuAllStats.Click += new System.EventHandler(this.MenuStats_Click);
            this.menuAllStats.MouseLeave += new System.EventHandler(this.SetCursor_MouseLeave);
            this.menuAllStats.MouseMove += new System.Windows.Forms.MouseEventHandler(this.SetCursor_MouseMove);
            // 
            // menuSeasonStats
            // 
            this.menuSeasonStats.CheckOnClick = true;
            this.menuSeasonStats.Name = "menuSeasonStats";
            this.menuSeasonStats.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.S)));
            this.menuSeasonStats.Size = new System.Drawing.Size(227, 22);
            this.menuSeasonStats.Text = "Season";
            this.menuSeasonStats.Click += new System.EventHandler(this.MenuStats_Click);
            this.menuSeasonStats.MouseLeave += new System.EventHandler(this.SetCursor_MouseLeave);
            this.menuSeasonStats.MouseMove += new System.Windows.Forms.MouseEventHandler(this.SetCursor_MouseMove);
            // 
            // menuWeekStats
            // 
            this.menuWeekStats.CheckOnClick = true;
            this.menuWeekStats.Name = "menuWeekStats";
            this.menuWeekStats.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.W)));
            this.menuWeekStats.Size = new System.Drawing.Size(227, 22);
            this.menuWeekStats.Text = "Week";
            this.menuWeekStats.Click += new System.EventHandler(this.MenuStats_Click);
            this.menuWeekStats.MouseLeave += new System.EventHandler(this.SetCursor_MouseLeave);
            this.menuWeekStats.MouseMove += new System.Windows.Forms.MouseEventHandler(this.SetCursor_MouseMove);
            // 
            // menuDayStats
            // 
            this.menuDayStats.CheckOnClick = true;
            this.menuDayStats.Name = "menuDayStats";
            this.menuDayStats.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.D)));
            this.menuDayStats.Size = new System.Drawing.Size(227, 22);
            this.menuDayStats.Text = "Day";
            this.menuDayStats.Click += new System.EventHandler(this.MenuStats_Click);
            this.menuDayStats.MouseLeave += new System.EventHandler(this.SetCursor_MouseLeave);
            this.menuDayStats.MouseMove += new System.Windows.Forms.MouseEventHandler(this.SetCursor_MouseMove);
            // 
            // menuSessionStats
            // 
            this.menuSessionStats.CheckOnClick = true;
            this.menuSessionStats.Name = "menuSessionStats";
            this.menuSessionStats.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.G)));
            this.menuSessionStats.Size = new System.Drawing.Size(227, 22);
            this.menuSessionStats.Text = "Session";
            this.menuSessionStats.Click += new System.EventHandler(this.MenuStats_Click);
            this.menuSessionStats.MouseLeave += new System.EventHandler(this.SetCursor_MouseLeave);
            this.menuSessionStats.MouseMove += new System.Windows.Forms.MouseEventHandler(this.SetCursor_MouseMove);
            // 
            // menuPartyFilter
            // 
            this.menuPartyFilter.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuAllPartyStats,
            this.menuSoloStats,
            this.menuPartyStats});
            this.menuPartyFilter.Image = global::FallGuysStats.Properties.Resources.player_icon;
            this.menuPartyFilter.Name = "menuPartyFilter";
            this.menuPartyFilter.Size = new System.Drawing.Size(101, 22);
            this.menuPartyFilter.Text = "Party";
            // 
            // menuAllPartyStats
            // 
            this.menuAllPartyStats.Checked = true;
            this.menuAllPartyStats.CheckOnClick = true;
            this.menuAllPartyStats.CheckState = System.Windows.Forms.CheckState.Checked;
            this.menuAllPartyStats.Name = "menuAllPartyStats";
            this.menuAllPartyStats.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.F)));
            this.menuAllPartyStats.Size = new System.Drawing.Size(174, 22);
            this.menuAllPartyStats.Text = "All";
            this.menuAllPartyStats.Click += new System.EventHandler(this.MenuStats_Click);
            this.menuAllPartyStats.MouseLeave += new System.EventHandler(this.SetCursor_MouseLeave);
            this.menuAllPartyStats.MouseMove += new System.Windows.Forms.MouseEventHandler(this.SetCursor_MouseMove);
            // 
            // menuSoloStats
            // 
            this.menuSoloStats.CheckOnClick = true;
            this.menuSoloStats.Name = "menuSoloStats";
            this.menuSoloStats.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.O)));
            this.menuSoloStats.Size = new System.Drawing.Size(174, 22);
            this.menuSoloStats.Text = "Solo";
            this.menuSoloStats.Click += new System.EventHandler(this.MenuStats_Click);
            this.menuSoloStats.MouseLeave += new System.EventHandler(this.SetCursor_MouseLeave);
            this.menuSoloStats.MouseMove += new System.Windows.Forms.MouseEventHandler(this.SetCursor_MouseMove);
            // 
            // menuPartyStats
            // 
            this.menuPartyStats.CheckOnClick = true;
            this.menuPartyStats.Name = "menuPartyStats";
            this.menuPartyStats.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.P)));
            this.menuPartyStats.Size = new System.Drawing.Size(174, 22);
            this.menuPartyStats.Text = "Party";
            this.menuPartyStats.Click += new System.EventHandler(this.MenuStats_Click);
            this.menuPartyStats.MouseLeave += new System.EventHandler(this.SetCursor_MouseLeave);
            this.menuPartyStats.MouseMove += new System.Windows.Forms.MouseEventHandler(this.SetCursor_MouseMove);
            // 
            // menuProfile
            // 
            this.menuProfile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuEditProfiles});
            this.menuProfile.Image = global::FallGuysStats.Properties.Resources.profile_icon;
            this.menuProfile.Name = "menuProfile";
            this.menuProfile.Size = new System.Drawing.Size(69, 23);
            this.menuProfile.Text = "Profile";
            // 
            // menuEditProfiles
            // 
            this.menuEditProfiles.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.menuEditProfiles.Image = global::FallGuysStats.Properties.Resources.setting_icon;
            this.menuEditProfiles.Name = "menuEditProfiles";
            this.menuEditProfiles.Size = new System.Drawing.Size(153, 22);
            this.menuEditProfiles.Text = "Profile Settings";
            this.menuEditProfiles.Click += new System.EventHandler(this.MenuEditProfiles_Click);
            this.menuEditProfiles.MouseLeave += new System.EventHandler(this.SetCursor_MouseLeave);
            this.menuEditProfiles.MouseMove += new System.Windows.Forms.MouseEventHandler(this.SetCursor_MouseMove);
            // 
            // menuOverlay
            // 
            this.menuOverlay.Image = global::FallGuysStats.Properties.Resources.stat_gray_icon;
            this.menuOverlay.Name = "menuOverlay";
            this.menuOverlay.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.menuOverlay.Size = new System.Drawing.Size(107, 23);
            this.menuOverlay.Text = "Show Overlay";
            this.menuOverlay.Click += new System.EventHandler(this.MenuOverlay_Click);
            this.menuOverlay.MouseEnter += new System.EventHandler(this.MenuOverlay_MouseEnter);
            this.menuOverlay.MouseLeave += new System.EventHandler(this.MenuOverlay_MouseLeave);
            this.menuOverlay.MouseMove += new System.Windows.Forms.MouseEventHandler(this.SetCursor_MouseMove);
            // 
            // menuUpdate
            // 
            this.menuUpdate.Image = global::FallGuysStats.Properties.Resources.github_icon;
            this.menuUpdate.Name = "menuUpdate";
            this.menuUpdate.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.U)));
            this.menuUpdate.Size = new System.Drawing.Size(73, 23);
            this.menuUpdate.Text = "Update";
            this.menuUpdate.Click += new System.EventHandler(this.MenuUpdate_Click);
            this.menuUpdate.MouseLeave += new System.EventHandler(this.SetCursor_MouseLeave);
            this.menuUpdate.MouseMove += new System.Windows.Forms.MouseEventHandler(this.SetCursor_MouseMove);
            // 
            // menuHelp
            // 
            this.menuHelp.Image = global::FallGuysStats.Properties.Resources.github_icon;
            this.menuHelp.Name = "menuHelp";
            this.menuHelp.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.H)));
            this.menuHelp.Size = new System.Drawing.Size(60, 23);
            this.menuHelp.Text = "Help";
            this.menuHelp.Click += new System.EventHandler(this.MenuHelp_Click);
            this.menuHelp.MouseLeave += new System.EventHandler(this.SetCursor_MouseLeave);
            this.menuHelp.MouseMove += new System.Windows.Forms.MouseEventHandler(this.SetCursor_MouseMove);
            // 
            // menuLaunchFallGuys
            // 
            this.menuLaunchFallGuys.Image = global::FallGuysStats.Properties.Resources.fallguys_icon;
            this.menuLaunchFallGuys.Name = "menuLaunchFallGuys";
            this.menuLaunchFallGuys.Size = new System.Drawing.Size(124, 23);
            this.menuLaunchFallGuys.Text = "Launch Fall Guys";
            this.menuLaunchFallGuys.Click += new System.EventHandler(this.MenuLaunchFallGuys_Click);
            this.menuLaunchFallGuys.MouseLeave += new System.EventHandler(this.SetCursor_MouseLeave);
            this.menuLaunchFallGuys.MouseMove += new System.Windows.Forms.MouseEventHandler(this.SetCursor_MouseMove);
            // 
            // menuTodaysShow
            // 
            this.menuTodaysShow.Image = global::FallGuysStats.Properties.Resources.fallguys_db_logo;
            this.menuTodaysShow.Name = "menuTodaysShow";
            this.menuTodaysShow.Size = new System.Drawing.Size(109, 23);
            this.menuTodaysShow.Text = "Today\'s Show!";
            this.menuTodaysShow.ToolTipText = "See what shows are currently available";
            this.menuTodaysShow.Click += new System.EventHandler(this.MenuTodaysShow_Click);
            this.menuTodaysShow.MouseEnter += new System.EventHandler(this.MenuTodaysShow_MouseEnter);
            this.menuTodaysShow.MouseLeave += new System.EventHandler(this.MenuTodaysShow_MouseLeave);
            // 
            // infoStrip
            // 
            this.infoStrip.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.infoStrip.AutoSize = false;
            this.infoStrip.BackColor = System.Drawing.Color.Transparent;
            this.infoStrip.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.infoStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.infoStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.infoStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblCurrentProfile,
            this.lblTotalShows,
            this.lblTotalRounds,
            this.lblTotalFinals,
            this.lblTotalWins});
            this.infoStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.infoStrip.Location = new System.Drawing.Point(0, 93);
            this.infoStrip.Name = "infoStrip";
            this.infoStrip.Padding = new System.Windows.Forms.Padding(20, 6, 20, 1);
            this.infoStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.infoStrip.Size = new System.Drawing.Size(880, 26);
            this.infoStrip.Stretch = true;
            this.infoStrip.TabIndex = 1;
            // 
            // lblCurrentProfile
            // 
            this.lblCurrentProfile.ForeColor = System.Drawing.Color.Crimson;
            this.lblCurrentProfile.Image = global::FallGuysStats.Properties.Resources.profile2_icon;
            this.lblCurrentProfile.Margin = new System.Windows.Forms.Padding(4, 1, 20, 2);
            this.lblCurrentProfile.Name = "lblCurrentProfile";
            this.lblCurrentProfile.Size = new System.Drawing.Size(46, 16);
            this.lblCurrentProfile.Text = "Solo";
            this.lblCurrentProfile.ToolTipText = "Click to change your current profile";
            this.lblCurrentProfile.MouseDown += new System.Windows.Forms.MouseEventHandler(this.LblCurrentProfile_MouseDown);
            this.lblCurrentProfile.MouseEnter += new System.EventHandler(this.InfoStrip_MouseEnter);
            this.lblCurrentProfile.MouseLeave += new System.EventHandler(this.InfoStrip_MouseLeave);
            // 
            // lblTotalShows
            // 
            this.lblTotalShows.ForeColor = System.Drawing.Color.Blue;
            this.lblTotalShows.Image = global::FallGuysStats.Properties.Resources.show_icon;
            this.lblTotalShows.Margin = new System.Windows.Forms.Padding(10, 1, 5, 2);
            this.lblTotalShows.Name = "lblTotalShows";
            this.lblTotalShows.Size = new System.Drawing.Size(29, 16);
            this.lblTotalShows.Text = "0";
            this.lblTotalShows.ToolTipText = "Click to view shows stats";
            this.lblTotalShows.Click += new System.EventHandler(this.LblTotalShows_Click);
            this.lblTotalShows.MouseEnter += new System.EventHandler(this.InfoStrip_MouseEnter);
            this.lblTotalShows.MouseLeave += new System.EventHandler(this.InfoStrip_MouseLeave);
            // 
            // lblTotalRounds
            // 
            this.lblTotalRounds.ForeColor = System.Drawing.Color.Blue;
            this.lblTotalRounds.Image = global::FallGuysStats.Properties.Resources.round_icon;
            this.lblTotalRounds.Margin = new System.Windows.Forms.Padding(10, 1, 5, 2);
            this.lblTotalRounds.Name = "lblTotalRounds";
            this.lblTotalRounds.Size = new System.Drawing.Size(29, 16);
            this.lblTotalRounds.Text = "0";
            this.lblTotalRounds.ToolTipText = "Click to view rounds stats";
            this.lblTotalRounds.Click += new System.EventHandler(this.LblTotalRounds_Click);
            this.lblTotalRounds.MouseEnter += new System.EventHandler(this.InfoStrip_MouseEnter);
            this.lblTotalRounds.MouseLeave += new System.EventHandler(this.InfoStrip_MouseLeave);
            // 
            // lblTotalFinals
            // 
            this.lblTotalFinals.ForeColor = System.Drawing.Color.Blue;
            this.lblTotalFinals.Image = global::FallGuysStats.Properties.Resources.final_icon;
            this.lblTotalFinals.Margin = new System.Windows.Forms.Padding(10, 1, 5, 2);
            this.lblTotalFinals.Name = "lblTotalFinals";
            this.lblTotalFinals.Size = new System.Drawing.Size(65, 16);
            this.lblTotalFinals.Text = "0 (0.0%)";
            this.lblTotalFinals.ToolTipText = "Click to view finals stats";
            this.lblTotalFinals.Click += new System.EventHandler(this.LblTotalFinals_Click);
            this.lblTotalFinals.MouseEnter += new System.EventHandler(this.InfoStrip_MouseEnter);
            this.lblTotalFinals.MouseLeave += new System.EventHandler(this.InfoStrip_MouseLeave);
            // 
            // lblTotalWins
            // 
            this.lblTotalWins.ForeColor = System.Drawing.Color.Blue;
            this.lblTotalWins.Image = global::FallGuysStats.Properties.Resources.crown_icon;
            this.lblTotalWins.Margin = new System.Windows.Forms.Padding(10, 1, 0, 2);
            this.lblTotalWins.Name = "lblTotalWins";
            this.lblTotalWins.Size = new System.Drawing.Size(65, 16);
            this.lblTotalWins.Text = "0 (0.0%)";
            this.lblTotalWins.ToolTipText = "Click to view wins stats";
            this.lblTotalWins.Click += new System.EventHandler(this.LblTotalWins_Click);
            this.lblTotalWins.MouseEnter += new System.EventHandler(this.InfoStrip_MouseEnter);
            this.lblTotalWins.MouseLeave += new System.EventHandler(this.InfoStrip_MouseLeave);
            // 
            // infoStrip2
            // 
            this.infoStrip2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.infoStrip2.AutoSize = false;
            this.infoStrip2.BackColor = System.Drawing.Color.Transparent;
            this.infoStrip2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.infoStrip2.Dock = System.Windows.Forms.DockStyle.None;
            this.infoStrip2.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.infoStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblTotalTime,
            this.lblGoldMedal,
            this.lblSilverMedal,
            this.lblBronzeMedal,
            this.lblPinkMedal,
            this.lblEliminatedMedal,
            this.lblKudos});
            this.infoStrip2.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.infoStrip2.Location = new System.Drawing.Point(0, 120);
            this.infoStrip2.Name = "infoStrip2";
            this.infoStrip2.Padding = new System.Windows.Forms.Padding(14, 6, 14, 1);
            this.infoStrip2.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.infoStrip2.Size = new System.Drawing.Size(880, 27);
            this.infoStrip2.TabIndex = 2;
            // 
            // lblTotalTime
            // 
            this.lblTotalTime.ForeColor = System.Drawing.Color.Blue;
            this.lblTotalTime.Image = global::FallGuysStats.Properties.Resources.clock_icon;
            this.lblTotalTime.Margin = new System.Windows.Forms.Padding(10, 1, 20, 2);
            this.lblTotalTime.Name = "lblTotalTime";
            this.lblTotalTime.Size = new System.Drawing.Size(59, 16);
            this.lblTotalTime.Text = "0:00:00";
            this.lblTotalTime.ToolTipText = "Click to view statistics graph by round";
            this.lblTotalTime.Click += new System.EventHandler(this.LblTotalTime_Click);
            this.lblTotalTime.MouseEnter += new System.EventHandler(this.InfoStrip_MouseEnter);
            this.lblTotalTime.MouseLeave += new System.EventHandler(this.InfoStrip_MouseLeave);
            // 
            // lblGoldMedal
            // 
            this.lblGoldMedal.ForeColor = System.Drawing.Color.DarkSlateGray;
            this.lblGoldMedal.Image = global::FallGuysStats.Properties.Resources.medal_gold;
            this.lblGoldMedal.Margin = new System.Windows.Forms.Padding(10, 1, 5, 2);
            this.lblGoldMedal.Name = "lblGoldMedal";
            this.lblGoldMedal.Size = new System.Drawing.Size(29, 16);
            this.lblGoldMedal.Text = "0";
            // 
            // lblSilverMedal
            // 
            this.lblSilverMedal.ForeColor = System.Drawing.Color.DarkSlateGray;
            this.lblSilverMedal.Image = global::FallGuysStats.Properties.Resources.medal_silver;
            this.lblSilverMedal.Margin = new System.Windows.Forms.Padding(10, 1, 5, 2);
            this.lblSilverMedal.Name = "lblSilverMedal";
            this.lblSilverMedal.Size = new System.Drawing.Size(29, 16);
            this.lblSilverMedal.Text = "0";
            // 
            // lblBronzeMedal
            // 
            this.lblBronzeMedal.ForeColor = System.Drawing.Color.DarkSlateGray;
            this.lblBronzeMedal.Image = global::FallGuysStats.Properties.Resources.medal_bronze;
            this.lblBronzeMedal.Margin = new System.Windows.Forms.Padding(10, 1, 5, 2);
            this.lblBronzeMedal.Name = "lblBronzeMedal";
            this.lblBronzeMedal.Size = new System.Drawing.Size(29, 16);
            this.lblBronzeMedal.Text = "0";
            // 
            // lblPinkMedal
            // 
            this.lblPinkMedal.ForeColor = System.Drawing.Color.DarkSlateGray;
            this.lblPinkMedal.Image = global::FallGuysStats.Properties.Resources.medal_pink;
            this.lblPinkMedal.Margin = new System.Windows.Forms.Padding(10, 1, 5, 2);
            this.lblPinkMedal.Name = "lblPinkMedal";
            this.lblPinkMedal.Size = new System.Drawing.Size(29, 16);
            this.lblPinkMedal.Text = "0";
            // 
            // lblEliminatedMedal
            // 
            this.lblEliminatedMedal.ForeColor = System.Drawing.Color.DarkSlateGray;
            this.lblEliminatedMedal.Image = global::FallGuysStats.Properties.Resources.medal_eliminated;
            this.lblEliminatedMedal.Margin = new System.Windows.Forms.Padding(10, 1, 5, 2);
            this.lblEliminatedMedal.Name = "lblEliminatedMedal";
            this.lblEliminatedMedal.Size = new System.Drawing.Size(29, 16);
            this.lblEliminatedMedal.Text = "0";
            // 
            // lblKudos
            // 
            this.lblKudos.ForeColor = System.Drawing.Color.DarkSlateGray;
            this.lblKudos.Image = global::FallGuysStats.Properties.Resources.kudos_icon;
            this.lblKudos.Margin = new System.Windows.Forms.Padding(10, 1, 0, 2);
            this.lblKudos.Name = "lblKudos";
            this.lblKudos.Size = new System.Drawing.Size(29, 16);
            this.lblKudos.Text = "0";
            // 
            // lblManualUpdateVersion
            // 
            this.lblManualUpdateVersion.AutoSize = true;
            this.lblManualUpdateVersion.FontSize = MetroFramework.MetroLabelSize.Small;
            this.lblManualUpdateVersion.FontWeight = MetroFramework.MetroLabelWeight.Regular;
            this.lblManualUpdateVersion.Location = new System.Drawing.Point(26, 50);
            this.lblManualUpdateVersion.Name = "lblManualUpdateVersion";
            this.lblManualUpdateVersion.Size = new System.Drawing.Size(129, 15);
            this.lblManualUpdateVersion.TabIndex = 0;
            this.lblManualUpdateVersion.Text = "Manual Update Version";
            // 
            // linkToIPinfoWebsite
            // 
            this.linkToIPinfoWebsite.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.linkToIPinfoWebsite.AutoSize = true;
            this.linkToIPinfoWebsite.Cursor = System.Windows.Forms.Cursors.Hand;
            this.linkToIPinfoWebsite.FontSize = MetroFramework.MetroLabelSize.Small;
            this.linkToIPinfoWebsite.FontWeight = MetroFramework.MetroLabelWeight.Bold;
            this.linkToIPinfoWebsite.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.linkToIPinfoWebsite.Location = new System.Drawing.Point(585, 9);
            this.linkToIPinfoWebsite.Name = "linkToIPinfoWebsite";
            this.linkToIPinfoWebsite.Size = new System.Drawing.Size(195, 15);
            this.linkToIPinfoWebsite.TabIndex = 4;
            this.linkToIPinfoWebsite.Text = "IP address data powered by IPinfo";
            this.linkToIPinfoWebsite.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.linkToIPinfoWebsite.Click += new System.EventHandler(this.LinkToIPinfoWebsite_Click);
            // 
            // gridDetails
            // 
            this.gridDetails.AllowUserToDeleteRows = false;
            this.gridDetails.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridDetails.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.gridDetails.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.LightGray;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.Cyan;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.gridDetails.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.gridDetails.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.gridDetails.EnableHeadersVisualStyles = false;
            this.gridDetails.GridColor = System.Drawing.Color.Gray;
            this.gridDetails.Location = new System.Drawing.Point(20, 158);
            this.gridDetails.Margin = new System.Windows.Forms.Padding(0);
            this.gridDetails.Name = "gridDetails";
            this.gridDetails.ReadOnly = true;
            this.gridDetails.RowHeadersVisible = false;
            this.gridDetails.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.gridDetails.Size = new System.Drawing.Size(860, 320);
            this.gridDetails.TabIndex = 3;
            this.gridDetails.TabStop = false;
            this.gridDetails.DataSourceChanged += new System.EventHandler(this.GridDetails_DataSourceChanged);
            this.gridDetails.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.GridDetails_CellClick);
            this.gridDetails.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.GridDetails_CellFormatting);
            this.gridDetails.CellMouseEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.GridDetails_CellMouseEnter);
            this.gridDetails.CellMouseLeave += new System.Windows.Forms.DataGridViewCellEventHandler(this.GridDetails_CellMouseLeave);
            this.gridDetails.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.GridDetails_ColumnHeaderMouseClick);
            this.gridDetails.SelectionChanged += new System.EventHandler(this.GridDetails_SelectionChanged);
            // 
            // Stats
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(900, 500);
            this.Controls.Add(this.linkToIPinfoWebsite);
            this.Controls.Add(this.lblManualUpdateVersion);
            this.Controls.Add(this.infoStrip);
            this.Controls.Add(this.infoStrip2);
            this.Controls.Add(this.gridDetails);
            this.Controls.Add(this.menu);
            this.ForeColor = System.Drawing.Color.Black;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Location = new System.Drawing.Point(15, 15);
            this.MinimumSize = new System.Drawing.Size(900, 350);
            this.Name = "Stats";
            this.ShadowType = MetroFramework.Forms.MetroFormShadowType.AeroShadow;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Style = MetroFramework.MetroColorStyle.Teal;
            this.Text = "Fall Guys Stats";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Stats_FormClosing);
            this.Load += new System.EventHandler(this.Stats_Load);
            this.Shown += new System.EventHandler(this.Stats_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Stats_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Stats_KeyUp);
            this.menu.ResumeLayout(false);
            this.menu.PerformLayout();
            this.infoStrip.ResumeLayout(false);
            this.infoStrip.PerformLayout();
            this.infoStrip2.ResumeLayout(false);
            this.infoStrip2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridDetails)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        
        #endregion

        private FallGuysStats.Grid gridDetails;
        private System.Windows.Forms.MenuStrip menu;
        private System.Windows.Forms.ToolStripMenuItem menuSettings;
        private System.Windows.Forms.ToolStripMenuItem menuFilters;
        private System.Windows.Forms.ToolStripMenuItem menuStatsFilter;
        private System.Windows.Forms.ToolStripMenuItem menuAllStats;
        private System.Windows.Forms.ToolStripMenuItem menuSeasonStats;
        private System.Windows.Forms.ToolStripMenuItem menuWeekStats;
        private System.Windows.Forms.ToolStripMenuItem menuDayStats;
        private System.Windows.Forms.ToolStripMenuItem menuSessionStats;
        private System.Windows.Forms.ToolStripMenuItem menuPartyFilter;
        private System.Windows.Forms.ToolStripMenuItem menuAllPartyStats;
        private System.Windows.Forms.ToolStripMenuItem menuSoloStats;
        private System.Windows.Forms.ToolStripMenuItem menuPartyStats;
        private System.Windows.Forms.ToolStripMenuItem menuOverlay;
        private System.Windows.Forms.ToolStripMenuItem menuUpdate;
        private System.Windows.Forms.ToolStripMenuItem menuHelp;
        private System.Windows.Forms.ToolStripMenuItem menuProfile;
        private System.Windows.Forms.ToolStrip infoStrip;
        private System.Windows.Forms.ToolStrip infoStrip2;
        private System.Windows.Forms.ToolStripLabel lblCurrentProfile;
        private System.Windows.Forms.ToolStripLabel lblTotalTime;
        private System.Windows.Forms.ToolStripLabel lblTotalShows;
        private System.Windows.Forms.ToolStripLabel lblTotalRounds;
        private System.Windows.Forms.ToolStripLabel lblTotalWins;
        private System.Windows.Forms.ToolStripLabel lblTotalFinals;
        private System.Windows.Forms.ToolStripLabel lblGoldMedal;
        private System.Windows.Forms.ToolStripLabel lblSilverMedal;
        private System.Windows.Forms.ToolStripLabel lblBronzeMedal;
        private System.Windows.Forms.ToolStripLabel lblPinkMedal;
        private System.Windows.Forms.ToolStripLabel lblEliminatedMedal;
        private System.Windows.Forms.ToolStripLabel lblKudos;
        private System.Windows.Forms.ToolStripMenuItem menuLaunchFallGuys;
        private System.Windows.Forms.ToolStripMenuItem menuTodaysShow;
        private System.Windows.Forms.ToolStripMenuItem menuEditProfiles;
        private System.Windows.Forms.ToolStripMenuItem menuCustomRangeStats;
        private System.Windows.Forms.ToolStripSeparator menuSeparator1;
        private MetroFramework.Controls.MetroLabel lblManualUpdateVersion;
        private MetroFramework.Controls.MetroLabel linkToIPinfoWebsite;
    }
}