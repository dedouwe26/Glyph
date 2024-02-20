namespace Glyph
{
    public static class Program
    {
        public static Glyph currentGlyph = new();
        public static bool closed = false;
        public static void Main(string[] args) {
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
            };
            Console.CursorVisible = false;
            currentGlyph.Setup();
            while (!closed)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                OnCommand(key);
                // Update();
                
            }
            currentGlyph.Exit();
        }

        public static void Update() {
            Console.ResetColor();
            currentGlyph.Draw();
        }
        public static void OnCommand(ConsoleKeyInfo key) {
            if (key.Modifiers == ConsoleModifiers.Control) {
                if (key.Key == ConsoleKey.X) {
                    closed = currentGlyph.Exit();
                    return;
                }
                if (currentGlyph.colorPaletteState!=0) {return;}
                if (key.Key == ConsoleKey.S) {
                    currentGlyph.Save();
                } else if (key.Key == ConsoleKey.E) {
                    currentGlyph.ShowColorPalette();
                } else if (key.Key == ConsoleKey.W) {
                    currentGlyph.ShowMarkerPalette();
                } else if (key.Key == ConsoleKey.F) {
                    currentGlyph.From();
                } else if (key.Key == ConsoleKey.D) {
                    currentGlyph.Load();
                }
            }
            else if (key.Modifiers == 0) {
                if (currentGlyph.colorPaletteState!=0) {
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