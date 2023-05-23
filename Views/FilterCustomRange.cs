using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using MetroFramework;
using MetroFramework.Controls;

namespace FallGuysStats {
    public partial class FilterCustomRange : MetroFramework.Forms.MetroForm {
        public DateTime startTime = DateTime.MinValue;
        public DateTime endTime = DateTime.MaxValue;
        public Stats StatsForm { get; set; }
        private readonly List<DateTime[]> templates = new List<DateTime[]>();

        public FilterCustomRange() => this.InitializeComponent();

        private void FilterCustomRange_Load(object sender, EventArgs e) {
            this.SuspendLayout();
            this.SetTheme(this.StatsForm.CurrentSettings.Theme == 0 ? MetroThemeStyle.Light : this.StatsForm.CurrentSettings.Theme == 1 ? MetroThemeStyle.Dark : MetroThemeStyle.Default);
            this.ResumeLayout(false);

            //this.Font = Overlay.GetMainFont(12);
            this.Text = Multilingual.GetWord("main_custom_range");
            this.lblCustomRange.Text = Multilingual.GetWord("custom_range_range");
            this.lblTemplates.Text = Multilingual.GetWord("custom_range_templates");
            this.chkStartNotSet.Text = Multilingual.GetWord("custom_range_not_set");
            this.chkEndNotSet.Text = Multilingual.GetWord("custom_range_not_set");
            this.btnFilter.Text = Multilingual.GetWord("custom_range_filter");

            this.templatesListBox.Items.Clear();
            for (int i = 0; i < Stats.Seasons.Count; i++) {
                if (Stats.Seasons.Count - 1 == i) {
                    this.templatesListBox.Items.Add(Multilingual.GetWord("custom_range_season") + " " + (i < 6 ? (i + 1) + " [" + Multilingual.GetWord("custom_range_legacy") + "]" : (i - 5) + " [" + Multilingual.GetWord("custom_range_ffa") + "]") + " (" + Stats.Seasons[i].ToString("d") + "-)");
                    templates.Add(new DateTime[2] { Stats.Seasons[i], DateTime.MaxValue });
                } else {
                    this.templatesListBox.Items.Add(Multilingual.GetWord("custom_range_season") + " " + (i < 6 ? (i + 1) + " [" + Multilingual.GetWord("custom_range_legacy") + "]" : (i - 5) + " [" + Multilingual.GetWord("custom_range_ffa") + "]") + " (" + Stats.Seasons[i].ToString("d") + "-" + Stats.Seasons[i + 1].ToString("d") + ")");
                    templates.Add(new DateTime[2] { Stats.Seasons[i], Stats.Seasons[i + 1] });
                }
            }
        }

        private void SetTheme(MetroThemeStyle theme) {
            this.Theme = theme;
            foreach (Control c1 in Controls) {
                if (c1 is MetroLabel ml1) {
                    ml1.Theme = theme;
                } else if (c1 is MetroTextBox mtb1) {
                    mtb1.Theme = theme;
                } else if (c1 is MetroButton mb1) {
                    mb1.Theme = theme;
                } else if (c1 is MetroCheckBox mcb1) {
                    mcb1.Theme = theme;
                } else if (c1 is MetroDateTime mdt1) {
                    mdt1.Theme = theme;
                } else if (c1 is ListBox lb1) {
                    lb1.BackColor = this.Theme == MetroThemeStyle.Light ? Color.WhiteSmoke : Color.FromArgb(2, 2, 2);
                    lb1.ForeColor = this.Theme == MetroThemeStyle.Light ? Color.Black : Color.DarkGray;
                }
            }
        }

        private void TemplatesListBox_SelectedValueChanged(object sender, EventArgs e) {
            if (this.templatesListBox.SelectedIndex < 0) { return; }

            if (templates[this.templatesListBox.SelectedIndex][0] == DateTime.MinValue) {
                this.dtStart.Visible = false;
                this.chkStartNotSet.Checked = true;
            } else {
                this.dtStart.Visible = true;
                this.chkStartNotSet.Checked = false;
                this.dtStart.Value = templates[this.templatesListBox.SelectedIndex][0];
            }
            if (templates[this.templatesListBox.SelectedIndex][1] == DateTime.MaxValue) {
                this.dtEnd.Visible = false;
                this.chkEndNotSet.Checked = true;
            } else {
                this.dtEnd.Visible = true;
                this.chkEndNotSet.Checked = false;
                this.dtEnd.Value = templates[this.templatesListBox.SelectedIndex][1];
            }
        }

        private void ChkStartNotSet_CheckedChanged(object sender, EventArgs e) {
            this.dtStart.Visible = !this.chkStartNotSet.Checked;
            if (this.chkStartNotSet.Checked && this.chkEndNotSet.Checked) {
                this.chkEndNotSet.Checked = false;
            }
        }

        private void ChkEndNotSet_CheckedChanged(object sender, EventArgs e) {
            this.dtEnd.Visible = !this.chkEndNotSet.Checked;
            if (this.chkEndNotSet.Checked && this.chkStartNotSet.Checked) {
                this.chkStartNotSet.Checked = false;
            }
        }

        private void BtnFilter_Click(object sender, EventArgs e) {
            this.startTime = this.chkStartNotSet.Checked ? DateTime.MinValue : this.dtStart.Value;
            this.endTime = this.chkEndNotSet.Checked ? DateTime.MaxValue : this.dtEnd.Value;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
