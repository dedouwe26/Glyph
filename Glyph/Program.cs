using OxDED.Terminal;

namespace Glyph
{
    internal static class Program {
        internal static void Main(string[] args) {
            if (args.Length != 1) {
                Console.WriteLine("Usage: Glyph [file]");
                return;
            }
            if (!System.IO.File.Exists(args[0])) {
                Console.WriteLine($"File not found: {args[0]}");
                return;
            }

            Glyph.Load(args[0]);

            Terminal.blockCancelKey = true;
            
            Glyph.Setup();

            Terminal.OnKeyPress += OnCommand;
            Terminal.ListenForKeys = true; // Does not block flow
        }

        internal static void OnCommand(ConsoleKey key, char keyChar, bool alt, bool shift, bool control) {
            if (control) {
                if (key == ConsoleKey.X) {
                    Glyph.Exit();
                    Terminal.ListenForKeys = false;
                    return;
                }
                if (Glyph.ColorPaletteState!=0) {return;}
                if (key == ConsoleKey.Z) {
                    Glyph.Save();
                } else if (key == ConsoleKey.E) {
                    Glyph.ShowColorPalette();
                } else if (key == ConsoleKey.W) {
                    Glyph.ShowMarkerPalette();
                } else if (key == ConsoleKey.F) {
                    Cursor.From();
                } else if (key == ConsoleKey.B) {
                    Glyph.Bold();
                } else if (key == ConsoleKey.I) {
                    Glyph.Itallic();
                } else if (key == ConsoleKey.U) {
                    Glyph.Underline();
                }
            } else if (!alt) {
                if (Glyph.ColorPaletteState!=0) {
                    Glyph.ChooseColor(keyChar);
                } else if (shift) {
                    if (key == ConsoleKey.UpArrow) {
                        Scroll.Update((0, -1));
                    } else if (key == ConsoleKey.DownArrow) {
                        Scroll.Update((0, 1));
                    } else if (key == ConsoleKey.LeftArrow) {
                        Scroll.Update((-1, 0));
                    } else if (key == ConsoleKey.RightArrow) {
                        Scroll.Update((1, 0));
                    } else {
                        Glyph.Type(key, keyChar, shift);
                    }
                } else if (key == ConsoleKey.UpArrow) {
                    Cursor.Up();
                } else if (key == ConsoleKey.DownArrow) {
                     Cursor.Down();
                } else if (key == ConsoleKey.LeftArrow) {
                     Cursor.Left();
                } else if (key == ConsoleKey.RightArrow) {
                     Cursor.Right();
                } else {
                    Glyph.Type(key, keyChar, shift);
                }
                
            }
        }
    }
}