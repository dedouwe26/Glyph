using System.Globalization;
using OxDEDTerm;

namespace Glyph
{
    public delegate Character CharacterChanger (Character character, int X, int Y);
    public static class Glyph
    {
        public static byte colorPaletteState = 0;
        public static string? colorPaletteCode = null;
        public static File? file;
        private static Timer? timer;
        public static List<List<Character>> text = new();
        public static void Load(string path) {
            file = new(path);
            text = file.Parse();
        }
        public static void Save() {
            file!.Write(text);
            for (int i = 0; i < "Saved".Length; i++) {
                Renderer.Set(new Character{character="Saved"[i], fg= Color.Orange, bg=new Color(1, 16, 41)}, i, 0);
            }
            timer = new((_) => {
                timer!.Dispose();
                for (int i = 0; i < "Glyph".Length; i++) {
                    Renderer.Set(new Character{character="Glyph"[i], bold=true, fg= Color.Orange, bg=new Color(1, 16, 41)}, i, 0);
                }
            }, null, TimeSpan.FromSeconds(2), TimeSpan.FromMilliseconds(-1));
        }
        public static void ShowColorPalette() {
            if (Cursor.from==(null, null)) { return; }
            colorPaletteState = 1;
            for (int i = 0; i < Color.fgColors.Length; i++) {
                char symbol = (char)(i+'a');
                Renderer.Set([new Character{character=' ',bg=Color.fgColors[i]}, new Character{character=symbol,bg=Color.fgColors[i]}, new Character{character=' ',bg=Color.fgColors[i]}], i*3, 1);
            }
            Renderer.Set(new Character{character='.',bg=Color.DarkGray}, 0, 2);
        }
        public static void ShowMarkerPalette() {
            if (Cursor.from==(null, null)) { return; }
            colorPaletteState = 2;
            for (int i = 0; i < Color.bgColors.Length; i++) {
                char symbol = (char)(i+'a');
                Renderer.Set([new Character{character=' ',bg=Color.bgColors[i]}, new Character{character=symbol,bg=Color.bgColors[i]}, new Character{character=' ',bg=Color.bgColors[i]}], i*3, 1); 
            }
            Renderer.Set(new Character{character='.',bg=Color.DarkGray}, 0, 2);
        }
        public static void ShowHexCodePalette() {
            if (colorPaletteCode==null) {return;}
            Renderer.Set([
                new Character{character=colorPaletteCode.Length >= 1 ? colorPaletteCode[0] : ' ',bg=Color.DarkRed}, new Character{character=colorPaletteCode.Length >= 2 ? colorPaletteCode[1] : ' ',bg=Color.DarkRed}, 
                new Character{character=colorPaletteCode.Length >= 3 ? colorPaletteCode[2] : ' ',bg=Color.DarkGreen}, new Character{character=colorPaletteCode.Length >= 4 ? colorPaletteCode[3] : ' ',bg=Color.DarkGreen}, 
                new Character{character=colorPaletteCode.Length >= 5 ? colorPaletteCode[4] : ' ',bg=Color.DarkBlue}, new Character{character=colorPaletteCode.Length >= 6 ? colorPaletteCode[5] : ' ',bg=Color.DarkBlue}
            ], 0, 1);
        }
        public static void ChooseColor(char key) {
            Color color;
            if (key=='.') {
                colorPaletteCode="";
                ShowHexCodePalette();
                return;
            } else {
                if (colorPaletteCode==null) {
                    color = colorPaletteState==1 ? Color.fgColors[key-'a'] : Color.bgColors[key-'a'];
                } else {
                    colorPaletteCode += key;
                    if (colorPaletteCode.Length != 6) { ShowHexCodePalette(); return; }
                    try {
                        color = new(byte.Parse(colorPaletteCode[..2], NumberStyles.HexNumber), byte.Parse(colorPaletteCode.Substring(2, 2), NumberStyles.HexNumber), byte.Parse(colorPaletteCode.Substring(4, 2), NumberStyles.HexNumber));
                    }
                    catch (FormatException) {
                        Cursor.from = (null, null);
                        colorPaletteState = 0;
                        colorPaletteCode = null;
                        Draw();
                        return;
                    }
                    
                    colorPaletteCode = null;
                }
            }
            Selection((Character character, int X, int Y) => {
                if (colorPaletteState == 1) {
                    return character with {fg = color};
                } else {
                    return character with {bg = color};
                }
            });
            Cursor.from = (null, null);
            colorPaletteState = 0;
            colorPaletteCode = null;
            Draw();
            
        }
        public static void Selection(CharacterChanger changer) {
            if (Cursor.from.X == null || Cursor.from.Y == null) {return;}
            int x=Cursor.from.X.Value;
            int y=Cursor.from.Y.Value;
            while (x!=Cursor.X || y!=Cursor.Y) {
                text[y][x] = changer.Invoke(text[y][x], x, y);
                if (x == text[y].Count-1 && y!=Cursor.Y) {
                    x = 0;
                    y++;
                } else {
                    x++;
                }
            }
        }
        public static void Bold() {
            if (Cursor.from.X == null || Cursor.from.Y == null) {return;}
            Selection((Character character, int X, int Y) => {
                Renderer.Set(character with {bold=!character.bold}, 1+Console.WindowHeight.ToString().Length-(Y+1).ToString().Length+1+(Y+1).ToString().Length+X, Y+1);
                return character with {bold=!character.bold};
            });
            Renderer.Set(text[Cursor.from.Y.Value][Cursor.from.X.Value], 1+Console.WindowHeight.ToString().Length-(Cursor.from.Y.Value+1).ToString().Length+1+(Cursor.from.Y.Value+1).ToString().Length+Cursor.from.X.Value, Cursor.from.Y.Value+1);
            Cursor.from=(null, null);
        }
        public static void Itallic() {
            if (Cursor.from.X == null || Cursor.from.Y == null) {return;}
            Selection((Character character, int X, int Y) => {
                Renderer.Set(character with {itallic=!character.itallic}, 1+Console.WindowHeight.ToString().Length-(Y+1).ToString().Length+1+(Y+1).ToString().Length+X, Y+1);
                return character with {itallic=!character.itallic};
            });
            Renderer.Set(text[Cursor.from.Y.Value][Cursor.from.X.Value], 1+Console.WindowHeight.ToString().Length-(Cursor.from.Y.Value+1).ToString().Length+1+(Cursor.from.Y.Value+1).ToString().Length+Cursor.from.X.Value, Cursor.from.Y.Value+1);
            Cursor.from=(null, null);
        }
        public static void Underline() {
            if (Cursor.from.X == null || Cursor.from.Y == null) {return;}
            Selection((Character character, int X, int Y) => {
                Renderer.Set(character with {underlined=!character.underlined}, 1+Console.WindowHeight.ToString().Length-(Y+1).ToString().Length+1+(Y+1).ToString().Length+X, Y+1);
                return character with {underlined=!character.underlined};
            });
            Renderer.Set(text[Cursor.from.Y.Value][Cursor.from.X.Value], 1+Console.WindowHeight.ToString().Length-(Cursor.from.Y.Value+1).ToString().Length+1+(Cursor.from.Y.Value+1).ToString().Length+Cursor.from.X.Value, Cursor.from.Y.Value+1);
            Cursor.from=(null, null);
        }
        public static void Type(ConsoleKeyInfo key) {
            if (key.Key == ConsoleKey.Escape) {
                if (Cursor.from.X == null || Cursor.from.Y == null) {return;}
                Renderer.Set(text[Cursor.from.Y.Value][Cursor.from.X.Value], 1+Console.WindowHeight.ToString().Length-(Cursor.from.Y.Value+1).ToString().Length+1+(Cursor.from.Y.Value+1).ToString().Length+Cursor.from.X.Value, Cursor.from.Y.Value+1);
                Cursor.from = (null, null);
                return;
            } else if (key.Key == ConsoleKey.Enter) {
                List<Character> newLine = text[Cursor.Y].Skip(Cursor.X).Take(text[Cursor.Y].Count - Cursor.X).ToList();
                text[Cursor.Y].RemoveRange(Cursor.X, text[Cursor.Y].Count-Cursor.X);
                text.Insert(Cursor.Y+1, newLine);
                Cursor.Down();
                Draw();
                return;
            } else if (key.Key == ConsoleKey.Backspace) {
                if (Cursor.X == 0) {
                    if (Cursor.Y == 0) {return;}
                    int nextY = text[Cursor.Y-1].Count;
                    text[Cursor.Y-1].AddRange(text[Cursor.Y]);
                    text.RemoveAt(Cursor.Y);
                    Cursor.X = nextY;
                    Cursor.Y--;
                    Draw();
                } else {
                    text[Cursor.Y].RemoveAt(Cursor.X-1);
                    Cursor.X--;
                    UpdateLine(Cursor.Y);
                }
            }
            if (!char.IsControl(key.KeyChar)) {
                text[Cursor.Y].Insert(Cursor.X-1 < 0 ? 0 : Cursor.X, new Character{character=key.Modifiers==ConsoleModifiers.Shift ? char.ToUpper(key.KeyChar) : key.KeyChar});
                Cursor.Right();
                UpdateLine(Cursor.Y);
            }
        }
        public static Character GetCharacter(int x, int y) { return text.Count > y ? text[y].Count > x ? text[y][x] : new Character{character=' '} : new Character{character=' '}; }
        public static int GetOffsetX(int y) { return 1+GetSpaces(y)+(y+Scroll.Y+1).ToString().Length; }
        public static void UpdateLine(int y) {
            (int X, int Y) screenPos;
            int spaces = GetSpaces(y);
            int offsetX = 1+spaces+(y+Scroll.Y+1).ToString().Length;
            for (int j = 0; j < offsetX; j++) {
                Renderer.Set(new Character{character=((y+1+Scroll.Y).ToString()+new string(' ', spaces)+(text.Count >= y+Scroll.Y+1 ? "" : ""))[j], fg=Color.Gray, bg=new Color(1, 16, 41)}, j, y+1);
            }
            for (int x = 0; x < Console.WindowWidth-offsetX; x++) {
                // To get: x=x+Scroll.x y=y+Scroll.y
                // To set: x=x+offsetX-Scroll.x y=y+1-Scroll.y
                screenPos = Scroll.GetScreenPos((x, y));
                Renderer.Set(GetCharacter(x, y), screenPos.X, screenPos.Y);
            }
            if (Cursor.Y==y+Scroll.Y&&!(Cursor.Y+1-Scroll.Y > Console.WindowHeight-1)&&!(offsetX+Cursor.X-Scroll.X > Console.WindowWidth-1)) {
                screenPos = Scroll.GetScreenPos((Cursor.X, Cursor.Y));
                Renderer.Set(new Character{character = GetCharacter(Cursor.X+Scroll.X, Cursor.Y+Scroll.Y).character, bg=Color.Gray}, screenPos.X, screenPos.Y);
            }
            if (Cursor.from.Y != null && Cursor.from.X != null) {
                if (Cursor.from.Y == y+Scroll.Y) {
                    screenPos = Scroll.GetScreenPos((Cursor.from.X.Value, Cursor.from.Y.Value));
                    Renderer.Set(new Character{character=GetCharacter(Cursor.from.X.Value, Cursor.from.Y.Value).character, bg=Color.DarkGray, fg=Color.White}, screenPos.X, screenPos.Y);
                }
            }
        }
        
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