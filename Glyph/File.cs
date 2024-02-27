using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace Glyph
{
    public class File(string path)
    {
        public string Path = path;
        public string Name {get {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                return Path.Split('\\').Last();
            } else {
                return Path.Split('/').Last();
            }
        }}

        private static readonly char[] specialChars = ['{', '}', '(', ')', '[', ']', '+', '-', '\\'];

        public List<List<Character>> Parse() {
            List<List<Character>> result = [[]];
            using StreamReader stream = new(Path);
            (Color bg, Color fg, bool bold, bool itallic, bool underlined) state = (Color.Black, Color.White, false, false, false);
            char previousChar = '\0';
            bool canStyle = (previousChar=='\\' && stream.Peek()=='\\')||(stream.Peek()!='\\');
            while (stream.Peek() >= 0) {
                char currentChar = (char)stream.Read();
                if (char.IsControl(currentChar)&&currentChar!='\n') {
                    continue;
                }
                if (canStyle) {
                    if (currentChar == '+') {
                        char[] buffer = new char[6];
                        stream.Read(buffer, 0, 6);
                        state.fg = ParseColor(new string(buffer));
                        continue;
                    } else if (currentChar == '-') {
                        char[] buffer = new char[6];
                        stream.Read(buffer, 0, 6);
                        state.bg = ParseColor(new string(buffer));
                        continue;
                    } else if (currentChar == '{') {
                        state.bold = true;
                        continue;
                    } else if (currentChar == '}') {
                        state.bold = false;
                        continue;
                    } else if (currentChar == '(') {
                        state.itallic = true;
                        continue;
                    } else if (currentChar == ')') {
                        state.itallic = false;
                        continue;
                    } else if (currentChar == '[') {
                        state.underlined = true;
                        continue;
                    } else if (currentChar == ']') {
                        state.underlined = false;
                        continue;
                    }
                }
                canStyle = (previousChar=='\\' && currentChar=='\\')||(currentChar!='\\');
                if (currentChar == '\\'&&previousChar != '\\') {
                    continue;
                }
                if (currentChar == '\n') { result.Add([]); continue; }
                result[^1].Add(new Character
                {
                    character = currentChar,
                    bg = state.bg,
                    fg = state.fg,
                    bold = state.bold,
                    itallic = state.itallic,
                    underlined = state.underlined
                });

            }
            return result;
        }
        public void Write(List<List<Character>> characters) {
            using StreamWriter stream = new(Path);
            (Color bg, Color fg, bool bold, bool itallic, bool underlined) state = (Color.Black, Color.White, false, false, false);
            foreach (List<Character> line in characters) {
                foreach (Character character in line) {
                    if (character.fg != state.fg) {
                        stream.Write("+"+character.fg.ToString());
                        state.fg = character.fg;
                    } if (character.bg != state.bg) {
                        stream.Write("-"+character.bg.ToString());
                        state.bg = character.bg;
                    } if (character.bold != state.bold) {
                        stream.Write(character.bold ? '{' : '}');
                        state.bold = character.bold;
                    } if (character.itallic != state.itallic) {
                        stream.Write(character.itallic ? '(' : ')');
                        state.itallic = character.itallic;
                    } if (character.underlined != state.underlined) {
                        stream.Write(character.underlined ? '[' : ']');
                        state.underlined = character.underlined;
                    }
                    if (specialChars.Contains(character.character)) {
                        stream.Write('\\');
                    }
                    stream.Write(character.character);
                }
                stream.Write('\n');
            }
        }
        public static Color ParseColor(string color) {
            return new Color(byte.Parse(color[..2], NumberStyles.HexNumber), byte.Parse(color.Substring(2, 2), NumberStyles.HexNumber), byte.Parse(color.Substring(4, 2), NumberStyles.HexNumber));
        }
    }
}