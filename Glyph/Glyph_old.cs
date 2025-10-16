using System.Globalization;
using LambdaKit.Terminal;

namespace Glyph
{
	internal static class Glyph2 {
		internal static byte RGBColorPaletteState = 0;
		private static string? RGBColorPaletteCode = null;
		private static File2? file;
		internal static File2 File { get { return file ?? throw new Exception("No file loaded"); } }
		private static Timer? timer;
		internal static List<List<StyledString>> text = [];
		internal static void Load(string path) {
			file = new(path);
			text = File.Parse();
		}
		internal static void Save() {
			File.Write(text);
				Terminal.Set("Saved", (0, 0), new Style{ForegroundColor = RGBColor.Orange, BackgroundColor=new RGBColor(1, 16, 41), Bold=true});
			timer = new((_) => {
				timer!.Dispose();
				Terminal.Set("Glyph", (0, 0), new Style{ForegroundColor = RGBColor.Orange, BackgroundColor=new RGBColor(1, 16, 41), Bold=true});
			}, null, TimeSpan.FromSeconds(2), TimeSpan.FromMilliseconds(-1));
		}
		internal static void ShowRGBColorPalette() {
			if (Cursor2.from==(null, null)) { return; }
			RGBColorPaletteState = 1;
			Renderer2.DrawPalette(Renderer2.fgRGBColors);
		}
		internal static void ShowMarkerPalette() {
			if (Cursor2.from==(null, null)) { return; }
			RGBColorPaletteState = 2;
			Renderer2.DrawPalette(Renderer2.bgRGBColors);
		}
		internal static void ChooseRGBColor(char key) { // TODO: fix text RGBColoring > 9: 1 less.
			RGBColor RGBColor;
			if (key=='.') {
				RGBColorPaletteCode="";
				Renderer2.ClearPalette(RGBColorPaletteState==1);
				Renderer2.DrawHexCodePalette(RGBColorPaletteCode);
				return;
			} else {
				if (RGBColorPaletteCode==null) {
					RGBColor = RGBColorPaletteState==1 ? Renderer2.fgRGBColors[key-'a'] : Renderer2.bgRGBColors[key-'a'];
				} else {
					RGBColorPaletteCode += key;
					if (RGBColorPaletteCode.Length != 6) { Renderer2.DrawHexCodePalette(RGBColorPaletteCode); return; }
					try {
						RGBColor = new(byte.Parse(RGBColorPaletteCode[..2], NumberStyles.HexNumber), byte.Parse(RGBColorPaletteCode.Substring(2, 2), NumberStyles.HexNumber), byte.Parse(RGBColorPaletteCode.Substring(4, 2), NumberStyles.HexNumber));
					}
					catch (FormatException) {
						if (RGBColorPaletteCode == null) {
							Renderer2.ClearPalette(RGBColorPaletteState==1);
						} else {
							Renderer2.ClearHexCodePalette();
						}
						Renderer2.DrawChar(Cursor2.from.X!.Value, Cursor2.from.Y!.Value);
						Cursor2.from = (null, null);
						RGBColorPaletteState = 0;
						RGBColorPaletteCode = null;
						return;
					}
					
					RGBColorPaletteCode = null;
				}
			}
			if (RGBColorPaletteState == 1) {
				Cursor2.Selection((StyledString old, int X, int Y) => {
					old.style.ForegroundColor = RGBColor;
					Renderer2.DrawStyledStringAtTextCoord(old, X, Y);
				});
			} else {
				Cursor2.Selection((StyledString old, int X, int Y) => {
					old.style.BackgroundColor = RGBColor;
					Renderer2.DrawStyledStringAtTextCoord(old, X, Y);
				});
			}
			if (RGBColorPaletteCode == null) {
				Renderer2.ClearPalette(RGBColorPaletteState==1);
			} else {
				Renderer2.ClearHexCodePalette();
			}
			Renderer2.DrawChar(Cursor2.from.X!.Value, Cursor2.from.Y!.Value);
			Cursor2.from = (null, null);
			RGBColorPaletteState = 0;
			RGBColorPaletteCode = null;
		}
		internal static void Bold() {
			if (Cursor2.from.X == null || Cursor2.from.Y == null) {return;}
			Cursor2.Selection((StyledString old, int X, int Y) => {
				old.style.Bold = !old.style.Bold;
				Renderer2.DrawStyledStringAtTextCoord(old, X, Y);
			});
			Renderer2.DrawChar(Cursor2.from.X.Value, Cursor2.from.Y.Value);
			Cursor2.from=(null, null);
		}
		internal static void Itallic() {
			if (Cursor2.from.X == null || Cursor2.from.Y == null) {return;}
			Cursor2.Selection((StyledString old, int X, int Y) => {
				old.style.Italic = !old.style.Italic;
				Renderer2.DrawStyledStringAtTextCoord(old, X, Y);
			});
			Renderer2.DrawChar(Cursor2.from.X.Value, Cursor2.from.Y.Value);
			Cursor2.from=(null, null);
		}
		internal static void Underline() {
			if (Cursor2.from.X == null || Cursor2.from.Y == null) {return;}
			Cursor2.Selection((StyledString old, int X, int Y) => {
				old.style.Underline = !old.style.Underline;
				Renderer2.DrawStyledStringAtTextCoord(old, X, Y);
			});
			Renderer2.DrawChar(Cursor2.from.X.Value, Cursor2.from.Y.Value);
			Cursor2.from=(null, null);
		}
		internal static void Type(ConsoleKey key, char keyChar, bool shift) {
			if (key == ConsoleKey.Escape) {
				if (Cursor2.from.X == null || Cursor2.from.Y == null) {return;}
				Renderer2.DrawChar(Cursor2.from.X.Value, Cursor2.from.Y.Value);
				Cursor2.from = (null, null);
				return;
			} else if (key == ConsoleKey.Enter) {
				Cursor2.NewLine();
				return;
			} else if (key == ConsoleKey.Backspace) { 
				if (Cursor2.X == 0) {
					if (Cursor2.Y == 0) {return;}
					int nextX = Renderer2.GetLength(Cursor2.Y-1);
					text[Cursor2.Y-1].AddRange(text[Cursor2.Y]);
					text.RemoveAt(Cursor2.Y);
					if (text.Count > Cursor2.Y+1) {
						Renderer2.FullDraw();
					} else {
						Renderer2.Draw();
					}
					Cursor2.UpdateCursor((nextX-Cursor2.X, -1));
				} else {
					int x = Cursor2.X - 1;
					int posInLine = 0;
					for (int i = 0; i < text[Cursor2.Y].Count; i++) {
						StyledString str = text[Cursor2.Y][i];
						if (x < posInLine+str.text.Length && x >= posInLine) {
							if (str.text.Length == 1) {
								text[Cursor2.Y].RemoveAt(i);
							} else {
								text[Cursor2.Y][i] = str with { text = str.text.Remove(x - posInLine, 1)};
							}
							break;
						}
						posInLine += str.text.Length;
					}
					Renderer2.DrawLine(Cursor2.Y);
					Renderer2.DrawChar(Renderer2.GetLength(Cursor2.Y)-Scroll2.X, Cursor2.Y);
					Cursor2.Left();
				}
			} else if (!char.IsControl(keyChar)) { // FIXME: make it add to previous.
				int x = Cursor2.X-1 < 0 ? 0 : Cursor2.X;
				string s = (shift ? char.ToUpper(keyChar) : keyChar).ToString();
				StyledString? str = Renderer2.GetStyledStringAt(x-1, Cursor2.Y, out int? charIndex, out int? index);
				if (str == null) { // here
					text[Cursor2.Y].Add(new StyledString() { text = s, style = new Style { ForegroundColor = RGBColor.White, BackgroundColor = RGBColor.Black} });
				} else if (str.Value.style == new Style {BackgroundColor = RGBColor.Black, ForegroundColor = RGBColor.White}||str.Value.style == new Style {BackgroundColor = PalleteColor.Default, ForegroundColor = RGBColor.White}) {
					str = text[Cursor2.Y][index!.Value];
					text[Cursor2.Y][index!.Value] = new StyledString {
						text = str.Value.text.Insert(x-charIndex!.Value, s),
						style = new Style {
							ForegroundColor = RGBColor.White,
							BackgroundColor = RGBColor.Black
						}
					};
				} else {
					if (!Cursor2.IsOnPartSplit(x, Cursor2.Y, out int split)) {
						split = Cursor2.CreateSplit(split, x, Cursor2.Y);
					}
					text[Cursor2.Y].Insert(split, new StyledString {
						text = s,
						style = new Style {
							ForegroundColor = RGBColor.White,
							BackgroundColor = RGBColor.Black
						}
					});
				}
				Renderer2.DrawLine(Cursor2.Y);
				Cursor2.Right();
			}
		}
		
		internal static void Setup() {
			Terminal.HideCursor = true;
			Terminal.UseAltScreenAndSaveCursor();
			Terminal.ClearScreen();
			Terminal.ClearAll();
			Terminal.Goto((0, 0));
			Renderer2.Init(File.Name);
		}
		internal static void Exit() {
			Terminal.ClearAll();
			Terminal.ClearScreen();
			Terminal.UseNormScreenAndRestoreCursor();
			Terminal.HideCursor = false;
		}
	}
}