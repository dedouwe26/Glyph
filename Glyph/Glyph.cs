using System.Text;

namespace Glyph
{
    public class Glyph {
        public static readonly List<ConsoleColor> markerColors = new() {
            ConsoleColor.Blue,
            ConsoleColor.Cyan,
            ConsoleColor.DarkBlue,
            ConsoleColor.DarkCyan,
            ConsoleColor.DarkGreen,
            ConsoleColor.DarkMagenta,
            ConsoleColor.DarkRed,
            ConsoleColor.DarkYellow,
            ConsoleColor.Green,
            ConsoleColor.Magenta,
            ConsoleColor.Red,
            ConsoleColor.Yellow,
            ConsoleColor.Black
        };
        public static readonly List<ConsoleColor> textColors = new() {
            ConsoleColor.Blue,
            ConsoleColor.Cyan,
            ConsoleColor.DarkBlue,
            ConsoleColor.DarkCyan,
            ConsoleColor.DarkGreen,
            ConsoleColor.DarkMagenta,
            ConsoleColor.DarkRed,
            ConsoleColor.DarkYellow,
            ConsoleColor.Green,
            ConsoleColor.Magenta,
            ConsoleColor.Red,
            ConsoleColor.Yellow,
            ConsoleColor.White,
            ConsoleColor.Gray
        };
        // public Selection selection = new();
        public string? copiedText = null;
        public bool exists = false;
        public (int X, int Y) cursor = (0, 0);	
        public (ConsoleColor background, ConsoleColor foreground) previousCursorColor = (ConsoleColor.Black, ConsoleColor.White);
        public (int? X, int? Y) fromCursor = (null, null);
        public (ConsoleColor background, ConsoleColor foreground) previousFromCursorColor = (ConsoleColor.Black, ConsoleColor.White);
        public string path = "";
        public string name = "the nameless glyph";
        public (int X, int Y) scroll = (0, 0);
        public int colorPaletteState = 0;
        public List<string> text = new List<string>{
            "Hi there, this is a test.",
            "This is the second line."
        };
        public Glyph() {}
        public void Save() {}
        public void Load() {}
        public void ShowColorPalette() {
            colorPaletteState = 1;

            Console.SetCursorPosition(0, 1);
            for (int i = 0; i < markerColors.Count; i++) {
                char symbol = (char)(i+97);
                Console.BackgroundColor = markerColors[i];
                Console.ForegroundColor = ConsoleColor.Black;
                Console.Write(" "+symbol+" ");
            }
            Console.ResetColor();
        }
        public void ShowMarkerPalette() {
            colorPaletteState = 2;

            Console.SetCursorPosition(0, 1);
            for (int i = 0; i < markerColors.Count; i++) {
                char symbol = (char)(i+97);
                Console.BackgroundColor = markerColors[i];
                Console.ForegroundColor = ConsoleColor.Black;
                Console.Write(" "+symbol+" ");
            }
            Console.ResetColor();
        }
        public void ChooseColor(char color) {
            if (fromCursor != (null, null)) {
                text[fromCursor.Y==null ? 0 : fromCursor.Y.Value].Insert(fromCursor.X==null ? 0 : fromCursor.X.Value, (colorPaletteState==1 ? "+" : "-")+color);
                text[fromCursor.Y==null ? 0 : fromCursor.Y.Value].Insert(fromCursor.X==null ? 0 : fromCursor.X.Value, colorPaletteState==1 ? "+m" : "-m");
            }
            colorPaletteState = 0;
            Draw();
        }
        public void Type(ConsoleKeyInfo key) {
            if (key.Key == ConsoleKey.Escape) {fromCursor = (null, null); return;}
            CursorRight();
            UpdateLine();
        }

        public void UpdateLine() {
            
        }

