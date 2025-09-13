using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace FontConvert880
{
    public partial class UI : Form
    {
        private readonly PictureBox[] boxes = new PictureBox[95];
        private readonly GlyphTransform[] gtransforms = new GlyphTransform[95];
        private Font font = new("Arial", 26f, FontStyle.Bold, GraphicsUnit.Pixel);
        private bool supTransChange = false;

        private readonly List<PictureBox> selected = [];

        private int FontH => (int)FontSizeH.Value;
        private int FontV => (int)FontSizeV.Value;

        public UI()
        {
            InitializeComponent();

            //List<byte> conv = [];
            //byte[] b = File.ReadAllBytes(@"R:\font8x16.bin");
            //for (int i = 0; i < b.Length; i+=16)
            //{
            //    for (int j = i; j < i + 8; j++)
            //    {
            //        conv.Add(b[j]);
            //        conv.Add(b[j+8]);
            //    }
            //}
            //File.WriteAllBytes(@"R:\default8x16.bin", conv.ToArray());


            for (int i = 0; i < 95; i++)
            {
                int c = i % 10;
                int r = i / 10;
                PictureBox pb = new()
                {
                    BorderStyle = BorderStyle.FixedSingle,
                    Dock = DockStyle.Fill,
                    BackgroundImageLayout = ImageLayout.Zoom,
                };
                pb.Click += Glyph_Click;
                MainGrid.SetColumn(pb, c);
                MainGrid.SetRow(pb, r);
                MainGrid.Controls.Add(pb);
                MainGrid.Margin = new(3, 3, 3, 3);
                boxes[i] = pb;
                pb.Tag = i;
                GlyphTransform gt = new();
                gtransforms[i] = gt;
            }
            ShowFontName();
            ShowFont();

        }

        private void Glyph_Click(object? sender, EventArgs e)
        {
            if (sender is PictureBox pb)
            {
                if (!selected.Remove(pb))
                {
                    selected.Add(pb);
                    RecallGlyphTransform(pb);
                }
                else
                {
                    if (selected.Count > 0)
                        RecallGlyphTransform(selected[0]);
                }
                DrawSelected(pb);
                Status(string.Empty);
            }
        }

        public void ShowFontName()
        {
            FontNameLabel.Font = new(font.FontFamily, 11, font.Style);
            FontNameLabel.Text = $"{font.FontFamily.Name} {font.Size:F1}";
        }

        public void DrawSelected(PictureBox pb)
        {
            pb.BackColor = selected.Contains(pb) ? Color.DarkBlue : Color.Black;
        }

        public void ShowFont()
        {
            for (int i = 0; i < 95; i++)
            {
                Bitmap bm = new(FontH - 1, FontV - 1);
                using (Graphics g = Graphics.FromImage(bm))
                {
                    string character = $"{(char)(i + 32)}";
                    using GraphicsPath path = new();
                    path.AddString(character, font.FontFamily, (int)font.Style, font.Size, new Point(0, 0), StringFormat.GenericDefault);
                    RectangleF bounds = path.GetBounds();
                    float offsetX = (FontH - bounds.Width) / 2f;
                    offsetX += (float)GlobalHOff.Value + gtransforms[i].HOffset;
                    g.ScaleTransform((float)GlobalHSize.Value * gtransforms[i].HSize, (float)GlobalVSize.Value * gtransforms[i].VSize);
                    g.TranslateTransform(offsetX, (float)GlobalVOff.Value + gtransforms[i].VOffset);
                    g.FillPath(Brushes.White, path);
                }
                var oldImage = boxes[i].BackgroundImage;
                DrawSelected(boxes[i]);
                boxes[i].BackgroundImage = bm;
                boxes[i].Refresh();
                oldImage?.Dispose();
            }
            Status(string.Empty);
        }

        private void FontChooseButton_Click(object sender, EventArgs e)
        {
            using var fd = new FontDialog()
            {
                Font = font
            };
            if (fd.ShowDialog() == DialogResult.OK)
            {
                font = fd.Font;
                ShowFontName();
                ShowFont();
                Status("Font Family/Style/Point Loaded");
            }

        }

        public void RecallGlyphTransform(int index)
        {
            supTransChange = true;
            GlyphHOff.Value = (decimal)gtransforms[index].HOffset;
            GlyphVOff.Value = (decimal)gtransforms[index].VOffset;
            GlyphHSize.Value = (decimal)gtransforms[index].HSize;
            GlyphVSize.Value = (decimal)gtransforms[index].VSize;
            supTransChange = false;
        }

        public void RecallGlyphTransform(PictureBox pb)
        {
            if (pb.Tag is int i)
            {
                RecallGlyphTransform(i);
            }
        }


        private void Globals_Changed(object sender, EventArgs e)
        {
            if (supTransChange) return;
            ShowFont();
        }

        private void PerGlyph_Changed(object sender, EventArgs e)
        {
            if (!supTransChange)
            {
                foreach (var pb in selected)
                {
                    if (pb.Tag is int i)
                    {
                        var gt = gtransforms[i];
                        gt.HOffset = (float)GlyphHOff.Value;
                        gt.VOffset = (float)GlyphVOff.Value;
                        gt.HSize = (float)GlyphHSize.Value;
                        gt.VSize = (float)GlyphVSize.Value;
                    }
                }
                ShowFont();
            }
        }

        private void SelectionButton_Clicked(object sender, EventArgs e)
        {
            if (sender == AllUpperButton)
            {
                for (int i = 33; i < 59; i++)
                {
                    if (!selected.Contains(boxes[i]))
                        selected.Add(boxes[i]);
                }
                RecallGlyphTransform(33);
            }
            else
            if (sender == AllLowerButton)
            {
                for (int i = 65; i < 91; i++)
                {
                    if (!selected.Contains(boxes[i]))
                        selected.Add(boxes[i]);
                }
                RecallGlyphTransform(65);
            }
            else
            if (sender == AllNumbersButton)
            {
                for (int i = 16; i < 26; i++)
                {
                    if (!selected.Contains(boxes[i]))
                        selected.Add(boxes[i]);
                }
                RecallGlyphTransform(16);
            }
            else
            if (sender == AllSymbolsButton)
            {
                for (int i = 0; i < 95; i++)
                {
                    if (i >= 33 && i < 59) continue;
                    if (i >= 65 && i < 91) continue;
                    if (i >= 16 && i < 26) continue;
                    if (!selected.Contains(boxes[i]))
                        selected.Add(boxes[i]);
                }
                RecallGlyphTransform(0);
            }
            else
            if (sender == ClearButton)
            {
                selected.Clear();
            }
            ShowFont();
        }

        private void FontSize_Changed(object sender, EventArgs e)
        {
            bool ok = true;
            int mod = FontV & 7;
            if (mod != 0)
            {
                FontSizeV.Value = (FontV & 0xF8) + (mod == 1 ? 8 : 0);
                ok = false;
            }
            mod = FontH & 7;
            if (mod != 0)
            {
                FontSizeH.Value = (FontH & 0xF8) + (mod == 1 ? 8 : 0);
                ok = false;
            }
            if (ok)
                ShowFont();
        }


        private static string Lf(string text) => text + "\r\n";

        private void SaveCurrent(string fileName)
        {
            string s =
                Lf($"fontfamily={font.FontFamily.Name}") +
                Lf($"fontsize={font.Size:F1}") +
                Lf($"fontstyle={font.Style}") +
                Lf($"bitmapwidth={FontH}") +
                Lf($"bitmapheight={FontV}") +
                Lf($"globalhoffset={GlobalHOff.Value:F1}") +
                Lf($"globalvoffset={GlobalVOff.Value:F1}") +
                Lf($"globalhsize={GlobalHSize.Value:F2}") +
                Lf($"globalvsize={GlobalVSize.Value:F2}");
            for (int i = 0; i < 95; i++)
            {
                s +=
                    Lf($"hoff={i} {gtransforms[i].HOffset:F1}") +
                    Lf($"voff={i} {gtransforms[i].VOffset:F1}") +
                    Lf($"hsize={i} {gtransforms[i].HSize:F2}") +
                    Lf($"vsize={i} {gtransforms[i].VSize:F2}");
            }
            try
            {
                File.WriteAllText(fileName, s);
                Status("Font definition saved");
            }
            catch
            {
                Status("Error saving font definition file.");
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            using var sfd = new SaveFileDialog()
            {
                Title = "Save font definition file",
                Filter = "FONT880 Files|*.font880"
            };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                SaveCurrent(sfd.FileName);
            }
        }

        private void LoadCurrent(string fileName)
        {
            string[] lines;
            string fontFamily = "Arial", fontStyle = "Bold";
            float fontSize = 26;
            try
            {
                lines = File.ReadAllLines(fileName);
            }
            catch             
            {
                Status("Error loading font definition file.");
                return;
            }
            supTransChange = true;
            foreach (string line in lines)
            {
                string[] field = line.Split('=');
                if (field.Length == 2)
                {
                    float f;
                    string[] p = field[1].Split(" ");
                    f = float.TryParse(field[1], out float temp) ? temp : 0;
                    int i = 0;
                    if (p.Length == 2)
                    {
                        i = int.TryParse(p[0], out int ii) ? ii : 0;
                        f = float.TryParse(p[1], out temp) ? temp : 0;
                    }
                    switch (field[0])
                    {
                        case "fontfamily":
                            fontFamily = field[1];
                            break;
                        case "fontstyle":
                            fontStyle = field[1];
                            break;
                        case "fontsize":
                            fontSize = f;
                            break;
                        case "bitmapwidth":
                            FontSizeH.Value = (decimal)f;
                            break;
                        case "bitmapheight":
                            FontSizeV.Value = (decimal)f;
                            break;
                        case "globalhoffset":
                            GlobalHOff.Value = (decimal)f;
                            break;
                        case "globalvoffset":
                            GlobalVOff.Value = (decimal)f;
                            break;
                        case "globalhsize":
                            GlobalHSize.Value = (decimal)f;
                            break;
                        case "globalvsize":
                            GlobalVSize.Value = (decimal)f;
                            break;
                        case "hoff":
                            gtransforms[i].HOffset = f;
                            break;
                        case "voff":
                            gtransforms[i].VOffset = f;
                            break;
                        case "hsize":
                            gtransforms[i].HSize = f;
                            break;
                        case "vsize":
                            gtransforms[i].VSize = f;
                            break;
                    }
                }
            }
            font?.Dispose();
            font = new(fontFamily, fontSize, (FontStyle)FontStyle.Parse(typeof(FontStyle), fontStyle), GraphicsUnit.Pixel);
            supTransChange = false;
            ShowFontName();
            ShowFont();
            Status("Font definition loaded");
        }

        private void LoadButton_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog()
            {
                Title = "Load font definition file",
                Filter = "FONT880 Files|*.font880|All Files|*.*"
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                LoadCurrent(ofd.FileName);
            }
        }

        private void Status(string message)
        {
            StatusLabel.Text = message;
        }

        private void SaveFontBinary(string fileName, bool src)
        {
            List<byte> bytes = [];
            string source = $"uint8_t font{font.FontFamily.Name}{FontH}x{FontV}[] " + " = {\r\n";
            
            for (int box = 0; box < 95; box++)
            {
                source += $"\t// '{(char)(box+32)}'\r\n\t";
                Bitmap bm = (Bitmap)boxes[box].BackgroundImage!;
                // FontH and FontW are always multiples of 8
                for (int x = 0; x < FontH; x++)
                {
                    int byt = 0;
                    for (int y = 0; y < FontV; y++)
                    {
                        int mask = 1 << (y & 7);
                        bool pxSet = x < bm.Width && y < bm.Height && bm.GetPixel(x, y).R > 128;
                        if(pxSet) 
                            byt |= mask;
                        if (mask == 128)
                        {
                            source += $"0x{byt:X2},";
                            bytes.Add((byte)byt);
                            byt = 0;
                        }
                    }
                }
                source += "\r\n";
            }
            source += "};\r\n";
            try
            {
                if (src)
                {
                    File.WriteAllText(fileName, source);
                }
                else
                {
                    File.WriteAllBytes(fileName, [.. bytes]);
                }
                Status("Saved bitmapped font file.");
            }
            catch
            {
                Status("Error saving bitmapped font file.");
            }
        }

        private void ExportButton_Click(object sender, EventArgs e)
        {
            bool src = sender == SourceButton;
            using var sfd = new SaveFileDialog()
            {
                Title = $"Save font {(src ? "source" : "binary")} file",
                Filter = src ?
                    "C Files|*.c|All Files|*.*" :
                    "RMSFONT Files|*.rmsfont|All Files|*.*"
            };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                SaveFontBinary(sfd.FileName, src);
            }
        }
    }
}
