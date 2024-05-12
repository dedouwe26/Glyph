using System.Runtime.InteropServices;
using OxDED.Terminal;

namespace Glyph
{
    internal class File(string path)
    {
        internal string Path = path;
        internal string Name {get {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                return Path.Split('\\').Last();
            } else {
                return Path.Split('/').Last();
            }
        }}

        private static readonly char[] specialChars = ['{', '}', '(', ')', '[', ']', '+', '-', '\\'];

        internal List<List<StyledString>> Parse() {
            List<List<StyledString>> result = [[]];
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
                        state.fg = new Color(new string(buffer));
                        continue;
                    } else if (currentChar == '-') {
                        char[] buffer = new char[6];
                        stream.Read(buffer, 0, 6);
                        state.bg = new Color(new string(buffer));
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

                Style style = new() { BackgroundColor = state.bg, ForegroundColor = state.fg, Bold = state.bold, Italic = state.itallic, Underline = state.underlined};
                if (result[^1].Count <= 0) {
                    result[^1].Add(new StyledString { text = currentChar.ToString(), style = style });
                } else if (result[^1][^1].style.Equals(style)) {
                    StyledString last = result[^1][^1];
                    result[^1][^1] = last with { text = last.text+currentChar };
                } else {
                    result[^1].Add(new StyledString { text = currentChar.ToString(), style = style });
                }
            }
            return result;
        }
        internal void Write(List<List<StyledString>> characters) {
            using StreamWriter stream = new(Path);
            (Color bg, Color fg, bool bold, bool itallic, bool underlined) state = (Color.Black, Color.White, false, false, false);
            foreach (List<StyledString> line in characters) {
                foreach (StyledString part in line) {
                    if (part.style.ForegroundColor != state.fg) {
                        stream.Write("+"+part.style.ForegroundColor.ToString());
                        state.fg = part.style.ForegroundColor;
                    } if (part.style.BackgroundColor != state.bg) {
                        stream.Write("-"+part.style.BackgroundColor.ToString());
                        state.bg = part.style.BackgroundColor;
                    } if (part.style.Bold != state.bold) {
                        stream.Write(part.style.Bold ? '{' : '}');
                        state.bold = part.style.Bold;
                    } if (part.style.Italic != state.itallic) {
                        stream.Write(part.style.Italic ? '(' : ')');
                        state.itallic = part.style.Italic;
                    } if (part.style.Underline != state.underlined) {
                        stream.Write(part.style.Underline ? '[' : ']');
                        state.underlined = part.style.Underline;
                    }
                    string str = part.text;
                    foreach (char special in specialChars) {
                        str = str.Replace(special.ToString(), "\\"+special);
                    }
                    stream.Write(str);
                }
                stream.Write('\n');
            }
        }
    }
}