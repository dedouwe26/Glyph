using System.Runtime.Versioning;
using OxDEDTerm;

namespace Glyph
{
    public struct StyledString {
        public Style style;
        public string text;
    }
    public static class Renderer {
        public static int FrameOffsetY {get{return 1;}}
        /// <summary>
        /// (size of row numbers)
        /// </summary>
        public static int FrameOffsetX {get{return GetLineNumber(FrameOffsetY).Length+GetSpaces(FrameOffsetY)+1;}}
        /// <summary>
        /// excluding row numbers.
        /// </summary>
        public static (uint width, uint height) FrameSize {get {return ((uint)(Terminal.Width-FrameOffsetX), (uint)(Terminal.Height-FrameOffsetY));}}
        public static void Init(File file) {
            Terminal.Clear();
            Terminal.Set("Glyph", (0, 0), new Style{Bold = true, foregroundColor = Color.Orange});
            Terminal.Set(file.Name, (Console.WindowWidth/2+1-file.Name.Length/2, 0), new Style{Bold = true, foregroundColor = Color.White});
        }
        /// <param name="y">screen coord y</param>
        public static string GetLineNumber(int y) {
            return (y+1+Scroll.Y).ToString();
        }
        /// <param name="y">screen coord y</param>
        public static int GetSpaces(int y) {
            return 1+Terminal.Height.ToString().Length-GetLineNumber(y).Length+1;
        }
        public static void DrawLineNumber(int line, bool isFilled) {
            Terminal.Set(GetLineNumber(line)+new string(' ', GetSpaces(line))+(isFilled ? '\u2503' : '\u2507'), (0, line+FrameOffsetY));
        }
        public static void Draw(List<List<StyledString>> newData, List<List<StyledString>> oldData) {
            for (int line = FrameOffsetY; line < FrameSize.height; line++) {
                if (newData[line].Count < 1) {
                    DrawLineNumber(line, false);
                } else if (newData[line][0].text.Length < 1) {
                     
                }
                
                DrawLine(line, dataDiff[line]);
            }
        }
        public static void DrawLine(int line, List<StyledString> dataDiff) {

        }
    }
}