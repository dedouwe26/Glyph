using OxDED.Terminal;

namespace Glyph
{
    internal struct StyledString {
        internal Style style;
        internal string text;
    }
    internal static class Renderer {
        internal static readonly Color[] fgColors = [Color.Green, Color.Red, Color.Blue, Color.LightRed, Color.DarkGreen, Color.DarkBlue, Color.Cyan, Color.Magenta, Color.Yellow, Color.Orange, new(128, 0, 128), Color.White, Color.Gray];
        internal static readonly Color[] bgColors = [Color.Green, Color.Red, Color.Blue, Color.LightRed, Color.DarkGreen, Color.DarkBlue, Color.Cyan, Color.Magenta, Color.Yellow, Color.Orange, new(128, 0, 128), Color.Black];
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
            Terminal.Clear();
            Terminal.Set("Glyph", (0, 0), new Style{Bold = true, ForegroundColor = Color.Orange});
            Terminal.Set(fileName, (Console.WindowWidth/2+1-fileName.Length/2, 0), new Style{Bold = true, ForegroundColor = Color.White});
            Draw();
        }
        /// <param name="y">screen coord y</param>
        internal static string GetLineNumber(int y) {
            return (y+1+Scroll.Y).ToString();
        }
        /// <param name="y">screen coord y</param>
        internal static int GetSpaces(int y) {
            return (FrameOffsetY+Terminal.Height).ToString().Length-GetLineNumber(y).Length+1;
        }
        internal static (int x, int y) GetScreenPos((int x, int y) pos) {
            return (FrameOffsetX+pos.x, FrameOffsetY+pos.y);
        }
        internal static void DrawLineNumber(int line, bool isFilled) {
            Terminal.Set(GetLineNumber(line)+new string(' ', GetSpaces(line))+(isFilled ? '\u2503' : '\u2507'), (0, line+FrameOffsetY));
        }
        internal static int GetLength(int line) {
            int length = 0;
            foreach (StyledString part in Glyph.text.Count > line ? Glyph.text[line] : []) {
                length += part.text.Length;
            }
            return length;
        }
        // internal static List<StyledString> CreateDiff(List<StyledString> oldData, List<StyledString> newData) {
        //     uint oldSize = 0;
        //     for (int i = 0; i < oldData.Count; i++) {
        //         oldSize+=Convert.ToUInt32(oldData[i].text.Length);
        //     }
        //     uint newSize = 0;
        //     for (int i = 0; i < oldData.Count; i++) {
        //         newSize+=Convert.ToUInt32(oldData[i].text.Length);
        //     }
        //     if (oldSize > newSize) {
        //         for (int i = 0; i < oldData.Count; i++) {
        //             StyledString oldStr = oldData[i];
        //             for (int j = 0; j < str.text.Length; j++) {
        //                 char oldChar = str.text[j];
        //                 char? newChar = 
        //             }
        //         }
        //     } else {
        //         // TODO: new size first
        //     }
        // }
        internal static char? GetCharacter(int x, int y) {
            StyledString? str = GetStyledStringAt(x, y, out int? i);
            if (i == null) {
                return null;
            }
            return str?.text[x - i.Value];
        }
        internal static StyledString? GetStyledStringAt(int x, int y, out int? indexInLine) {
            int i = 0;
            List<StyledString> line = Glyph.text.Count > y ? Glyph.text[y] : [];
            foreach (StyledString part in line) {
                i += part.text.Length;
                if (i > x) {
                    indexInLine = i-part.text.Length;
                    return part;
                }
            }
            indexInLine = null;
            return null;
        }
        internal static Style? GetCharacterStyle(int x, int y) {
            return GetStyledStringAt(x, y, out int? _)?.style;
        }
        internal static void DrawStyledString(StyledString str, int x, int y) {
            if (str.style.BackgroundColor == null) {
                
            }
            Terminal.Set(str.text, GetScreenPos((x, y)), str.style);
        }
        internal static void DrawTextOnly() {
            for (int line = FrameOffsetY; line < FrameSize.height; line++) {
                if (Glyph.text.Count > line) {
                    DrawLine(line);
                }
            }
        }
        internal static void Draw() {
            for (int line = 0; line < FrameSize.height-FrameOffsetY; line++) {
                if (Glyph.text.Count > line) {
                    if (!(Glyph.text[line].Count < 1)) {
                        if (!(Glyph.text[line][0].text.Length < 1)) {
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
            int offset = 0;
            List<StyledString> lineList = Glyph.text.Count > line ? Glyph.text[line] : [];
            foreach (StyledString str in lineList) {
                DrawStyledString(str, offset, line);
                offset+=str.text.Length;
            }
        }
        internal static void DrawPalette(Color[] colors) {
            for (int i = 0; i < colors.Length; i++) {
                char symbol = (char)(i+'a');
                Terminal.Set($" {symbol} ", (i*3, 1), new Style{BackgroundColor = colors[i], ForegroundColor = Color.White});
            }
            Terminal.Set(" . ", (0, 2), new Style{BackgroundColor = new(64, 64, 64), ForegroundColor = Color.White});
        }
        internal static void ClearPalette() {
            if (Glyph.text.Count > 0) {
                if (Glyph.text[0].Count > 0) {
                    if (Glyph.text[0][0].text.Length > 0) {
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
            if (Glyph.text.Count > 1) {
                if (Glyph.text[1].Count > 0) {
                    if (Glyph.text[1][0].text.Length > 0) {
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
        internal static void DrawHexCodePalette(string? colorPaletteCode) {
            if (colorPaletteCode==null) {return;}
            string val
                = new Color(128, 0, 0).ToBackgroundANSI()+new string([colorPaletteCode.Length >= 1 ? colorPaletteCode[0] : ' ', colorPaletteCode.Length >= 2 ? colorPaletteCode[1] : ' '])
                + new Color(0, 128, 0).ToBackgroundANSI()+new string([colorPaletteCode.Length >= 3 ? colorPaletteCode[2] : ' ', colorPaletteCode.Length >= 4 ? colorPaletteCode[3] : ' '])
                + new Color(0, 0, 128).ToBackgroundANSI()+new string([colorPaletteCode.Length >= 5 ? colorPaletteCode[4] : ' ', colorPaletteCode.Length >= 6 ? colorPaletteCode[5] : ' ']);
            Terminal.Set(val, (0, 1));
        }
        internal static void ClearHexCodePalette() {
            Terminal.Set("      ", (0, 1));
            if (Glyph.text.Count > 0) {
                if (Glyph.text[0].Count > 0) {
                    if (Glyph.text[0][0].text.Length > 0) {
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
            DrawChar(0, 0);
            DrawChar(1, 0);
            DrawChar(2, 0);
            DrawChar(3, 0);
            DrawChar(4, 0);
            DrawChar(5, 0);
        }
        internal static void DrawChar(int x, int y) {
            Terminal.Set(GetCharacter(x, y) ?? ' ', GetScreenPos((x, y)), GetCharacterStyle(x, y));
        }
        internal static void DrawFromCursor() {
            if (Cursor.from.Y != null && Cursor.from.X != null) {
                Terminal.Set(GetCharacter(Cursor.from.X.Value+Scroll.X, Cursor.from.Y.Value+Scroll.Y) ?? ' ', GetScreenPos((Cursor.from.X.Value, Cursor.from.Y.Value)), new Style{BackgroundColor = Cursor.FromCursorColor});
            }
        }
        internal static void DrawCursor() {
            Terminal.Set(GetCharacter(Cursor.X+Scroll.X, Cursor.Y+Scroll.Y) ?? ' ', GetScreenPos((Cursor.X+Scroll.X, Cursor.Y+Scroll.Y)), new Style{BackgroundColor = Cursor.CursorColor});
        }
    }
}