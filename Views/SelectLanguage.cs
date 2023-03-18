using System;
using System.Drawing;
using System.Windows.Forms;
namespace FallGuysStats {
    public partial class SelectLanguage : Form {
        public int selectedLanguage = 1;
        public SelectLanguage() => this.InitializeComponent();

        private void SelectLanguage_Load(object sender, EventArgs e) {
            this.ChangeLanguage(1);
            this.cboLanguage.SelectedIndex = 1;
        }

        private void CboLanguage_SelectedIndexChanged(object sender, EventArgs e) {
            this.selectedLanguage = ((ComboBox)sender).SelectedIndex;
            this.ChangeLanguage(((ComboBox)sender).SelectedIndex);
        }

        private void BtnLanguageSave_Click(object sender, EventArgs e) {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void SelectLanguage_FormClosing(object sender, EventArgs e) {
            this.DialogResult = DialogResult.OK;
        }

        private void ChangeLanguage(int lang) {
            this.Font = new Font(Overlay.GetMainFontFamilies(lang), 9, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            this.Text = Multilingual.GetWordWithLang("settings_select_language_title", lang);
            this.btnLanguageSave.Text = Multilingual.GetWordWithLang("settings_select_language_button", lang);
        }
    }
}