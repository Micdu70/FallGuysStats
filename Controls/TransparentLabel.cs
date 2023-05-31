using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace FallGuysStats {
    public class TransparentLabel : Label {
        protected override CreateParams CreateParams {
            get {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;
                return cp;
            }
        }
        public TransparentLabel() {
            this.DrawVisible = true;
            this.TextRight = null;
            this.Visible = false;
        }
        [DefaultValue(null)]
        public string TextRight { get; set; }
        [DefaultValue(true)]
        public bool DrawVisible { get; set; }
        public Image PlatformIcon { get; set; }
        public int ImageX { get; set; }
        public int ImageY { get; set; }
        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }
        public Color LevelColor { get; set; }
        public Image RoundIcon { get; set; }
        public bool IsCreativeRound { get; set; }
        public bool IsShareCodeFormat { get; set; }
        public void Draw(Graphics g) {
            if (!this.DrawVisible) { return; }
            if (this.PlatformIcon != null) {
                using (SolidBrush brFore = new SolidBrush(this.ForeColor)) {
                    StringFormat stringFormat = new StringFormat {
                        Alignment = StringAlignment.Far,
                        LineAlignment = StringAlignment.Far
                    };
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.InterpolationMode = InterpolationMode.HighQualityBilinear;
                    g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                    g.DrawImage(this.PlatformIcon, this.ImageX, this.ImageY, this.ImageWidth == 0 ? this.PlatformIcon.Width : this.ImageWidth, this.ImageHeight == 0 ? this.PlatformIcon.Height : this.ImageHeight);
                    if (this.TextRight != null) {
                        g.DrawString(this.TextRight, new Font(this.Font.FontFamily, 12, this.Font.Style, GraphicsUnit.Pixel), brFore, this.ClientRectangle, stringFormat);
                    }
                }
            } else {
                using (SolidBrush brBack = new SolidBrush(this.BackColor)) {
                    using (SolidBrush brFore = new SolidBrush(this.ForeColor)) {
                        g.SmoothingMode = SmoothingMode.HighQuality;
                        g.InterpolationMode = InterpolationMode.HighQualityBilinear;
                        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                        StringFormat stringFormat = new StringFormat {
                            Alignment = StringAlignment.Near
                        };
                        switch (this.TextAlign) {
                            case ContentAlignment.BottomLeft:
                            case ContentAlignment.BottomCenter:
                            case ContentAlignment.BottomRight:
                                stringFormat.LineAlignment = StringAlignment.Far;
                                break;
                            case ContentAlignment.MiddleLeft:
                            case ContentAlignment.MiddleCenter:
                            case ContentAlignment.MiddleRight:
                                stringFormat.LineAlignment = StringAlignment.Center;
                                break;
                            case ContentAlignment.TopLeft:
                            case ContentAlignment.TopCenter:
                            case ContentAlignment.TopRight:
                                stringFormat.LineAlignment = StringAlignment.Near;
                                break;
                        }
                        switch (this.TextAlign) {
                            case ContentAlignment.TopCenter:
                            case ContentAlignment.MiddleCenter:
                            case ContentAlignment.BottomCenter:
                                if (string.IsNullOrEmpty(this.TextRight)) {
                                    stringFormat.Alignment = StringAlignment.Center;
                                }
                                break;
                        }

                        if (!string.IsNullOrEmpty(this.Text)) {
                            /*if (this.Name.Equals("lblRound")) {
                                if (!this.LevelColor.IsEmpty) {
                            this.DrawOutlineText(g, this.ClientRectangle, null, brFore, this.Font.FontFamily, this.Font.Style, this.Font.Size, this.Text, stringFormat);
                                    //g.DrawString(this.Text, this.Font, brFore, this.ClientRectangle.X, this.ClientRectangle.Y + ((Stats.CurrentLanguage == 2 || Stats.CurrentLanguage == 3) ? 10 : 0), stringFormat);
                                    
                                } else {
                                    this.DrawOutlineText(g, this.ClientRectangle, null, brFore, this.Font.FontFamily, this.Font.Style, this.Font.Size, this.Text, stringFormat);
                                    //g.DrawString(this.Text, this.Font, brFore, this.ClientRectangle, stringFormat);
                        }
                            } else {
                                this.DrawOutlineText(g, this.ClientRectangle, null, brFore, this.Font.FontFamily, this.Font.Style, this.Font.Size, this.Text, stringFormat);
                                //g.DrawString(this.Text, this.Font, brFore, this.ClientRectangle, stringFormat);
                            }*/
                            this.DrawOutlineText(g, this.ClientRectangle, null, brFore, this.Font.FontFamily, this.Font.Style, this.Font.Size, this.Text, stringFormat);
                        }

                        if (this.Image != null) {
                            g.DrawImage(this.Image, this.ImageX, this.ImageY, this.ImageWidth == 0 ? this.Image.Width : this.ImageWidth, this.ImageHeight == 0 ? this.Image.Height : this.ImageHeight);
                        }

                        if (!string.IsNullOrEmpty(this.TextRight)) {
                            stringFormat.Alignment = StringAlignment.Far;
                            if (this.Name.Equals("lblRound")) {
                                Font fontForLongText = this.GetFontForLongText(this.TextRight);
                                if (!this.LevelColor.IsEmpty) {
                                    //float sizeOfText = g.MeasureString(this.TextRight, fontForLongText).Width;
                                    float widthOfText = this.GetNewSizeOfText(this.TextRight, fontForLongText);
                                    this.FillRoundedRectangle(g, null, new SolidBrush(this.LevelColor), (int)(this.ClientRectangle.Width - widthOfText), this.ClientRectangle.Y, (int)widthOfText, 22, 10);
                                    if (this.RoundIcon != null) {
                                        g.DrawImage(this.RoundIcon, this.ClientRectangle.Width - widthOfText - this.ImageWidth - 5, this.ClientRectangle.Y, this.ImageWidth, this.ImageHeight);
                                    }
                                }
                                brFore.Color = this.LevelColor.IsEmpty ? this.ForeColor : Color.White;
                                this.DrawOutlineText(g, this.ClientRectangle, null, brFore, fontForLongText.FontFamily, fontForLongText.Style, fontForLongText.Size, this.TextRight, stringFormat);
                                //g.DrawString(this.TextRight, this.GetFontForLongText(this.TextRight), brFore, this.ClientRectangle, stringFormat);
                            } else {
                                this.DrawOutlineText(g, this.ClientRectangle, null, brFore, this.Font.FontFamily, this.Font.Style, this.Font.Size * this.GetFontSizeFactor(), this.TextRight, stringFormat);
                                //g.DrawString(this.TextRight, this.Font, brFore, this.ClientRectangle, stringFormat);
                            }
                        }
                    }
                }
            }
        }
        private Color GetComplementaryColor(Color source, int alpha) {
            return Color.FromArgb(alpha, 255 - source.R, 255 - source.G, 255 - source.B);
        }
        private float GetFontSizeFactor() {
            switch (this.Name) {
                case "lblFinals":
                    return (this.TextRight.Length > 15 ? (Stats.CurrentLanguage == 0 ? 0.92f : Stats.CurrentLanguage == 1 ? 0.87f : 1) : 1);
                case "lblStreak":
                    return (this.TextRight.Length > 9 ? (Stats.CurrentLanguage == 0 ? 0.92f : Stats.CurrentLanguage == 1 ? 0.87f : 1) : 1);
                case "lblQualifyChance":
                    return (this.TextRight.Length > 18 ? (Stats.CurrentLanguage == 0 ? 0.92f : Stats.CurrentLanguage == 1 ? 0.87f : 1) : 1);
                default:
                    return 1f;
            }
        }
        private Font GetFontForLongText(string text) {
            return Stats.CurrentLanguage <= 3 && text.Length > 9
                   ? new Font(this.Font.FontFamily, this.GetRoundNameFontSize(text.Length), this.Font.Style, GraphicsUnit.Pixel)
                   : this.Font;
        }
        private float GetRoundNameFontSize(int textLength) {
            float defaultFontSize = 18.0F;
            float fontSize = this.Font.Size;
            if (fontSize < 18) {
                fontSize += defaultFontSize - fontSize;
            } else {
                fontSize -= fontSize - defaultFontSize;
            }

            if (this.IsShareCodeFormat) {
                fontSize -= 0.0F;
            } else {
                if (this.IsCreativeRound || Stats.CurrentLanguage == 0 || Stats.CurrentLanguage == 1) { // English, French (or an Official Creative Round)
                    if (textLength == 10) {
                        fontSize -= 2.0F;
                    } else if (textLength == 11) {
                        fontSize -= 2.5F;
                    } else if (textLength == 12) {
                        fontSize -= 3.0F;
                    } else if (textLength == 13) {
                        fontSize -= 3.5F;
                    } else if (textLength == 14) {
                        fontSize -= 4.0F;
                    } else if (textLength == 15) {
                        fontSize -= 4.5F;
                    } else if (textLength == 16) {
                        fontSize -= 5.0F;
                    } else if (textLength == 17) {
                        fontSize -= 5.5F;
                    } else if (textLength == 18) {
                        fontSize -= 6.0F;
                    } else if (textLength == 19) {
                        fontSize -= 6.5F;
                    } else if (textLength == 20) {
                        fontSize -= 7.0F; ;
                    } else if (textLength == 21) {
                        fontSize -= 7.5F;
                    } else if (textLength == 22) {
                        fontSize -= 8.0F;
                    } else if (textLength == 23) {
                        fontSize -= 8.5F;
                    } else if (textLength == 24) {
                        fontSize -= 9.0F;
                    } else if (textLength == 25) {
                        fontSize -= 9.5F;
                    } else if (textLength >= 26) {
                        fontSize -= 10.0F;
                    }
                } else if (Stats.CurrentLanguage == 2) { // Korean
                    if (textLength == 10) {
                        fontSize -= 1.0F;
                    } else if (textLength == 11) {
                        fontSize -= 1.5F;
                    } else if (textLength == 12) {
                        fontSize -= 2.0F;
                    } else if (textLength == 13) {
                        fontSize -= 2.5F;
                    } else if (textLength >= 14) {
                        fontSize -= 3.0F;
                    }
                } else if (Stats.CurrentLanguage == 3) { // Japanese
                    if (textLength == 10) {
                        fontSize -= 4.0F;
                    } else if (textLength == 11) {
                        fontSize -= 4.5F;
                    } else if (textLength == 12) {
                        fontSize -= 5.0F;
                    } else if (textLength == 13) {
                        fontSize -= 5.5F;
                    } else if (textLength >= 14) {
                        fontSize -= 6.0F;
                    }
                }
            }
            return fontSize;
        }
        private void FillRoundedRectangle(Graphics g, Pen pen, Brush brush, int x, int y, int width, int height, int radius) {
            using (GraphicsPath path = new GraphicsPath()) {
                Rectangle corner = new Rectangle(x, y, radius, radius);
                path.AddArc(corner, 180, 90);
                corner.X = x + width - radius;
                path.AddArc(corner, 270, 90);
                corner.Y = y + height - radius;
                path.AddArc(corner, 0, 90);
                corner.X = x;
                path.AddArc(corner, 90, 90);
                path.CloseFigure();
                g.FillPath(brush, path);
                if (pen != null) {
                    g.DrawPath(pen, path);
                }
            }
        }
        private void DrawOutlineText(Graphics g, Rectangle layoutRect, Pen outlinePen, Brush fillBrush, FontFamily fontFamily, FontStyle fontStyle, float fontSize, string text, StringFormat stringFormat) {
            using (GraphicsPath path = new GraphicsPath()) {
                path.AddString(text, fontFamily, (int)fontStyle, fontSize, layoutRect, stringFormat);
                path.CloseFigure();
                g.FillPath(fillBrush, path);
                if (outlinePen != null) g.DrawPath(outlinePen, path);
            }
        }
        private float GetNewSizeOfText(string s, Font font) {
            float sizeOfText = TextRenderer.MeasureText(s, font).Width;
            float defaultFontSize = 18.0F;
            float fontSize = font.Size;
            if (fontSize < 18) {
                fontSize += defaultFontSize - fontSize;
            } else {
                fontSize -= fontSize - defaultFontSize;
            }
            return sizeOfText + fontSize;
        }
    }
}