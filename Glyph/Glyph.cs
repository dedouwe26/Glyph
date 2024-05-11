using System.Globalization;
using OxDED.Terminal;

namespace Glyph
{
    public static class Glyph
    {
        public static byte colorPaletteState = 0;
        public static string? colorPaletteCode = null;
        public static File? file;
        private static Timer? timer;
        public static List<List<StyledString>> text = new();
        public static void Load(string path) {
            file = new(path);
            text = file.Parse();
        }
        public static void Save() {
            file!.Write(text);
                Terminal.Set("Saved", (0, 0), new Style{foregroundColor = Color.Orange, backgroundColor=new Color(1, 16, 41), Bold=true});
            timer = new((_) => {
                timer!.Dispose();
                Terminal.Set("Glyph", (0, 0), new Style{foregroundColor = Color.Orange, backgroundColor=new Color(1, 16, 41), Bold=true});
            }, null, TimeSpan.FromSeconds(2), TimeSpan.FromMilliseconds(-1));
        }
        public static void ShowColorPalette() {
            if (Cursor.from==(null, null)) { return; }
            colorPaletteState = 1;
            Renderer.DrawPalette(Renderer.fgColors);
        }
        public static void ShowMarkerPalette() {
            if (Cursor.from==(null, null)) { return; }
            colorPaletteState = 2;
            Renderer.DrawPalette(Renderer.bgColors);
        }
        public static void ChooseColor(char key) {
            Color color;
            if (key=='.') {
                colorPaletteCode="";
                Renderer.ClearPalette();
                Renderer.DrawHexCodePalette(colorPaletteCode);
                return;
            } else {
                if (colorPaletteCode==null) {
                    color = colorPaletteState==1 ? Renderer.fgColors[key-'a'] : Renderer.bgColors[key-'a'];
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
                        colorPaletteState = 0;
                        colorPaletteCode = null;
                        return;
                    }
                    
                    colorPaletteCode = null;
                }
            }
            if (colorPaletteState == 1) {
                Cursor.Selection((StyledString old, int X, int Y) => {
                    return old.style with {foregroundColor = color};
                });
            } else {
                Cursor.Selection((StyledString old, int X, int Y) => {
                    return old.style with {backgroundColor = color};
                });
            }
            if (colorPaletteCode == null) {
                Renderer.ClearPalette();
            } else {
                Renderer.ClearHexCodePalette();
            }
            Cursor.from = (null, null);
            colorPaletteState = 0;
            colorPaletteCode = null;
        }
        public static void Bold() {
            if (Cursor.from.X == null || Cursor.from.Y == null) {return;}
            Cursor.Selection((StyledString old, int X, int Y) => {
                old = old with { style = old.style with {Bold=!old.style.Bold}};
                Renderer.DrawStyledString(old, X, Y);
                return old.style;
            });
            Renderer.DrawChar(Cursor.from.X.Value, Cursor.from.Y.Value);
            Cursor.from=(null, null);
        }
        public static void Itallic() {
            if (Cursor.from.X == null || Cursor.from.Y == null) {return;}
            Cursor.Selection((StyledString old, int X, int Y) => {
                old = old with { style = old.style with {Italic=!old.style.Italic}};
                Renderer.DrawStyledString(old, X, Y);
                return old.style;
            });
            Renderer.DrawChar(Cursor.from.X.Value, Cursor.from.Y.Value);
            Cursor.from=(null, null);
        }
        public static void Underline() {
            if (Cursor.from.X == null || Cursor.from.Y == null) {return;}
            Cursor.Selection((StyledString old, int X, int Y) => {
                old = old with { style = old.style with {Underline=!old.style.Underline}};
                Renderer.DrawStyledString(old, X, Y);
                return old.style;
            });
            Renderer.DrawChar(Cursor.from.X.Value, Cursor.from.Y.Value);
            Cursor.from=(null, null);
        }

        public static void Type(ConsoleKey key, char keyChar, bool shift) {
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
                    int nextX = 0;
                    foreach (StyledString str in text[Cursor.Y-1]) {
                        nextX+=str.text.Length;
                    }
                    text[Cursor.Y-1].AddRange(text[Cursor.Y]);
                    text.RemoveAt(Cursor.Y);
                    Cursor.X = nextX;
                    Cursor.Y--;
                    Draw();
                } else {
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
                if () {

                }
                text[Cursor.Y].Insert(Cursor.X-1 < 0 ? 0 : Cursor.X, new Character{character=shift ? char.ToUpper(keyChar) : keyChar});
                Cursor.Right();
                Renderer.DrawLine(Cursor.Y);
            }
        }
        // public static Character GetCharacter(int x, int y) { return text.Count > y ? text[y].Count > x ? text[y][x] : new Character{character=' '} : new Character{character=' '}; }
        // public static int GetOffsetX(int y) { return 1+GetSpaces(y)+(y+Scroll.Y+1).ToString().Length; }
        // public static void UpdateLine(int y) {
        //     (int X, int Y) screenPos;
        //     int spaces = GetSpaces(y);
        //     int offsetX = 1+spaces+(y+Scroll.Y+1).ToString().Length;
        //     for (int j = 0; j < offsetX; j++) {
        //         Renderer.Set(new Character{character=((y+1+Scroll.Y).ToString()+new string(' ', spaces)+(text.Count >= y+Scroll.Y+1 ? "" : ""))[j], fg=Color.Gray, bg=new Color(1, 16, 41)}, j, y+1);
        //     }
        //     for (int x = 0; x < Console.WindowWidth-offsetX; x++) {
        //         // To get: x=x+Scroll.x y=y+Scroll.y
        //         // To set: x=x+offsetX-Scroll.x y=y+1-Scroll.y
        //         screenPos = Scroll.GetScreenPos((x, y));
        //         Renderer.Set(GetCharacter(x, y), screenPos.X, screenPos.Y);
        //     }
        //     if (Cursor.Y==y+Scroll.Y&&!(Cursor.Y+1-Scroll.Y > Console.WindowHeight-1)&&!(offsetX+Cursor.X-Scroll.X > Console.WindowWidth-1)) {
        //         screenPos = Scroll.GetScreenPos((Cursor.X, Cursor.Y));
        //         Renderer.Set(new Character{character = GetCharacter(Cursor.X+Scroll.X, Cursor.Y+Scroll.Y).character, bg=Color.Gray}, screenPos.X, screenPos.Y);
        //     }
        //     if (Cursor.from.Y != null && Cursor.from.X != null) {
        //         if (Cursor.from.Y == y+Scroll.Y) {
        //             screenPos = Scroll.GetScreenPos((Cursor.from.X.Value, Cursor.from.Y.Value));
        //             Renderer.Set(new Character{character=GetCharacter(Cursor.from.X.Value, Cursor.from.Y.Value).character, bg=Color.DarkGray, fg=Color.White}, screenPos.X, screenPos.Y);
        //         }
        //     }
        // }
        
        public static void Setup() {
            Console.SetCursorPosition(0, 0);
            Console.Clear();
            if (file==null) { throw new Exception("No file loaded"); }
            Draw();
            Console.SetCursorPosition(Console.WindowWidth-1, Console.WindowHeight-1);
            Console.Write(Styles.Reset);

        }
        public static void Draw() {
            for (int i = 0; i < Console.WindowHeight-1; i++) {
                UpdateLine(i);
            }
        }
        public static bool Exit() {
            Console.CursorVisible = true;
            Console.SetCursorPosition(0, 0);
            Console.ResetColor();
            Console.Write(Styles.Reset);
            Console.Clear();
            return true;
        }
    }
}