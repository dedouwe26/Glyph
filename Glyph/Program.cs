namespace Glyph
{
    public static class Program
    {
        public static Glyph? currentGlyph;
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
            currentGlyph = new(args[0]);

            Console.CancelKeyPress += (sender, e) => {
                e.Cancel = true;
            };

            Console.CursorVisible = false;
            currentGlyph.Setup();
            while (!closed)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                if (!Console.IsInputRedirected) {
                    OnCommand(key);
                }
            }
            currentGlyph.Exit();
        }
        public static void OnCommand(ConsoleKeyInfo key) {
            if (key.Modifiers == ConsoleModifiers.Control) {
                if (key.Key == ConsoleKey.X) {
                    closed = currentGlyph!.Exit();
                    return;
                }
                if (currentGlyph!.colorPaletteState!=0) {return;}
                if (key.Key == ConsoleKey.Z) {
                    currentGlyph!.Save();
                } else if (key.Key == ConsoleKey.E) {
                    currentGlyph!.ShowColorPalette();
                } else if (key.Key == ConsoleKey.W) {
                    currentGlyph!.ShowMarkerPalette();
                } else if (key.Key == ConsoleKey.F) {
                    currentGlyph!.From();
                } else if (key.Key == ConsoleKey.B) {
                    currentGlyph!.Bold();
                } else if (key.Key == ConsoleKey.I) {
                    currentGlyph!.Itallic();
                } else if (key.Key == ConsoleKey.U) {
                    currentGlyph!.Underline();
                }
            }
            else if (key.Modifiers == 0 || key.Modifiers == ConsoleModifiers.Shift) {
                if (currentGlyph!.colorPaletteState!=0) {
                    currentGlyph.ChooseColor(key.KeyChar);
                } else if (key.Key == ConsoleKey.UpArrow) {
                    currentGlyph.CursorUp();
                } else if (key.Key == ConsoleKey.DownArrow) {
                    currentGlyph.CursorDown();
                } else if (key.Key == ConsoleKey.LeftArrow) {
                    currentGlyph.CursorLeft();
                } else if (key.Key == ConsoleKey.RightArrow) {
                    currentGlyph.CursorRight();
                } else {
                    currentGlyph.Type(key);
                }
                
            }
        }
    }
}