using System;
using System.Reflection;
using System.Windows.Forms;

namespace FallGuysStats {
    public partial class SetLang : Form {
        public UserSettings CurrentSettings { get; set; }
        public SetLang() {
            this.InitializeComponent();
        }

        private void SetLang_Load(object sender, EventArgs e) {
            this.ChangeLanguage(Stats.CurrentLanguage);
            switch (Stats.CurrentLanguage) {
                case 0: this.cboMultilingual.SelectedItem = "English"; break;
                case 1: this.cboMultilingual.SelectedItem = "Français"; break;
                case 2: this.cboMultilingual.SelectedItem = "한국어"; break;
                case 3: this.cboMultilingual.SelectedItem = "日本語"; break;
                case 4: this.cboMultilingual.SelectedItem = "简体中文"; break;
            }
        }
        private void BtnOK_Click(object sender, EventArgs e) {
            switch ((string)this.cboMultilingual.SelectedItem) {
                case "English":
                    Stats.CurrentLanguage = 0;
                    break;
                case "Français":
                    Stats.CurrentLanguage = 1;
                    break;
                case "한국어":
                    Stats.CurrentLanguage = 2;
                    break;
                case "日本語":
                    Stats.CurrentLanguage = 3;
                    break;
                case "简体中文":
                    Stats.CurrentLanguage = 4;
                    break;
                default:
                    Stats.CurrentLanguage = 1;
                    break;
            }
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        private void CboMultilingual_SelectedIndexChanged(object sender, EventArgs e) {
            this.ChangeLanguage(((ComboBox)sender).SelectedIndex);
        }
        private void ChangeLanguage(int lang) {
            int tempLanguage = Stats.CurrentLanguage;
            Stats.CurrentLanguage = lang;

            this.grpLang.Text = Multilingual.GetWord("settings_language");
            this.Text = $"Fall Guys Stats \"FE\" v{Assembly.GetExecutingAssembly().GetName().Version.ToString(2)}";

            Stats.CurrentLanguage = tempLanguage;
        }
    }
}