        public void UpdateScroll() {
            if (false) {
                Draw();
            } else {

            }
        }
        public void UpdateCursor((short X, short Y) offset) {
            Console.SetCursorPosition((cursor.Y+1).ToString().Length+2+cursor.X, cursor.Y+1);
            
            if (cursor.Y > text.Count-1) {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("#");
            } else if (cursor.X > text[cursor.Y].Length-1) {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("#");
            } else {
                Console.BackgroundColor=previousCursorColor.background;
                Console.ForegroundColor=previousCursorColor.foreground;
                Console.Write(text[cursor.Y][cursor.X]);
            }
            cursor.X += offset.X;
            cursor.Y += offset.Y;
            Console.Write(Console.ForegroundColor);
            Console.SetCursorPosition((cursor.Y+1).ToString().Length+2+cursor.X, cursor.Y+1);
            Console.Write(Console.ForegroundColor);
            if (cursor.Y > text.Count-1) {
                Console.BackgroundColor=ConsoleColor.Gray;
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("#");
            } else if (cursor.X > text[cursor.Y].Length-1) {
                Console.BackgroundColor=ConsoleColor.Gray;
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("#");
            } else {
                previousCursorColor = (Console.BackgroundColor, Console.ForegroundColor);
                Console.BackgroundColor=ConsoleColor.Gray;
                Console.ForegroundColor = ConsoleColor.White;
                
            }
        }
        public void UpdateText() {}
        public void CursorLeft() {
            if (cursor.X == 0) {return;}
            UpdateCursor((-1, 0));
            UpdateScroll();
        }
        public void CursorRight() {
            UpdateCursor((1, 0));
            UpdateScroll();
        }
        public void CursorUp() {
            if (cursor.Y == 0) {return;}
            UpdateCursor((0, -1));
            UpdateScroll();
        }
        public void CursorDown() {
            UpdateCursor((0, 1));
            UpdateScroll();
        }
        public void Setup() {
            Console.Clear();
            Console.ResetColor();
            Console.BackgroundColor=ConsoleColor.White;
            Console.ForegroundColor=ConsoleColor.Black;
            for (int i = 0; i <= Console.WindowWidth+1; i++) {
                if (i == 0) {Console.ForegroundColor = ConsoleColor.DarkYellow;Console.Write("Glyph");Console.ForegroundColor = ConsoleColor.Black;i+=5;}
                else if (i==Console.WindowWidth/2+1-name.Length/2) {
                    Console.Write(name);
                    i+=name.Length;
                } else {
                    Console.Write(" ");
                }
            }
            Draw();
        }
        public void Draw() {
            Console.SetCursorPosition(0, 1);
            int height = Console.WindowHeight-1;
            (ConsoleColor bg, ConsoleColor fg) currentColors = (ConsoleColor.Black, ConsoleColor.White);
            for (int i = 0; i < height; i++) {
                Console.BackgroundColor=ConsoleColor.Gray;
                Console.ForegroundColor=ConsoleColor.Black;
                Console.Write(i+1 + " |");
                char previousChar = '\0';
                bool canStyle = false;
                for (int j = 0; j < Console.WindowWidth-(2+(i+1).ToString().Length); j++) {
                    if (text.Count > i) {
                        if (text[i].Length > j) {
                            if (canStyle) {
                                int index = Encoding.Default.GetBytes(new char[]{text[i][j]})[0]-97;
                                if (index >= (previousChar=='+' ? textColors.Count : markerColors.Count)) {return;}
                                ConsoleColor c = previousChar=='+' ? textColors[index] : markerColors[index];
                                colorPaletteState = 0;
                                Console.BackgroundColor = c;
                            }
                            if (previousChar!='\\') {
                                if (text[i][j]=='+'||text[i][j]=='-') {
                                    canStyle = true;
                                    continue;
                                } else if (text[i][j]=='\\') {
                                    continue;
                                }
                            }
                            Console.BackgroundColor=currentColors.bg;
                            Console.ForegroundColor=currentColors.fg;
                            Console.Write(text[i][j]);
                            previousChar = text[i][j];
                        } else {
                            Console.BackgroundColor=ConsoleColor.Black;
                            Console.ForegroundColor=ConsoleColor.DarkGray;
                            Console.Write("#");
                        }
                    } else {
                        Console.BackgroundColor=ConsoleColor.Black;
                        Console.ForegroundColor=ConsoleColor.DarkGray;
                        Console.Write("#");
                    }
                }
                if (!(i+1 <= height)) {
                    Console.SetCursorPosition(0, Console.GetCursorPosition().Top+1);
                }
            }
            if (fromCursor != (null, null)) {
                Console.BackgroundColor=ConsoleColor.DarkGray;
                Console.ForegroundColor=ConsoleColor.White;
                Console.SetCursorPosition(((fromCursor.X==null ? 0 : fromCursor.X.Value)+1).ToString().Length+2+(fromCursor.X==null ? 0 : fromCursor.X.Value), (fromCursor.Y==null ? 0 : fromCursor.Y.Value)+1);
                Console.Write(text[fromCursor.Y==null ? 0 : fromCursor.Y.Value][fromCursor.X==null ? 0 : fromCursor.X.Value]);
            }
            Console.SetCursorPosition((cursor.Y+1).ToString().Length+2+cursor.X, cursor.Y+1);
            Console.BackgroundColor=ConsoleColor.Gray;
            if (cursor.Y > text.Count-1) {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("#");
            } else if (cursor.X > text[cursor.Y].Length-1) {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("#");
            } else {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(text[cursor.Y][cursor.X]);
            }
            
            Console.ResetColor();
        }
        public void From() {
            if (cursor.Y > text.Count-1 || cursor.Y < 0) {return;}
            if (cursor.X > text[cursor.Y].Length-1 || cursor.X < 0) {return;}
            if (fromCursor != (null, null)) {
                Console.SetCursorPosition(((fromCursor.X==null ? 0 : fromCursor.X.Value)+1).ToString().Length+2+(fromCursor.X==null ? 0 : fromCursor.X.Value), (fromCursor.Y==null ? 0 : fromCursor.Y.Value)+1);
                Console.ForegroundColor=previousFromCursorColor.foreground;
                Console.BackgroundColor=previousFromCursorColor.background;
                Console.Write(text[fromCursor.Y==null ? 0 : fromCursor.Y.Value][fromCursor.X==null ? 0 : fromCursor.X.Value]);
            }
            fromCursor = cursor;
            Console.SetCursorPosition(((fromCursor.X==null ? 0 : fromCursor.X.Value)+1).ToString().Length+2+(fromCursor.X==null ? 0 : fromCursor.X.Value), (fromCursor.Y==null ? 0 : fromCursor.Y.Value)+1);
            previousFromCursorColor = (Console.BackgroundColor, Console.ForegroundColor);
            Console.ForegroundColor=ConsoleColor.White;
            Console.BackgroundColor=ConsoleColor.DarkGray;
            Console.Write(text[fromCursor.Y==null ? 0 : fromCursor.Y.Value][fromCursor.X==null ? 0 : fromCursor.X.Value]);
            
            
        }
        public bool Exit() {
            Console.CursorVisible = true;
            Console.SetCursorPosition(0, 0);
            Console.ResetColor();
            Console.Clear();
            return true;
        }
    }
}