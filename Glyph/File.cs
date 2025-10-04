using System.Runtime.InteropServices;
using LambdaKit.Terminal;

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

        private static readonly char[] specialChars = ['\\', '{', '}', '(', ')', '[', ']', '+', '-'];

        internal List<List<StyledString>> Parse() {
            List<List<StyledString>> result = [[]];
            using StreamReader stream = new(Path);
            (RGBColor bg, RGBColor fg, bool bold, bool itallic, bool underlined) = (RGBColor.Black, RGBColor.White, false, false, false);
            char? previousChar = null;
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
                        fg = new RGBColor(new string(buffer));
                        previousChar = currentChar;
                        continue;
                    } else if (currentChar == '-') {
                        char[] buffer = new char[6];
                        stream.Read(buffer, 0, 6);
                        bg = new RGBColor(new string(buffer));
                        previousChar = currentChar;
                        continue;
                    } else if (currentChar == '{') {
                        bold = true;
                        previousChar = currentChar;
                        continue;
                    } else if (currentChar == '}') {
                        bold = false;
                        previousChar = currentChar;
                        continue;
                    } else if (currentChar == '(') {
                        itallic = true;
                        previousChar = currentChar;
                        continue;
                    } else if (currentChar == ')') {
                        itallic = false;
                        previousChar = currentChar;
                        continue;
                    } else if (currentChar == '[') {
                        underlined = true;
                        previousChar = currentChar;
                        continue;
                    } else if (currentChar == ']') {
                        underlined = false;
                        previousChar = currentChar;
                        continue;
                    }
                }
                canStyle = (previousChar=='\\' && currentChar=='\\')||(currentChar!='\\');
                if (currentChar == '\\' && previousChar != '\\') {
                    previousChar = currentChar;
                    continue;
                }

                if (currentChar == '\n') { result.Add([]); previousChar = currentChar; continue; }

                Style style = new() { BackgroundColor = bg, ForegroundColor = fg, Bold = bold, Italic = itallic, Underline = underlined};
                if (result[^1].Count <= 0) {
                    result[^1].Add(new StyledString { text = currentChar.ToString(), style = style });
                } else if (result[^1][^1].style.Equals(style)) {
                    StyledString last = result[^1][^1];
                    result[^1][^1] = last with { text = last.text+currentChar };
                } else {
                    result[^1].Add(new StyledString { text = currentChar.ToString(), style = style });
                }
                previousChar = currentChar;
            }
            return result;
        }
        internal void Write(List<List<StyledString>> characters) {
            using StreamWriter stream = new(Path);
            (RGBColor bg, RGBColor fg, bool bold, bool itallic, bool underlined) = (RGBColor.Black, RGBColor.White, false, false, false);
            foreach (List<StyledString> line in characters) {
                foreach (StyledString part in line) {
                    part.style.BackgroundColor = part.style.BackgroundColor==PalleteColor.Default ? RGBColor.Black : part.style.BackgroundColor;
                    if (part.style.ForegroundColor != fg) {
						RGBColor cast = (part.style.ForegroundColor as RGBColor)!;
						stream.Write("+"+cast.ToHex());
                        fg = cast;
                    } if (part.style.BackgroundColor != bg) {
						RGBColor cast = (part.style.BackgroundColor as RGBColor)!;
                        stream.Write("-"+cast.ToHex());
                        bg = cast;
                    } if (part.style.Bold != bold) {
                        stream.Write(part.style.Bold ? '{' : '}');
                        bold = part.style.Bold;
                    } if (part.style.Italic != itallic) {
                        stream.Write(part.style.Italic ? '(' : ')');
                        itallic = part.style.Italic;
                    } if (part.style.Underline != underlined) {
                        stream.Write(part.style.Underline ? '[' : ']');
                        underlined = part.style.Underline;
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