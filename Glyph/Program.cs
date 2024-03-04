namespace Glyph
{
    public static class Program
    {
        public static bool closed = false;
        public static void Main(string[] args) {
            if (args.Length != 1) {
                Console.WriteLine("Usage: Glyph [file]");
                return;
            }
            if (!System.IO.File.Exists(args[0])) {
                Console.WriteLine($"File not found: {args[0]}");
                return;
            }
            Glyph.Load(args[0]);

            Console.CancelKeyPress += (sender, e) => {
                e.Cancel = true;
            };

            Console.CursorVisible = false;
            Glyph.Setup();
            while (!closed)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                if (!Console.IsInputRedirected) {
                    OnCommand(key);
                }
            }
            Glyph.Exit();
        }
        public static void OnCommand(ConsoleKeyInfo key) {
            if (key.Modifiers == ConsoleModifiers.Control) {
                if (key.Key == ConsoleKey.X) {
                    closed = Glyph.Exit();
                    return;
                }
                if (Glyph.colorPaletteState!=0) {return;}
                if (key.Key == ConsoleKey.Z) {
                    Glyph.Save();
                } else if (key.Key == ConsoleKey.E) {
                    Glyph.ShowColorPalette();
                } else if (key.Key == ConsoleKey.W) {
                    Glyph.ShowMarkerPalette();
                } else if (key.Key == ConsoleKey.F) {
                    Cursor.From();
                } else if (key.Key == ConsoleKey.B) {
                    Glyph.Bold();
                } else if (key.Key == ConsoleKey.I) {
                    Glyph.Itallic();
                } else if (key.Key == ConsoleKey.U) {
                    Glyph.Underline();
                }
            }
            else if (key.Modifiers == 0 || key.Modifiers == ConsoleModifiers.Shift) {
                if (Glyph.colorPaletteState!=0) {
                    Glyph.ChooseColor(key.KeyChar);
                } else if (key.Modifiers == ConsoleModifiers.Shift) {
                    if (key.Key == ConsoleKey.UpArrow) {
                        Scroll.Update((0, -1));
                    } else if (key.Key == ConsoleKey.DownArrow) {
                        Scroll.Update((0, 1));
                    } else if (key.Key == ConsoleKey.LeftArrow) {
                        Scroll.Update((-1, 0));
                    } else if (key.Key == ConsoleKey.RightArrow) {
                        Scroll.Update((1, 0));
                    }
                } else if (key.Key == ConsoleKey.UpArrow) {
                    Cursor.Up();
                } else if (key.Key == ConsoleKey.DownArrow) {
                     Cursor.Down();
                } else if (key.Key == ConsoleKey.LeftArrow) {
                     Cursor.Left();
                } else if (key.Key == ConsoleKey.RightArrow) {
                     Cursor.Right();
                } else {
                    Glyph.Type(key);
                }
                
            }
        }
    }
}