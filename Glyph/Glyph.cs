using System.Globalization;
using System.Security.Cryptography.X509Certificates;

namespace Glyph
{
    public delegate Character CharacterChanger (Character character, int X, int Y);
    public class Glyph
    {
        public (int X, int Y) cursor = (0, 0);	
        public (int? X, int? Y) fromCursor = (null, null);
        public (int X, int Y) scroll = (0, 0);
        public byte colorPaletteState = 0;
        public string? colorPaletteCode = null;
        public byte colorPaletteIndex = 0;
        public File file;
        private Timer? timer;
        public List<List<Character>> text;
        public Glyph(string path) {
            file = new File(path);
            text = file.Parse();
        }
        public void Save() {
            file.Write(text);
            for (int i = 0; i < "Saved".Length; i++) {
                Renderer.Set(new Character{character="Saved"[i], fg= Color.Orange, bg=new Color(1, 16, 41)}, i, 0);
            }
            timer = new((ignore) => {
                timer!.Dispose();
                for (int i = 0; i < "Glyph".Length; i++) {
                    Renderer.Set(new Character{character="Glyph"[i], fg= Color.Orange, bg=new Color(1, 16, 41)}, i, 0);
                }
            }, null, TimeSpan.FromSeconds(2), TimeSpan.FromMilliseconds(-1));
        }
        public void ShowColorPalette() {
            colorPaletteState = 1;
            for (int i = 0; i < Color.fgColors.Length; i++) {
                char symbol = (char)(i+'a');
                Renderer.Set([new Character{character=' ',bg=Color.fgColors[i]}, new Character{character=symbol,bg=Color.fgColors[i]}, new Character{character=' ',bg=Color.fgColors[i]}], 0, 1);
                Renderer.Set(new Character{character='.',bg=Color.DarkGray}, 0, 2);
            }
        }
        public void ShowMarkerPalette() {
            colorPaletteState = 2;

            for (int i = 0; i < Color.bgColors.Length; i++) {
                char symbol = (char)(i+'a');
                Renderer.Set([new Character{character=' ',bg=Color.bgColors[i]}, new Character{character=symbol,bg=Color.bgColors[i]}, new Character{character=' ',bg=Color.bgColors[i]}], 0, 1);
                Renderer.Set(new Character{character='.',bg=Color.DarkGray}, 0, 2);
            }
        }
        public void ShowHexCodePalette() {
            if (colorPaletteCode==null) {return;}
            Renderer.Set([
                new Character{character=colorPaletteCode.Length >= 1 ? colorPaletteCode[0] : ' ',bg=Color.DarkRed}, new Character{character=colorPaletteCode.Length >= 2 ? colorPaletteCode[1] : ' ',bg=Color.DarkRed}, 
                new Character{character=colorPaletteCode.Length >= 3 ? colorPaletteCode[2] : ' ',bg=Color.DarkGreen}, new Character{character=colorPaletteCode.Length >= 4 ? colorPaletteCode[3] : ' ',bg=Color.DarkGreen}, 
                new Character{character=colorPaletteCode.Length >= 5 ? colorPaletteCode[4] : ' ',bg=Color.DarkBlue}, new Character{character=colorPaletteCode.Length >= 6 ? colorPaletteCode[5] : ' ',bg=Color.DarkBlue}
            ], 0, 1);
        }
        public void ChooseColor(char key) {
            if (fromCursor == (null, null)) { colorPaletteState = 0; Draw(); return; }
            Color color;
            if (key=='.') {
                colorPaletteCode="      ";
                ShowHexCodePalette();
                return;
            } else {
                if (colorPaletteCode==null) {
                    color = colorPaletteState==1 ? Color.fgColors[key-'a'] : Color.bgColors[key-'a'];
                } else {
                    colorPaletteCode += key;
                    colorPaletteIndex++;
                    if (colorPaletteCode.Length != 6) { ShowHexCodePalette(); return; }
                    color = new(byte.Parse(colorPaletteCode[..2], NumberStyles.HexNumber), byte.Parse(colorPaletteCode.Substring(2, 2), NumberStyles.HexNumber), byte.Parse(colorPaletteCode.Substring(4, 2), NumberStyles.HexNumber));
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
            Draw(); // Maybe optimise?
            colorPaletteState = 0;
            colorPaletteCode = null;
            colorPaletteIndex = 0;
        }
        public void Selection(CharacterChanger changer) {
            if (fromCursor.X == null || fromCursor.Y == null) {return;}
            int x=fromCursor.X.Value;
            int y=fromCursor.Y.Value;
            while (x!=cursor.X && y!=cursor.Y) {
                text[y][x] = changer.Invoke(text[y][x], x, y);
                if (x == text[y].Count-1) {
                    x = 0;
                    y++;
                } else {
                    x++;
                }
            }
        }
        public void Bold() {
            Selection((Character character, int X, int Y) => {
                Renderer.Set(character with {bold=!character.bold}, 0, 1);
                return character with {bold=!character.bold};
            });
        }
        public void Itallic() {
            Selection((Character character, int X, int Y) => character with {itallic=!character.itallic});
        }
        public void Underline() {
            Selection((Character character, int X, int Y) => character with {underlined=!character.underlined});
        }
        public void Type(ConsoleKeyInfo key) {
            if (key.Key == ConsoleKey.Escape) {
                if (fromCursor.X == null || fromCursor.Y == null) {return;}
                Renderer.Set(text[fromCursor.Y.Value][fromCursor.X.Value], 1+Console.WindowHeight.ToString().Length-(fromCursor.Y.Value+1).ToString().Length+1+(fromCursor.Y.Value+1).ToString().Length+fromCursor.X.Value, fromCursor.Y.Value+1);
                fromCursor = (null, null);
                return;
            } else if (key.Key == ConsoleKey.Enter) {
                List<Character> newLine = text[cursor.Y].Skip(cursor.X).Take(text[cursor.Y].Count - cursor.X).ToList();
                text[cursor.Y].RemoveRange(cursor.X, text[cursor.Y].Count-cursor.X);
                text.Insert(cursor.Y+1, newLine);
                cursor.X = 0;
                cursor.Y++;
                Draw();
                return;
            } else if (key.Key == ConsoleKey.Backspace) {
                if (cursor.X == 0) {
                    if (cursor.Y == 0) {return;}
                    int nextY = text[cursor.Y-1].Count;
                    text[cursor.Y-1].AddRange(text[cursor.Y]);
                    text.RemoveAt(cursor.Y);
                    cursor.X = nextY;
                    cursor.Y--;
                    Draw();
                } else {
                    text[cursor.Y].RemoveAt(cursor.X-1);
                    cursor.X--;
                    UpdateLine(cursor.Y);
                }
            }
            if (!char.IsControl(key.KeyChar)) {
                text[cursor.Y].Insert(cursor.X-1 < 0 ? 0 : cursor.X, new Character{character=key.Modifiers==ConsoleModifiers.Shift ? char.ToUpper(key.KeyChar) : key.KeyChar});
                CursorRight();
                UpdateLine(cursor.Y);
            }
        }

        public void UpdateScroll() {
            if (false) {
                Draw();
            } else {

            }
        }
        public void UpdateLine(int y) {
            int spaces = Console.WindowHeight.ToString().Length-(y+1).ToString().Length+1;
            int offsetX = 1+spaces+(y+1).ToString().Length;
            for (int j = 0; j < offsetX; j++) {
                Renderer.Set(new Character{character=((y+1).ToString()+new string(' ', spaces)+(text.Count >= y+1 ? "\u2503" : "\u2507"))[j], fg=Color.Gray, bg=new Color(1, 16, 41)}, j, y+1);
            }
            for (int j = 0; j < Console.WindowWidth-offsetX; j++) {
                
                if (text.Count > y) {
                    if (text[y].Count > j) {
                        Renderer.Set(text[y][j], j+offsetX, y+1);
                    } else {
                        Renderer.Set(new Character{character=' ', fg=Color.White}, j+offsetX, y+1);
                    }
                } else {
                    Renderer.Set(new Character{character=' '}, j+offsetX, y+1);
                }
            }
            if (cursor.Y==y) {
                int cursorOffset = 1+Console.WindowHeight.ToString().Length-(cursor.Y+1).ToString().Length+1+(cursor.Y+1).ToString().Length;
                if (cursor.Y > text.Count-1) {
                    Renderer.Set(new Character{character=' ', bg=Color.Gray}, cursorOffset+cursor.X, cursor.Y+1);
                } else if (cursor.X > text[cursor.Y].Count-1) {
                    Renderer.Set(new Character{character=' ', bg=Color.Gray}, cursorOffset+cursor.X, cursor.Y+1);
                } else {
                    Renderer.Set(new Character{character=text[cursor.Y][cursor.X].character, fg=Color.White, bg=Color.Gray}, cursorOffset+cursor.X, cursor.Y+1);
                }
            }
            if (fromCursor.Y != null && fromCursor.X != null) {
                if (fromCursor.Y == y) {
                    Renderer.Set(new Character{character=text[fromCursor.Y.Value][fromCursor.X.Value].character, bg=Color.DarkGray, fg=Color.White}, 1+Console.WindowHeight.ToString().Length-(fromCursor.Y.Value+1).ToString().Length+1+(fromCursor.Y.Value+1).ToString().Length+fromCursor.X.Value, fromCursor.Y.Value+1);
                }
            }
        }
        public void UpdateCursor((short X, short Y) offset) {
            if (cursor.Y+offset.Y >= text.Count) { return; }

            Console.SetCursorPosition((cursor.Y+1).ToString().Length+2+cursor.X, cursor.Y+1);
            if (fromCursor.Y != null && fromCursor.X != null) {
                if (cursor.X < fromCursor.X.Value || cursor.Y < fromCursor.Y.Value) {
                    Renderer.Set(text[fromCursor.Y.Value][fromCursor.X.Value], 1+Console.WindowHeight.ToString().Length-(fromCursor.Y.Value+1).ToString().Length+1+(fromCursor.Y.Value+1).ToString().Length+fromCursor.X.Value, fromCursor.Y.Value+1);
                    fromCursor = (null, null);
                }
            }
            int cursorOffset = 1+Console.WindowHeight.ToString().Length-(cursor.Y+1).ToString().Length+1+(cursor.Y+1).ToString().Length;
            if (cursor==fromCursor) {
                if (fromCursor.Y != null && fromCursor.X != null) {
                    Renderer.Set(new Character{character=text[fromCursor.Y.Value][fromCursor.X.Value].character, bg=Color.DarkGray, fg=Color.White}, cursor.X+cursorOffset, cursor.Y+1);
                }
            } else if (cursor.Y > text.Count-1) {
                Renderer.Set(new Character{character=' ', fg=Color.White}, cursor.X+cursorOffset, cursor.Y+1);
            } else if (cursor.X > text[cursor.Y].Count-1) {
                Renderer.Set(new Character{character=' ', fg=Color.White}, cursor.X+cursorOffset, cursor.Y+1);
            } else {
                Renderer.Set(text[cursor.Y][cursor.X], cursorOffset+cursor.X, cursor.Y+1);
            }
            if (cursor.X+offset.X >= text[cursor.Y+offset.Y].Count) {
                cursor.X=text[cursor.Y+offset.Y].Count;
            } else {
                cursor.X += offset.X;
            }
            cursor.Y += offset.Y;
            cursorOffset = 1+Console.WindowHeight.ToString().Length-(cursor.Y+1).ToString().Length+1+(cursor.Y+1).ToString().Length;
            if (cursor.Y > text.Count-1) {
                Renderer.Set(new Character{character=' ', bg=Color.Gray}, cursorOffset+cursor.X, cursor.Y+1);
            } else if (cursor.X > text[cursor.Y].Count-1) {
                Renderer.Set(new Character{character=' ', bg=Color.Gray}, cursorOffset+cursor.X, cursor.Y+1);
            } else {
                Renderer.Set(new Character{character=text[cursor.Y][cursor.X].character, fg=Color.White, bg=Color.Gray}, cursorOffset+cursor.X, cursor.Y+1);
            }
        }
        public void CursorLeft() {
            if (cursor.X == 0) { return; }
            UpdateCursor((-1, 0));
            UpdateScroll();
        }
        public void CursorRight() {
            if (cursor.X >= text[cursor.Y].Count) { return; }
            UpdateCursor((1, 0));
            UpdateScroll();
        }
        public void CursorUp() {
            if (cursor.Y == 0) { return; }
            UpdateCursor((0, -1));
            UpdateScroll();
        }
        public void CursorDown() {
            if (cursor.Y+1 >= text.Count) { return; }
            // if (cursor.X-1 >= text[cursor.Y+1].Count) { return; }
            UpdateCursor((0, 1));
            UpdateScroll();
        }
        public void Setup() {
            Console.SetCursorPosition(0, 0);
            Console.Clear();
            if (file==null) { throw new Exception("No file loaded"); }
            for (int i = 0; i < Console.WindowWidth; i++) {
                if (i < 5) {
                    Renderer.Set(new Character{character="Glyph"[i], bg=new Color(1, 16, 41), fg=Color.Orange, bold=true}, i, 0);
                }
                else if (i==Console.WindowWidth/2+1-file.Name.Length/2 && i < file.Name.Length) {
                    Renderer.Set(new Character{character=file.Name[i-(Console.WindowWidth/2+1-file.Name.Length/2)], bg=new Color(1, 16, 41)}, i, 0);
                } else {
                    Renderer.Set(new Character{character=' ', bg=new Color(1, 16, 41)}, i, 0);
                }
            }
            Draw();
            Console.SetCursorPosition(Console.WindowWidth-1, Console.WindowHeight-1);
            Console.Write(Styles.Reset);

        }
        public void Draw() {
            for (int i = 0; i < Console.WindowHeight-1; i++) {
                UpdateLine(i);
            }
        }
        public void From() {
            if (cursor.Y > text.Count-1 || cursor.Y < 0) {return;}
            if (cursor.X > text[cursor.Y].Count-1 || cursor.X < 0) {return;}
            if (fromCursor.Y != null && fromCursor.X != null) {
                Renderer.Set(text[fromCursor.Y.Value][fromCursor.X.Value], 1+Console.WindowHeight.ToString().Length-(fromCursor.Y.Value+1).ToString().Length+1+(fromCursor.Y.Value+1).ToString().Length+fromCursor.X.Value, fromCursor.Y.Value+1);
            }
            fromCursor = cursor;
            Renderer.Set(new Character{character=text[fromCursor.Y.Value][fromCursor.X.Value].character, bg=Color.DarkGray, fg=Color.White}, 1+Console.WindowHeight.ToString().Length-(fromCursor.Y.Value+1).ToString().Length+1+(fromCursor.Y.Value+1).ToString().Length+fromCursor.X.Value, fromCursor.Y.Value+1);
        }
        public bool Exit() {
            Console.CursorVisible = true;
            Console.SetCursorPosition(0, 0);
            Console.ResetColor();
            Console.Write(Styles.Reset);
            Console.Clear();
            return true;
        }
    }
}