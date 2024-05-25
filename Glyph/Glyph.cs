using System.Globalization;
using OxDED.Terminal;

namespace Glyph
{
    internal static class Glyph {
        internal static byte ColorPaletteState = 0;
        private static string? colorPaletteCode = null;
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
                Terminal.Set("Saved", (0, 0), new Style{ForegroundColor = Color.Orange, BackgroundColor=new Color(1, 16, 41), Bold=true});
            timer = new((_) => {
                timer!.Dispose();
                Terminal.Set("Glyph", (0, 0), new Style{ForegroundColor = Color.Orange, BackgroundColor=new Color(1, 16, 41), Bold=true});
            }, null, TimeSpan.FromSeconds(2), TimeSpan.FromMilliseconds(-1));
        }
        internal static void ShowColorPalette() {
            if (Cursor.from==(null, null)) { return; }
            ColorPaletteState = 1;
            Renderer.DrawPalette(Renderer.fgColors);
        }
        internal static void ShowMarkerPalette() {
            if (Cursor.from==(null, null)) { return; }
            ColorPaletteState = 2;
            Renderer.DrawPalette(Renderer.bgColors);
        }
        internal static void ChooseColor(char key) { // TODO: fix text coloring > 9: 1 less.
            Color color;
            if (key=='.') {
                colorPaletteCode="";
                Renderer.ClearPalette(ColorPaletteState==1);
                Renderer.DrawHexCodePalette(colorPaletteCode);
                return;
            } else {
                if (colorPaletteCode==null) {
                    color = ColorPaletteState==1 ? Renderer.fgColors[key-'a'] : Renderer.bgColors[key-'a'];
                } else {
                    colorPaletteCode += key;
                    if (colorPaletteCode.Length != 6) { Renderer.DrawHexCodePalette(colorPaletteCode); return; }
                    try {
                        color = new(byte.Parse(colorPaletteCode[..2], NumberStyles.HexNumber), byte.Parse(colorPaletteCode.Substring(2, 2), NumberStyles.HexNumber), byte.Parse(colorPaletteCode.Substring(4, 2), NumberStyles.HexNumber));
                    }
                    catch (FormatException) {
                        if (colorPaletteCode == null) {
                            Renderer.ClearPalette(ColorPaletteState==1);
                        } else {
                            Renderer.ClearHexCodePalette();
                        }
                        Renderer.DrawChar(Cursor.from.X!.Value, Cursor.from.Y!.Value);
                        Cursor.from = (null, null);
                        ColorPaletteState = 0;
                        colorPaletteCode = null;
                        return;
                    }
                    
                    colorPaletteCode = null;
                }
            }
            if (ColorPaletteState == 1) {
                Cursor.Selection((StyledString old, int X, int Y) => {
                    old.style.ForegroundColor = color;
                    Renderer.DrawStyledStringAtTextCoord(old, X, Y);
                });
            } else {
                Cursor.Selection((StyledString old, int X, int Y) => {
                    old.style.BackgroundColor = color;
                    Renderer.DrawStyledStringAtTextCoord(old, X, Y);
                });
            }
            if (colorPaletteCode == null) {
                Renderer.ClearPalette(ColorPaletteState==1);
            } else {
                Renderer.ClearHexCodePalette();
            }
            Renderer.DrawChar(Cursor.from.X!.Value, Cursor.from.Y!.Value);
            Cursor.from = (null, null);
            ColorPaletteState = 0;
            colorPaletteCode = null;
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
                Renderer.Draw(); // TODO?: optimalisation
                return;
            } else if (key == ConsoleKey.Backspace) { 
                if (Cursor.X == 0) {
                    if (Cursor.Y == 0) {return;}
                    int nextX = Renderer.GetLength(Cursor.Y-1);
                    text[Cursor.Y-1].AddRange(text[Cursor.Y]);
                    text.RemoveAt(Cursor.Y);
                    Renderer.FullDraw();
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
            } else if (!char.IsControl(keyChar)) {
                int x = Cursor.X-1 < 0 ? 0 : Cursor.X;
                string s = (shift ? char.ToUpper(keyChar) : keyChar).ToString();
                StyledString? str = Renderer.GetStyledStringAt(x, Cursor.Y, out int? charIndex, out int? index);
                if (str == null) {
                    text[Cursor.Y].Add(new StyledString() { text = s, style = new Style { ForegroundColor = Color.White, BackgroundColor = Color.Black} });
                } else if (str.Value.style == new Style {BackgroundColor = Color.Black, ForegroundColor = Color.White}) {
                    str = text[Cursor.Y][index!.Value];
                    text[Cursor.Y][index!.Value] = new StyledString {
                        text = str.Value.text.Insert(x-charIndex!.Value, s),
                        style = new Style {
                            ForegroundColor = Color.White,
                            BackgroundColor = Color.Black
                        }
                    };
                } else {
                    if (!Cursor.IsOnPartSplit(x, Cursor.Y, out int split)) {
                        split = Cursor.CreateSplit(split, x, Cursor.Y);
                    }
                    text[Cursor.Y].Insert(split, new StyledString {
                        text = s,
                        style = new Style {
                            ForegroundColor = Color.White,
                            BackgroundColor = Color.Black
                        }
                    });
                }
                Renderer.DrawLine(Cursor.Y);
                Cursor.Right();
            }
        }
        
        internal static void Setup() {
            Terminal.HideCursor = true;
            Terminal.Clear();
            Renderer.Init(File.Name);
        }
        internal static void Exit() {
            Terminal.Clear();
            Terminal.HideCursor = false;
        }
    }
}