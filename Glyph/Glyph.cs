using System.Globalization;
using LambdaKit.Terminal;

namespace Glyph
{
    internal static class Glyph {
        internal static byte RGBColorPaletteState = 0;
        private static string? RGBColorPaletteCode = null;
        private static File? file;
        internal static File File { get { return file ?? throw new Exception("No file loaded"); } }
        private static Timer? timer;
        internal static List<List<StyledString>> text = [];
        internal static void Load(string path) {
            file = new(path);
            text = File.Parse();
        }
        internal static void Save() {
            File.Write(text);
                Terminal.Set("Saved", (0, 0), new Style{ForegroundColor = RGBColor.Orange, BackgroundColor=new RGBColor(1, 16, 41), Bold=true});
            timer = new((_) => {
                timer!.Dispose();
                Terminal.Set("Glyph", (0, 0), new Style{ForegroundColor = RGBColor.Orange, BackgroundColor=new RGBColor(1, 16, 41), Bold=true});
            }, null, TimeSpan.FromSeconds(2), TimeSpan.FromMilliseconds(-1));
        }
        internal static void ShowRGBColorPalette() {
            if (Cursor.from==(null, null)) { return; }
            RGBColorPaletteState = 1;
            Renderer.DrawPalette(Renderer.fgRGBColors);
        }
        internal static void ShowMarkerPalette() {
            if (Cursor.from==(null, null)) { return; }
            RGBColorPaletteState = 2;
            Renderer.DrawPalette(Renderer.bgRGBColors);
        }
        internal static void ChooseRGBColor(char key) { // TODO: fix text RGBColoring > 9: 1 less.
            RGBColor RGBColor;
            if (key=='.') {
                RGBColorPaletteCode="";
                Renderer.ClearPalette(RGBColorPaletteState==1);
                Renderer.DrawHexCodePalette(RGBColorPaletteCode);
                return;
            } else {
                if (RGBColorPaletteCode==null) {
                    RGBColor = RGBColorPaletteState==1 ? Renderer.fgRGBColors[key-'a'] : Renderer.bgRGBColors[key-'a'];
                } else {
                    RGBColorPaletteCode += key;
                    if (RGBColorPaletteCode.Length != 6) { Renderer.DrawHexCodePalette(RGBColorPaletteCode); return; }
                    try {
                        RGBColor = new(byte.Parse(RGBColorPaletteCode[..2], NumberStyles.HexNumber), byte.Parse(RGBColorPaletteCode.Substring(2, 2), NumberStyles.HexNumber), byte.Parse(RGBColorPaletteCode.Substring(4, 2), NumberStyles.HexNumber));
                    }
                    catch (FormatException) {
                        if (RGBColorPaletteCode == null) {
                            Renderer.ClearPalette(RGBColorPaletteState==1);
                        } else {
                            Renderer.ClearHexCodePalette();
                        }
                        Renderer.DrawChar(Cursor.from.X!.Value, Cursor.from.Y!.Value);
                        Cursor.from = (null, null);
                        RGBColorPaletteState = 0;
                        RGBColorPaletteCode = null;
                        return;
                    }
                    
                    RGBColorPaletteCode = null;
                }
            }
            if (RGBColorPaletteState == 1) {
                Cursor.Selection((StyledString old, int X, int Y) => {
                    old.style.ForegroundColor = RGBColor;
                    Renderer.DrawStyledStringAtTextCoord(old, X, Y);
                });
            } else {
                Cursor.Selection((StyledString old, int X, int Y) => {
                    old.style.BackgroundColor = RGBColor;
                    Renderer.DrawStyledStringAtTextCoord(old, X, Y);
                });
            }
            if (RGBColorPaletteCode == null) {
                Renderer.ClearPalette(RGBColorPaletteState==1);
            } else {
                Renderer.ClearHexCodePalette();
            }
            Renderer.DrawChar(Cursor.from.X!.Value, Cursor.from.Y!.Value);
            Cursor.from = (null, null);
            RGBColorPaletteState = 0;
            RGBColorPaletteCode = null;
        }
        internal static void Bold() {
            if (Cursor.from.X == null || Cursor.from.Y == null) {return;}
            Cursor.Selection((StyledString old, int X, int Y) => {
                old.style.Bold = !old.style.Bold;
                Renderer.DrawStyledStringAtTextCoord(old, X, Y);
            });
            Renderer.DrawChar(Cursor.from.X.Value, Cursor.from.Y.Value);
            Cursor.from=(null, null);
        }
        internal static void Itallic() {
            if (Cursor.from.X == null || Cursor.from.Y == null) {return;}
            Cursor.Selection((StyledString old, int X, int Y) => {
                old.style.Italic = !old.style.Italic;
                Renderer.DrawStyledStringAtTextCoord(old, X, Y);
            });
            Renderer.DrawChar(Cursor.from.X.Value, Cursor.from.Y.Value);
            Cursor.from=(null, null);
        }
        internal static void Underline() {
            if (Cursor.from.X == null || Cursor.from.Y == null) {return;}
            Cursor.Selection((StyledString old, int X, int Y) => {
                old.style.Underline = !old.style.Underline;
                Renderer.DrawStyledStringAtTextCoord(old, X, Y);
            });
            Renderer.DrawChar(Cursor.from.X.Value, Cursor.from.Y.Value);
            Cursor.from=(null, null);
        }
        internal static void Type(ConsoleKey key, char keyChar, bool shift) {
            if (key == ConsoleKey.Escape) {
                if (Cursor.from.X == null || Cursor.from.Y == null) {return;}
                Renderer.DrawChar(Cursor.from.X.Value, Cursor.from.Y.Value);
                Cursor.from = (null, null);
                return;
            } else if (key == ConsoleKey.Enter) {
                Cursor.NewLine();
                return;
            } else if (key == ConsoleKey.Backspace) { 
                if (Cursor.X == 0) {
                    if (Cursor.Y == 0) {return;}
                    int nextX = Renderer.GetLength(Cursor.Y-1);
                    text[Cursor.Y-1].AddRange(text[Cursor.Y]);
                    text.RemoveAt(Cursor.Y);
                    if (text.Count > Cursor.Y+1) {
                        Renderer.FullDraw();
                    } else {
                        Renderer.Draw();
                    }
                    Cursor.UpdateCursor((nextX-Cursor.X, -1));
                } else {
                    int x = Cursor.X - 1;
                    int posInLine = 0;
                    for (int i = 0; i < text[Cursor.Y].Count; i++) {
                        StyledString str = text[Cursor.Y][i];
                        if (x < posInLine+str.text.Length && x >= posInLine) {
                            if (str.text.Length == 1) {
                                text[Cursor.Y].RemoveAt(i);
                            } else {
                                text[Cursor.Y][i] = str with { text = str.text.Remove(x - posInLine, 1)};
                            }
                            break;
                        }
                        posInLine += str.text.Length;
                    }
                    Renderer.DrawLine(Cursor.Y);
                    Renderer.DrawChar(Renderer.GetLength(Cursor.Y)-Scroll.X, Cursor.Y);
                    Cursor.Left();
                }
            } else if (!char.IsControl(keyChar)) { // FIXME: make it add to previous.
                int x = Cursor.X-1 < 0 ? 0 : Cursor.X;
                string s = (shift ? char.ToUpper(keyChar) : keyChar).ToString();
                StyledString? str = Renderer.GetStyledStringAt(x-1, Cursor.Y, out int? charIndex, out int? index);
                if (str == null) { // here
                    text[Cursor.Y].Add(new StyledString() { text = s, style = new Style { ForegroundColor = RGBColor.White, BackgroundColor = RGBColor.Black} });
                } else if (str.Value.style == new Style {BackgroundColor = RGBColor.Black, ForegroundColor = RGBColor.White}||str.Value.style == new Style {BackgroundColor = PalleteColor.Default, ForegroundColor = RGBColor.White}) {
                    str = text[Cursor.Y][index!.Value];
                    text[Cursor.Y][index!.Value] = new StyledString {
                        text = str.Value.text.Insert(x-charIndex!.Value, s),
                        style = new Style {
                            ForegroundColor = RGBColor.White,
                            BackgroundColor = RGBColor.Black
                        }
                    };
                } else {
                    if (!Cursor.IsOnPartSplit(x, Cursor.Y, out int split)) {
                        split = Cursor.CreateSplit(split, x, Cursor.Y);
                    }
                    text[Cursor.Y].Insert(split, new StyledString {
                        text = s,
                        style = new Style {
                            ForegroundColor = RGBColor.White,
                            BackgroundColor = RGBColor.Black
                        }
                    });
                }
                Renderer.DrawLine(Cursor.Y);
                Cursor.Right();
            }
        }
        
        internal static void Setup() {
            Terminal.HideCursor = true;
			Terminal.UseAltScreenAndSaveCursor();
			Terminal.ClearAll();
			Terminal.ClearScreen();
			Renderer.Init(File.Name);
        }
        internal static void Exit() {
			Terminal.ClearAll();
			Terminal.ClearScreen();
			Terminal.UseNormScreenAndRestoreCursor();
			Terminal.HideCursor = false;
        }
    }
}