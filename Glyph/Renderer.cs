using System.Net;
using System.Security.Claims;
using OxDED.Terminal;

namespace Glyph
{
    public struct StyledString {
        public Style style;
        public string text;
    }
    public static class Renderer {
        public static readonly Color[] fgColors = [Color.Green, Color.Red, Color.Blue, Color.LightRed, Color.DarkGreen, Color.DarkBlue, new(0, 255, 255), new(255, 0, 255), Color.Yellow, Color.Orange, new(128, 0, 128), Color.White, Color.Gray];
        public static readonly Color[] bgColors = [Color.Green, Color.Red, Color.Blue, Color.LightRed, Color.DarkGreen, Color.DarkBlue, new(0, 255, 255), new(255, 0, 255), Color.Yellow, Color.Orange, new(128, 0, 128), Color.Black];
        public static int FrameOffsetY {get{return 1;}}
        /// <summary>
        /// (size of row numbers)
        /// </summary>
        public static int FrameOffsetX {get{return GetLineNumber(FrameOffsetY).Length+GetSpaces(FrameOffsetY)+1;}}
        /// <summary>
        /// excluding row numbers.
        /// </summary>
        public static (uint width, uint height) FrameSize {get {return ((uint)(Terminal.Width-FrameOffsetX), (uint)(Terminal.Height-FrameOffsetY));}}
        public static void Init(string fileName) {
            Terminal.Clear();
            Terminal.Set("Glyph", (0, 0), new Style{Bold = true, foregroundColor = Color.Orange});
            Terminal.Set(fileName, (Console.WindowWidth/2+1-fileName.Length/2, 0), new Style{Bold = true, foregroundColor = Color.White});
            for (int line = FrameOffsetY; line < FrameSize.height; line++) {
                DrawLineNumber(line, false);
            }
        }
        /// <param name="y">screen coord y</param>
        public static string GetLineNumber(int y) {
            return (y+1+Scroll.Y).ToString();
        }
        /// <param name="y">screen coord y</param>
        public static int GetSpaces(int y) {
            return 1+Terminal.Height.ToString().Length-GetLineNumber(y).Length+1;
        }
        public static (int x, int y) GetScreenPos((int x, int y) pos) {
            return (FrameOffsetX+pos.x, FrameOffsetY+pos.y);
        }
        public static void DrawLineNumber(int line, bool isFilled) {
            Terminal.Set(GetLineNumber(line)+new string(' ', GetSpaces(line))+(isFilled ? '\u2503' : '\u2507'), (0, line+FrameOffsetY));
        }
        // public static List<StyledString> CreateDiff(List<StyledString> oldData, List<StyledString> newData) {
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
        public static char? GetCharacter(int x, List<StyledString> strings) {
            string text = "";
            foreach (StyledString s in strings) {
                text += s.text;
            }
            if (text.Length <= x) {
                return null;
            }
            return text[x];
        }
        public static void Draw(List<List<StyledString>> newData) {
            for (int line = FrameOffsetY; line < FrameSize.height; line++) {
                if (newData.Count > line) {
                    if (newData[line].Count < 1) {
                        DrawLineNumber(line, false);
                    } else if (newData[line][0].text.Length < 1) {
                        DrawLineNumber(line, false);
                    } else {
                        DrawLineNumber(line, true);
                    }
                    DrawLine(line, newData[line]);
                } else {
                    DrawLineNumber(line, false);
                }
            }
        }
        public static void DrawLine(int line, List<StyledString> lineData) {
            int offset = 0;
            foreach (StyledString str in lineData) {
                Terminal.Set(str.text, GetScreenPos((offset, line)), str.style);
                offset+=str.text.Length;
            }
            // draw cursor
            if (Cursor.Y == line+Scroll.Y) {
                // TODO: draw cursor.
                Terminal.Set(GetCharacter(Cursor.X+Scroll.X, lineData) ?? ' ', GetScreenPos((Cursor.X+Scroll.X, line+Scroll.Y)), new Style{backgroundColor = new Color(128, 128, 128)});
            }
            // Draw from cursor
            if (Cursor.from.Y != null && Cursor.from.X != null) {
                if (Cursor.from.Y == line+Scroll.Y) {
                    Terminal.Set(GetCharacter(Cursor.from.X.Value+Scroll.X, lineData) ?? ' ', GetScreenPos((Cursor.from.X.Value, Cursor.from.Y.Value)), new Style{backgroundColor = new Color(64, 64, 64)});
                }
            }
        }
        public static void DrawPalette(Color[] colors) {
            for (int i = 0; i < colors.Length; i++) {
                char symbol = (char)(i+'a');
                Terminal.Set($" {symbol} ", (i*3, 1), new Style{backgroundColor = colors[i], foregroundColor = Color.White});
            }
            Terminal.Set(" . ", (0, 2), new Style{backgroundColor = new(64, 64, 64), foregroundColor = Color.White});
        }
        public static void ClearPalette(List<StyledString>[] lines) {
            if (lines.Length <= 2) {
                throw new ArgumentException("Array to small (min length: 2).", nameof(lines));
            }
            if (lines[0].Count < 1) {
                DrawLineNumber(0, false);
            } else if (lines[0][0].text.Length < 1) {
                DrawLineNumber(0, false);
            } else {
                DrawLineNumber(0, true);
            }
            if (lines[1].Count < 1) {
                DrawLineNumber(1, false);
            } else if (lines[0][0].text.Length < 1) {
                DrawLineNumber(1, false);
            } else {
                DrawLineNumber(1, true);
            }
            DrawLine(0, lines[0]);
            DrawLine(1, lines[1]);
        }
        public static void DrawHexCodePalette(string? colorPaletteCode, List<StyledString>[]? lines = null) {
            if (lines!=null) { ClearPalette(lines); }
            if (colorPaletteCode==null) {return;}
            string val
                = new Color(128, 0, 0).ToBackgroundANSI()+new string([colorPaletteCode.Length >= 1 ? colorPaletteCode[0] : ' ', colorPaletteCode.Length >= 2 ? colorPaletteCode[1] : ' '])
                + new Color(0, 128, 0).ToBackgroundANSI()+new string([colorPaletteCode.Length >= 3 ? colorPaletteCode[2] : ' ', colorPaletteCode.Length >= 4 ? colorPaletteCode[3] : ' '])
                + new Color(0, 0, 128).ToBackgroundANSI()+new string([colorPaletteCode.Length >= 5 ? colorPaletteCode[4] : ' ', colorPaletteCode.Length >= 6 ? colorPaletteCode[5] : ' ']);
            Terminal.Set(val, (0, 1));
        }
        public static void ClearHexCodePalette(List<StyledString> line) {
            Terminal.Set("      ", (0, 1));
            if (line.Count < 1) {
                DrawLineNumber(0, false);
            } else if (line[0].text.Length < 1) {
                DrawLineNumber(0, false);
            } else {
                DrawLineNumber(0, true);
            }
            DrawLine(0, line);
        }
    }
}