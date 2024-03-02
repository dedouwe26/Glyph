using System.Globalization;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.Serialization;

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
            timer = new((_) => {
                timer!.Dispose();
                for (int i = 0; i < "Glyph".Length; i++) {
                    Renderer.Set(new Character{character="Glyph"[i], bold=true, fg= Color.Orange, bg=new Color(1, 16, 41)}, i, 0);
                }
            }, null, TimeSpan.FromSeconds(2), TimeSpan.FromMilliseconds(-1));
        }
        public void ShowColorPalette() {
            if (fromCursor==(null, null)) { return; }
            colorPaletteState = 1;
            for (int i = 0; i < Color.fgColors.Length; i++) {
                char symbol = (char)(i+'a');
                Renderer.Set([new Character{character=' ',bg=Color.fgColors[i]}, new Character{character=symbol,bg=Color.fgColors[i]}, new Character{character=' ',bg=Color.fgColors[i]}], i*3, 1);
            }
            Renderer.Set(new Character{character='.',bg=Color.DarkGray}, 0, 2);
        }
        public void ShowMarkerPalette() {
            if (fromCursor==(null, null)) { return; }
            colorPaletteState = 2;
            for (int i = 0; i < Color.bgColors.Length; i++) {
                char symbol = (char)(i+'a');
                Renderer.Set([new Character{character=' ',bg=Color.bgColors[i]}, new Character{character=symbol,bg=Color.bgColors[i]}, new Character{character=' ',bg=Color.bgColors[i]}], i*3, 1); 
            }
            Renderer.Set(new Character{character='.',bg=Color.DarkGray}, 0, 2);
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
                        fromCursor = (null, null);
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
            fromCursor = (null, null);
            colorPaletteState = 0;
            colorPaletteCode = null;
            Draw();
            
        }
        public void Selection(CharacterChanger changer) {
            if (fromCursor.X == null || fromCursor.Y == null) {return;}
            int x=fromCursor.X.Value;
            int y=fromCursor.Y.Value;
            while (x!=cursor.X || y!=cursor.Y) {
                text[y][x] = changer.Invoke(text[y][x], x, y);
                if (x == text[y].Count-1 && y!=cursor.Y) {
                    x = 0;
                    y++;
                } else {
                    x++;
                }
            }
        }
        public void Bold() {
            if (fromCursor.X == null || fromCursor.Y == null) {return;}
            Selection((Character character, int X, int Y) => {
                Renderer.Set(character with {bold=!character.bold}, 1+Console.WindowHeight.ToString().Length-(Y+1).ToString().Length+1+(Y+1).ToString().Length+X, Y+1);
                return character with {bold=!character.bold};
            });
            Renderer.Set(text[fromCursor.Y.Value][fromCursor.X.Value], 1+Console.WindowHeight.ToString().Length-(fromCursor.Y.Value+1).ToString().Length+1+(fromCursor.Y.Value+1).ToString().Length+fromCursor.X.Value, fromCursor.Y.Value+1);
            fromCursor=(null, null);
        }
        public void Itallic() {
            if (fromCursor.X == null || fromCursor.Y == null) {return;}
            Selection((Character character, int X, int Y) => {
                Renderer.Set(character with {itallic=!character.itallic}, 1+Console.WindowHeight.ToString().Length-(Y+1).ToString().Length+1+(Y+1).ToString().Length+X, Y+1);
                return character with {itallic=!character.itallic};
            });
            Renderer.Set(text[fromCursor.Y.Value][fromCursor.X.Value], 1+Console.WindowHeight.ToString().Length-(fromCursor.Y.Value+1).ToString().Length+1+(fromCursor.Y.Value+1).ToString().Length+fromCursor.X.Value, fromCursor.Y.Value+1);
            fromCursor=(null, null);
        }
        public void Underline() {
            if (fromCursor.X == null || fromCursor.Y == null) {return;}
            Selection((Character character, int X, int Y) => {
                Renderer.Set(character with {underlined=!character.underlined}, 1+Console.WindowHeight.ToString().Length-(Y+1).ToString().Length+1+(Y+1).ToString().Length+X, Y+1);
                return character with {underlined=!character.underlined};
            });
            Renderer.Set(text[fromCursor.Y.Value][fromCursor.X.Value], 1+Console.WindowHeight.ToString().Length-(fromCursor.Y.Value+1).ToString().Length+1+(fromCursor.Y.Value+1).ToString().Length+fromCursor.X.Value, fromCursor.Y.Value+1);
            fromCursor=(null, null);
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
                CursorDown();
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

        public bool UpdateScroll((int X, int Y) offset) {
            int height = Console.WindowHeight-1;
            int width = 1+Console.WindowHeight.ToString().Length-(cursor.Y+1).ToString().Length+1+(cursor.Y+1).ToString().Length;
            if (cursor.Y + offset.Y - scroll.Y >= height) {
                scroll.Y++;
                return true;
            } else if (cursor.Y + offset.Y - scroll.Y < 0) {
                scroll.Y--;
                return true;
            } else if (cursor.X + offset.X - scroll.X >= width) {
                scroll.X++;
                return true;
            } else if (cursor.X + offset.X - scroll.X < 0) {
                scroll.X--;
                return true;
            }
            return false;
        }
        public void UpdateLine(int y) {
            int spaces = Console.WindowHeight.ToString().Length-(y+1+scroll.Y).ToString().Length+1;
            int offsetX = 1+spaces+(y+1).ToString().Length;
            for (int j = 0; j < offsetX; j++) {
                Renderer.Set(new Character{character=((y+1+scroll.Y).ToString()+new string(' ', spaces)+(text.Count >= y+scroll.Y+1 ? "\u2503" : "\u2507"))[j], fg=Color.Gray, bg=new Color(1, 16, 41)}, j, y+1-scroll.Y);
            }
            for (int j = 0; j < Console.WindowWidth-offsetX; j++) {
                
                if (text.Count > y+scroll.Y) {
                    if (text[y+scroll.Y].Count > j) {
                        if (j+offsetX-scroll.X < 0||y+1-scroll.Y < 0) { continue; }
                        Renderer.Set(text[y+scroll.Y][j], j+offsetX-scroll.X, y+1-scroll.Y);
                    } else {
                        Renderer.Set(new Character{character=' ', fg=Color.White}, j+offsetX, y+1-scroll.Y);
                    }
                } else {
                    Renderer.Set(new Character{character=' '}, j+offsetX-scroll.X, y+1-scroll.Y);
                }
            }
            if (cursor.Y==y+scroll.Y) {
                int cursorOffset = 1+Console.WindowHeight.ToString().Length-(cursor.Y+1).ToString().Length+1+(cursor.Y+1).ToString().Length;
                if (cursor.Y > text.Count-1) {
                    Renderer.Set(new Character{character=' ', bg=Color.Gray}, cursorOffset+cursor.X-scroll.X, cursor.Y+1-scroll.Y);
                } else if (cursor.X > text[cursor.Y].Count-1) {
                    Renderer.Set(new Character{character=' ', bg=Color.Gray}, cursorOffset+cursor.X-scroll.X, cursor.Y+1-scroll.Y);
                } else {
                    Renderer.Set(new Character{character=text[cursor.Y][cursor.X].character, fg=Color.White, bg=Color.Gray}, cursorOffset+cursor.X-scroll.X, cursor.Y+1-scroll.Y);
                }
            }
            if (fromCursor.Y != null && fromCursor.X != null) {
                if (fromCursor.Y == y+scroll.Y) {
                    Renderer.Set(new Character{character=text[fromCursor.Y.Value][fromCursor.X.Value].character, bg=Color.DarkGray, fg=Color.White}, 1+Console.WindowHeight.ToString().Length-(fromCursor.Y.Value+1).ToString().Length+1+(fromCursor.Y.Value+1).ToString().Length+fromCursor.X.Value-scroll.X, fromCursor.Y.Value+1-scroll.Y);
                }
            }
        }
        public void UpdateCursor((short X, short Y) offset) {
            if (cursor.Y+offset.Y >= text.Count||cursor.Y+offset.Y < 0) { return; }
            if (cursor.X+offset.X > text[cursor.Y].Count||cursor.X+offset.X < 0) { return; }
            
            if (fromCursor.Y != null && fromCursor.X != null) {
                if (cursor.Y+offset.Y < fromCursor.Y.Value||(cursor.Y+offset.Y == fromCursor.Y.Value&&cursor.X+offset.X < fromCursor.X.Value)) {
                    Renderer.Set(text[fromCursor.Y.Value][fromCursor.X.Value], 1+Console.WindowHeight.ToString().Length-(fromCursor.Y.Value+1).ToString().Length+1+(fromCursor.Y.Value+1).ToString().Length+fromCursor.X.Value-scroll.X, fromCursor.Y.Value+1-scroll.Y);
                    fromCursor = (null, null);
                }
            }

            if (UpdateScroll(offset)) {

                Draw();
                return;
            }

            int cursorOffset = 1+Console.WindowHeight.ToString().Length-(cursor.Y+1).ToString().Length+1+(cursor.Y+1).ToString().Length;
            if (cursor==fromCursor) {
                if (fromCursor.Y != null && fromCursor.X != null) {
                    Renderer.Set(new Character{character=text[fromCursor.Y.Value][fromCursor.X.Value].character, bg=Color.DarkGray, fg=Color.White}, cursor.X+cursorOffset-scroll.X, cursor.Y+1-scroll.Y);
                }
            } else if (cursor.Y > text.Count-1) {
                Renderer.Set(new Character{character=' ', fg=Color.White}, cursor.X+cursorOffset-scroll.X, cursor.Y+1-scroll.Y);
            } else if (cursor.X > text[cursor.Y].Count-1) {
                Renderer.Set(new Character{character=' ', fg=Color.White}, cursor.X+cursorOffset-scroll.X, cursor.Y+1-scroll.Y);
            } else {
                Renderer.Set(text[cursor.Y][cursor.X], cursorOffset+cursor.X-scroll.X-scroll.X, cursor.Y+1-scroll.Y);
            }
            if (cursor.X+offset.X >= text[cursor.Y+offset.Y].Count) {
                cursor.X=text[cursor.Y+offset.Y].Count;
            } else {
                cursor.X += offset.X;
            }
            cursor.Y += offset.Y;
            cursorOffset = 1+Console.WindowHeight.ToString().Length-(cursor.Y+1).ToString().Length+1+(cursor.Y+1).ToString().Length;
            if (cursor.Y > text.Count-1) {
                Renderer.Set(new Character{character=' ', bg=Color.Gray}, cursorOffset+cursor.X-scroll.X, cursor.Y+1-scroll.Y);
            } else if (cursor.X > text[cursor.Y].Count-1) {
                Renderer.Set(new Character{character=' ', bg=Color.Gray}, cursorOffset+cursor.X-scroll.X, cursor.Y+1-scroll.Y);
            } else {
                Renderer.Set(new Character{character=text[cursor.Y][cursor.X].character, fg=Color.White, bg=Color.Gray}, cursorOffset+cursor.X-scroll.X, cursor.Y+1-scroll.Y);
            }
        }
        public void CursorLeft() {
            UpdateCursor((-1, 0));
        }
        public void CursorRight() {
            UpdateCursor((1, 0));
        }
        public void CursorUp() {
            UpdateCursor((0, -1));
        }
        public void CursorDown() {
            UpdateCursor((0, 1));
        }
        public void Setup() {
            Console.SetCursorPosition(0, 0);
            Console.Clear();
            if (file==null) { throw new Exception("No file loaded"); }
            for (int i = 0; i < Console.WindowWidth; i++) {
                if (i < 5) {
                    Renderer.Set(new Character{character="Glyph"[i], bg=new Color(1, 16, 41), fg=Color.Orange, bold=true}, i, 0);
                }
                else if (i>=Console.WindowWidth/2+1-file.Name.Length/2 && i < file.Name.Length+(Console.WindowWidth/2+1-file.Name.Length/2)) {
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