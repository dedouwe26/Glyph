namespace Glyph
{
    using System.Globalization;
    using Style = string[];
    public struct Character {
        public required char character;
        public Color bg = Color.Black;
        public Color fg = Color.White;
        public bool bold = false;
        public bool itallic = false;
        public bool underlined = false;
        public override readonly string ToString() {
            return 
                fg.GetForegroundANSI() + 
                bg.GetBackgroundANSI() + 
                (bold ? Styles.Bold[0] : Styles.Bold[1]) + 
                (itallic ? Styles.Itallic[0] : Styles.Itallic[1]) + 
                (underlined ? Styles.Underlined[0] : Styles.Underlined[1]) + 
                character;
        }

        public Character(){}
    }
    public struct Color(byte r, byte g, byte b)
    {
        public static Color Parse(string hex) {
            return new Color(byte.Parse(hex[..2], NumberStyles.HexNumber), byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber), byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber));
        }
        public static bool operator ==(Color c1, Color c2) {
            return c1.r==c2.r && c1.g==c2.g && c1.b == c2.b;
        }
        public static bool operator !=(Color c1, Color c2) {
            return !(c1==c2);
        }
        public readonly string GetForegroundANSI() {
            return "\x1b[38;2;" + r + ";" + g + ";" + b + "m";
        }
        public readonly string GetBackgroundANSI() {
            return "\x1b[48;2;" + r + ";" + g + ";" + b + "m";
        }
        public readonly override string ToString() {
            return BitConverter.ToString([r, g, b]).Replace("-", string.Empty);
        }
        public byte r = r, g = g, b = b;
        public static readonly Color Red = new(255, 0, 0);
        public static readonly Color Green = new(0, 255, 0);
        public static readonly Color Blue = new(0, 0, 255);
        public static readonly Color DarkRed = new(128, 0, 0);
        public static readonly Color DarkGreen = new(0, 128, 0);
        public static readonly Color DarkBlue = new(0, 0, 128);
        public static readonly Color Cyan = new(0, 255, 255);
        public static readonly Color Magenta = new(255, 0, 255);
        public static readonly Color Yellow = new(255, 255, 0);
        public static readonly Color Orange = new(255, 128, 0);
        public static readonly Color Purple = new(128, 0, 128);
        public static readonly Color White = new(255, 255, 255);
        public static readonly Color Gray = new(128, 128, 128);
        public static readonly Color DarkGray = new(64, 64, 64);
        public static readonly Color Black = new(0, 0, 0);
        public static readonly Color[] fgColors = [Green, Red, Blue, DarkRed, DarkGreen, DarkBlue, Cyan, Magenta, Yellow, Orange, Purple, White, Gray];
        public static readonly Color[] bgColors = [Green, Red, Blue, DarkRed, DarkGreen, DarkBlue, Cyan, Magenta, Yellow, Orange, Purple, Black];

        public override readonly bool Equals(object? obj)
        {
            if (obj==null) {return false;}
            if (obj.GetType()==typeof(Color)) {
                return this==((Color)obj);
            }
            return false;
        }

        public override readonly int GetHashCode()
        {
            return b*1_000_000+g*1_000+r;
        }
    }
    public readonly struct Styles {
        public static readonly Style Bold = ["\x1b[1m", "\x1b[22m"];
        public static readonly Style Itallic = ["\x1b[3m", "\x1b[23m"];
        public static readonly Style Underlined = ["\x1b[4m", "\x1b[24m"];
        public static readonly string Reset = "\x1b[0m";
    }
    public static class Renderer {
        public static void Set(Character character, int X, int Y) {
            if (X == -1&&Y == -1) {return;}
            Console.SetCursorPosition(X, Y);
            Console.Write(character.ToString());
        }
        public static void Set(Character[] characters, int X, int Y) {
            if (X == -1&&Y == -1) {return;}
            foreach (Character character in characters) {
                Set(character, X, Y);
                X++;
            }
            
        }
    }
}