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
        internal static void ChooseColor(char key) {
            Color color;
            if (key=='.') {
                colorPaletteCode="";
                Renderer.ClearPalette();
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
                            Renderer.ClearPalette();
                        } else {
                            Renderer.ClearHexCodePalette();
                        }
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
                    return old.style with {ForegroundColor = color};
                });
            } else {
                Cursor.Selection((StyledString old, int X, int Y) => {
                    return old.style with {BackgroundColor = color};
                });
            }
            if (colorPaletteCode == null) {
                Renderer.ClearPalette();
            } else {
                Renderer.ClearHexCodePalette();
            }
            Cursor.from = (null, null);
            ColorPaletteState = 0;
            colorPaletteCode = null;
        }
        internal static void Bold() {
            if (Cursor.from.X == null || Cursor.from.Y == null) {return;}
            Cursor.Selection((StyledString old, int X, int Y) => {
                old = old with { style = old.style with {Bold=!old.style.Bold}};
                Renderer.DrawStyledString(old, X, Y);
                return old.style;
            });
            Renderer.DrawChar(Cursor.from.X.Value, Cursor.from.Y.Value);
            Cursor.from=(null, null);
        }
        internal static void Itallic() {
            if (Cursor.from.X == null || Cursor.from.Y == null) {return;}
            Cursor.Selection((StyledString old, int X, int Y) => {
                old = old with { style = old.style with {Italic=!old.style.Italic}};
                Renderer.DrawStyledString(old, X, Y);
                return old.style;
            });
            Renderer.DrawChar(Cursor.from.X.Value, Cursor.from.Y.Value);
            Cursor.from=(null, null);
        }
        internal static void Underline() {
            if (Cursor.from.X == null || Cursor.from.Y == null) {return;}
            Cursor.Selection((StyledString old, int X, int Y) => {
                old = old with { style = old.style with {Underline=!old.style.Underline}};
                Renderer.DrawStyledString(old, X, Y);
                return old.style;
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
                Renderer.Draw(); // TODO: optimalisation
                return;
            } else if (key == ConsoleKey.Backspace) {
                if (Cursor.X == 0) {
                    if (Cursor.Y == 0) {return;}
                    int nextX = Renderer.GetLength(Cursor.Y-1);
                    text[Cursor.Y-1].AddRange(text[Cursor.Y]);
                    int length = Renderer.GetLength(Cursor.Y);
                    text.RemoveAt(Cursor.Y);
                    for (int i = 0; i < length; i++) {
                        Renderer.DrawChar(i, Cursor.Y);
                    }
                    Renderer.DrawChar(Cursor.X, Cursor.Y);
                    Cursor.X = nextX;
                    Cursor.Y--;
                    Renderer.Draw();
                } else { // FIXME: 
                    int posInLine = 0;
                    for (int i = 0; i < text[Cursor.Y].Count; i++) {
                        StyledString str = text[Cursor.Y][i];
                        if (Cursor.X < posInLine+str.text.Length && Cursor.X >= posInLine) {
                            text[Cursor.Y][i] = str with { text = str.text.Remove(Cursor.X - posInLine, 1)} ;
                            break;
                        }
                        posInLine += str.text.Length;
                    }
                    Renderer.DrawLine(Cursor.Y);
                }
            } else if (!char.IsControl(keyChar)) {
                StyledString? str = Renderer.GetStyledStringAt(Cursor.X-1 < 0 ? 0 : Cursor.X, Cursor.Y, out int? index);
                if (str == null) {
                    text[Cursor.Y].Add(new StyledString() { text = (shift ? char.ToUpper(keyChar) : keyChar).ToString() });
                } else if (str.Value.style.Equals(default(Style))) {
                    str = text[Cursor.Y][index!.Value];
                    text[Cursor.Y][index!.Value] = new StyledString {
                        text = str.Value.text.Insert((Cursor.X-1 < 0 ? 0 : Cursor.X)-index!.Value, (shift ? char.ToUpper(keyChar) : keyChar).ToString())
                    };
                }
                Cursor.Right();
                Renderer.DrawLine(Cursor.Y);
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