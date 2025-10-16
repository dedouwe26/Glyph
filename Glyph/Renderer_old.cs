using LambdaKit.Terminal;

namespace Glyph
{
	internal struct StyledString {
		internal Style style = new();
		internal required string text;

		public StyledString() { }
	}
	internal static class Renderer2 {
		private const char FilledLineChar = '\u2503';
		private const char EmptyLineChar  = '\u2507';
		internal static readonly RGBColor[] fgRGBColors = [RGBColor.Green, RGBColor.Red, RGBColor.Blue, RGBColor.LightRed, RGBColor.DarkGreen, RGBColor.DarkBlue, RGBColor.Cyan, RGBColor.Magenta, RGBColor.Yellow, RGBColor.Orange, new(128, 0, 128), RGBColor.White, RGBColor.Gray];
		internal static readonly RGBColor[] bgRGBColors = [RGBColor.Green, RGBColor.Red, RGBColor.Blue, RGBColor.LightRed, RGBColor.DarkGreen, RGBColor.DarkBlue, RGBColor.Cyan, RGBColor.Magenta, RGBColor.Yellow, RGBColor.Orange, new(128, 0, 128), RGBColor.Black];
		internal static int FrameOffsetY {get{ return 1; }}
		/// <summary>
		/// (size of row numbers)
		/// </summary>
		internal static int FrameOffsetX {get{return GetLineNumber(FrameOffsetY).Length+GetSpaces(FrameOffsetY)+1;}}
		/// <summary>
		/// excluding row numbers.
		/// </summary>
		internal static (uint width, uint height) FrameSize {get {return ((uint)(Terminal.Width-FrameOffsetX), (uint)(Terminal.Height-FrameOffsetY));}}
		internal static void Init(string fileName) {
			Draw();
		}
		/// <param name="y">screen coord y</param>
		internal static string GetLineNumber(int y) {
			return (y+1+Scroll2.Y).ToString();
		}
		/// <param name="y">screen coord y</param>
		internal static int GetSpaces(int y) {
			return (FrameOffsetY+Terminal.Height).ToString().Length-GetLineNumber(y-Scroll2.Y).Length+1;
		}
		internal static (int x, int y) GetScreenPos((int x, int y) pos) {
			return (FrameOffsetX+pos.x, FrameOffsetY+pos.y);
		}
		internal static void DrawLineNumber(int line, bool isFilled) {
			Terminal.Set(GetLineNumber(line)+new string(' ', GetSpaces(line+Scroll2.Y))+(isFilled ? FilledLineChar : EmptyLineChar), (0, line+FrameOffsetY));
		}
		internal static int GetLength(int line) {
			int length = 0;
			foreach (StyledString part in Glyph2.text.Count > line ? Glyph2.text[line] : []) {
				length += part.text.Length;
			}
			return length;
		}
		internal static char? GetCharacter(int x, int y) {
			StyledString? str = GetStyledStringAt(x, y, out int? i, out int? _);
			if (i == null) {
				return null;
			}
			return str?.text[x - i.Value];
		}
		internal static StyledString? GetStyledStringAt(int x, int y, out int? charIndexInLine, out int? indexInLine) {
			int i = 0;
			List<StyledString> line = Glyph2.text.Count > y ? Glyph2.text[y] : [];
			for (int i1 = 0; i1 < line.Count; i1++) {
				StyledString part = line[i1];
				i += part.text.Length;
				if (i > x) {
					charIndexInLine = i-part.text.Length;
					indexInLine = i1;
					return part;
				}
			}
			charIndexInLine = null;
			indexInLine = null;
			return null;
		}
		internal static Style? GetCharacterStyle(int x, int y) {
			return GetStyledStringAt(x, y, out int? _, out int? _)?.style;
		}
		internal static void DrawStyledString(StyledString str, int x, int y) {
			Style style = str.style;
			style.BackgroundColor = style.BackgroundColor == RGBColor.Black ? PalleteColor.Default : style.BackgroundColor;
			Terminal.Set(str.text, GetScreenPos((x, y)), style); 
		}
		internal static void DrawStyledStringAtTextCoord(StyledString str, int x, int y) {
			int offset = -Scroll2.X;
			for (int i = 0; i < x; i++) {
				offset+=Glyph2.text[y][i].text.Length;
			}
			y -= Scroll2.Y;
			if (y < 0) {
				return;
			}
			if (offset < 0) {
				for (int i = offset; i < str.text.Length; i++) {
					if (!(i < 0)) {
						DrawChar(i, y);
					}
				}
			} else {
				DrawStyledString(str, offset, y);
			}
		}
		internal static void Draw() {
			for (int line = 0; line < FrameSize.height; line++) {
				if (Glyph2.text.Count > line+Scroll2.Y) {
					if (!(Glyph2.text[line+Scroll2.Y].Count < 1)) {
						if (!(Glyph2.text[line+Scroll2.Y][0].text.Length < 1)) {
							DrawLine(line);
						}
					}
					DrawLineNumber(line, true);
				} else {
					DrawLineNumber(line, false);
				}
			}
			DrawCursor();
			DrawFromCursor();
		}
		internal static void DrawLine(int line) {
			int offset = -Scroll2.X;
			List<StyledString> lineList = Glyph2.text.Count > line+Scroll2.Y ? Glyph2.text[line+Scroll2.Y] : [];
			foreach (StyledString str in lineList) {
				if (offset < 0) {
					for (int i = offset; i < str.text.Length; i++) {
						if (!(i < 0)) {
							DrawChar(i, line);
						}
					}
				} else {
					DrawStyledString(str, offset, line);
				}
				offset+=str.text.Length;
			}
		}
		internal static void DrawPalette(RGBColor[] RGBColors) {
			for (int i = 0; i < RGBColors.Length; i++) {
				char symbol = (char)(i+'a');
				Terminal.Set($" {symbol} ", (i*3, 1), new Style{BackgroundColor = RGBColors[i], ForegroundColor = RGBColor.White});
			}
			Terminal.Set(" . ", (0, 2), new Style{BackgroundColor = new RGBColor(64, 64, 64), ForegroundColor = RGBColor.White});
		}
		internal static void ClearPalette(bool fg) {
			Terminal.Set(new string(' ', (fg ? fgRGBColors : bgRGBColors).Length * 3), (0, 1));
			Terminal.Set("   ", (0, 2));
			if (Glyph2.text.Count > 0) {
				if (Glyph2.text[0].Count > 0) {
					if (Glyph2.text[0][0].text.Length > 0) {
						DrawLineNumber(0, true);
					} else {
						DrawLineNumber(0, false);
					}
				} else {
					DrawLineNumber(0, false);
				}
			} else {
				DrawLineNumber(0, false);
			}
			if (Glyph2.text.Count > 1) {
				if (Glyph2.text[1].Count > 0) {
					if (Glyph2.text[1][0].text.Length > 0) {
						DrawLineNumber(1, true);
					} else {
						DrawLineNumber(1, false);
					}
				} else {
					DrawLineNumber(1, false);
				}
			} else {
				DrawLineNumber(1, false);
			}
			DrawLine(0);
			DrawLine(1);
		}
		internal static void FullDraw() {
			for (int line = 0; line < FrameSize.height; line++) {
				if (Glyph2.text.Count > line+Scroll2.Y) {
					DrawLineNumber(line, true);
				} else {
					DrawLineNumber(line, false);
				}
				for (int x = 0; x < FrameSize.width; x++) {
					DrawChar(x, line);
				}
			}
			DrawCursor();
			DrawFromCursor();
		}
		internal static void DrawHexCodePalette(string? RGBColorPaletteCode) {
			if (RGBColorPaletteCode==null) {return;}
			string val
				= new RGBColor(128, 0, 0).ToBackgroundANSI()+new string([RGBColorPaletteCode.Length >= 1 ? RGBColorPaletteCode[0] : ' ', RGBColorPaletteCode.Length >= 2 ? RGBColorPaletteCode[1] : ' '])
				+ new RGBColor(0, 128, 0).ToBackgroundANSI()+new string([RGBColorPaletteCode.Length >= 3 ? RGBColorPaletteCode[2] : ' ', RGBColorPaletteCode.Length >= 4 ? RGBColorPaletteCode[3] : ' '])
				+ new RGBColor(0, 0, 128).ToBackgroundANSI()+new string([RGBColorPaletteCode.Length >= 5 ? RGBColorPaletteCode[4] : ' ', RGBColorPaletteCode.Length >= 6 ? RGBColorPaletteCode[5] : ' ']);
			Terminal.Set(val, (0, 1));
		}
		internal static void ClearHexCodePalette() {
			Terminal.Set("      ", (0, 1));
			if (Glyph2.text.Count > 0) {
				if (Glyph2.text[Scroll2.Y].Count > 1) {
					if (Glyph2.text[Scroll2.Y][1].text.Length > 1) {
						DrawLineNumber(1, true);
					} else {
						DrawLineNumber(1, false);
					}
				} else {
					DrawLineNumber(1, false);
				}
			} else {
				DrawLineNumber(1, false);
			}
			DrawChar(0, 1);
			DrawChar(1, 1);
			DrawChar(2, 1);
			DrawChar(3, 1);
			DrawChar(4, 1);
			DrawChar(5, 1);
		}
		internal static void DrawChar(int x, int y) {
			Style style = GetCharacterStyle(x+Scroll2.X, y+Scroll2.Y) ?? new Style();
			style.BackgroundColor = style.BackgroundColor == RGBColor.Black ? PalleteColor.Default : style.BackgroundColor;
			Terminal.Set(GetCharacter(x+Scroll2.X, y+Scroll2.Y) ?? ' ', GetScreenPos((x, y)), style);
		}
		internal static void DrawFromCursor() {
			if (Cursor2.from.Y != null && Cursor2.from.X != null) {
				Terminal.Set(GetCharacter(Cursor2.from.X.Value, Cursor2.from.Y.Value) ?? ' ', GetScreenPos((Cursor2.from.X.Value-Scroll2.X, Cursor2.from.Y.Value-Scroll2.Y)), new Style{BackgroundColor = Cursor2.FromCursorRGBColor});
			}
		}
		internal static void DrawCursor() {
			Terminal.Set(GetCharacter(Cursor2.X, Cursor2.Y) ?? ' ', GetScreenPos((Cursor2.X-Scroll2.X, Cursor2.Y-Scroll2.Y)), new Style{BackgroundColor = Cursor2.CursorRGBColor});
		}
	}
}