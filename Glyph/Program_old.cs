using LambdaKit.Terminal;

namespace Glyph
{
	internal static class Program2 {
		internal static void Main2(string[] args) {
			if (args.Length != 1) {
				Console.WriteLine("Usage: Glyph [file]");
				return;
			}
			if (!System.IO.File.Exists(args[0])) {
				Console.WriteLine($"File not found: {args[0]}");
				return;
			}

			Glyph2.Load(args[0]);

			Terminal.BlockCancelKey = true;
			
			Glyph2.Setup();

			Terminal.OnKeyPress += OnCommand;
			Terminal.ListenForKeys = true; // Does not block flow
		}

		internal static void OnCommand(ConsoleKey key, char keyChar, bool alt, bool shift, bool control) {
			if (control) {
				if (key == ConsoleKey.X) {
					Glyph2.Exit();
					Terminal.ListenForKeys = false;
					return;
				}
				if (Glyph2.RGBColorPaletteState!=0) {return;}
				if (key == ConsoleKey.Z) {
					Glyph2.Save();
				} else if (key == ConsoleKey.E) {
					Glyph2.ShowRGBColorPalette();
				} else if (key == ConsoleKey.W) {
					Glyph2.ShowMarkerPalette();
				} else if (key == ConsoleKey.F) {
					Cursor2.From();
				} else if (key == ConsoleKey.B) {
					Glyph2.Bold();
				} else if (key == ConsoleKey.I) {
					Glyph2.Itallic();
				} else if (key == ConsoleKey.U) {
					Glyph2.Underline();
				}
			} else if (!alt) {
				if (Glyph2.RGBColorPaletteState!=0) {
					Glyph2.ChooseRGBColor(keyChar);
				} else if (shift) {
					if (key == ConsoleKey.UpArrow) {
						Scroll2.Update((0, -1));
					} else if (key == ConsoleKey.DownArrow) {
						Scroll2.Update((0, 1));
					} else if (key == ConsoleKey.LeftArrow) {
						Scroll2.Update((-1, 0));
					} else if (key == ConsoleKey.RightArrow) {
						Scroll2.Update((1, 0));
					} else {
						Glyph2.Type(key, keyChar, shift);
					}
				} else if (key == ConsoleKey.UpArrow) {
					Cursor2.Up();
				} else if (key == ConsoleKey.DownArrow) {
					Cursor2.Down();
				} else if (key == ConsoleKey.LeftArrow) {
					Cursor2.Left();
				} else if (key == ConsoleKey.RightArrow) {
					Cursor2.Right();
				} else {
					Glyph2.Type(key, keyChar, shift);
				}
				
			}
		}
	}
}