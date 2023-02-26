namespace FallGuysStats {
    partial class SetLang {
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
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SetLang));
            this.cboMultilingual = new System.Windows.Forms.ComboBox();
            this.picLanguageSelection = new System.Windows.Forms.PictureBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.grpLang = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.picLanguageSelection)).BeginInit();
            this.grpLang.SuspendLayout();
            this.SuspendLayout();
            // 
            // cboMultilingual
            // 
            this.cboMultilingual.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboMultilingual.Items.AddRange(new object[] {
            "English",
            "Français",
            "한국어",
            "日本語",
            "简体中文"});
            this.cboMultilingual.Location = new System.Drawing.Point(51, 23);
            this.cboMultilingual.Name = "cboMultilingual";
            this.cboMultilingual.Size = new System.Drawing.Size(94, 21);
            this.cboMultilingual.TabIndex = 100;
            this.cboMultilingual.SelectedIndexChanged += new System.EventHandler(this.CboMultilingual_SelectedIndexChanged);
            // 
            // picLanguageSelection
            // 
            this.picLanguageSelection.Image = global::FallGuysStats.Properties.Resources.language_icon;
            this.picLanguageSelection.Location = new System.Drawing.Point(23, 23);
            this.picLanguageSelection.Name = "picLanguageSelection";
            this.picLanguageSelection.Size = new System.Drawing.Size(22, 22);
            this.picLanguageSelection.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picLanguageSelection.TabIndex = 40;
            this.picLanguageSelection.TabStop = false;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnOK.Location = new System.Drawing.Point(160, 23);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(61, 22);
            this.btnOK.TabIndex = 101;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.BtnOK_Click);
            // 
            // grpLang
            // 
            this.grpLang.Controls.Add(this.cboMultilingual);
            this.grpLang.Controls.Add(this.btnOK);
            this.grpLang.Controls.Add(this.picLanguageSelection);
            this.grpLang.Location = new System.Drawing.Point(11, 3);
            this.grpLang.Margin = new System.Windows.Forms.Padding(2);
            this.grpLang.Name = "grpLang";
            this.grpLang.Padding = new System.Windows.Forms.Padding(2);
            this.grpLang.Size = new System.Drawing.Size(240, 59);
            this.grpLang.TabIndex = 100;
            this.grpLang.TabStop = false;
            this.grpLang.Text = "Language";
            // 
            // SetLang
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(251)))));
            this.ClientSize = new System.Drawing.Size(262, 73);
            this.ControlBox = false;
            this.Controls.Add(this.grpLang);
            this.ForeColor = System.Drawing.Color.Black;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SetLang";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Fall Guys Stats";
            this.Load += new System.EventHandler(this.SetLang_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picLanguageSelection)).EndInit();
            this.grpLang.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grpLang;
        private System.Windows.Forms.PictureBox picLanguageSelection;
        private System.Windows.Forms.ComboBox cboMultilingual;
        private System.Windows.Forms.Button btnOK;
    }
}