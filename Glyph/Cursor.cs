using OxDED.Terminal;

namespace Glyph
{
    public delegate Style StyleChanger (StyledString oldStyle, int X, int Y);
    public static class Cursor {
        public static int X = 0;
        public static int Y = 0;
        public static (int? X, int? Y) from = (null, null);
        public static void From() {
            (int X, int Y) screenPos;
            if (Y > Glyph.text.Count-1 || Y < 0) {return;}
            if (X > Glyph.text[Y].Count-1 || X < 0) {return;}
            if (from.Y != null && from.X != null) {
                screenPos = Scroll.GetScreenPos((from.X.Value, from.Y.Value));
                Renderer.Set(Glyph.GetCharacter(from.X.Value, from.Y.Value), screenPos.X, screenPos.Y);
            }
            from = (X, Y);
            screenPos = Scroll.GetScreenPos((from.X.Value, from.Y.Value));
            Renderer.Set(new Character{character=Glyph.GetCharacter(from.X.Value, from.Y.Value).character, bg=Color.DarkGray}, screenPos.X, screenPos.Y);
        }
        public static void UpdateCursor((short X, short Y) offset) {
            (int X, int Y) screenPos;
            int cursorOffset = Glyph.GetOffsetX(Y+offset.Y);
            if (Y+offset.Y >= Glyph.text.Count||Y+offset.Y < 0||Y+1+offset.Y-Scroll.Y > Console.WindowHeight-1) { return; }
            if (X+offset.X > Glyph.text[Y].Count||X+offset.X < 0||cursorOffset+offset.X+X-Scroll.X > Console.WindowWidth-1) { return; }
            
            if (from.Y != null && from.X != null) {
                if (Y+offset.Y < from.Y.Value||(Y+offset.Y == from.Y.Value&&X+offset.X < from.X.Value)) {
                    screenPos = Scroll.GetScreenPos((from.X.Value, from.Y.Value));
                    Renderer.Set(Glyph.GetCharacter(from.X.Value, from.Y.Value), screenPos.X, screenPos.Y);
                    from = (null, null);
                }
            }
            
            if (X==from.X&&Y==from.Y) {
                if (from.Y != null && from.X != null) {
                    screenPos = Scroll.GetScreenPos((X, Y));
                    Renderer.Set(new Character{character=Glyph.GetCharacter(from.X.Value, from.Y.Value).character, bg=Color.DarkGray}, screenPos.X, screenPos.Y);
                }
            } else {
                screenPos = Scroll.GetScreenPos((X, Y));
                Renderer.Set(Glyph.GetCharacter(X, Y), screenPos.X, screenPos.Y);
            }
            
            if (X+offset.X >= Glyph.text[Y+offset.Y].Count) {
                X=Glyph.text[Y+offset.Y].Count;
            } else {
                X+=offset.X;
            }
            Y += offset.Y;
            screenPos = Scroll.GetScreenPos((X, Y));
            Renderer.Set(new Character{character=Glyph.GetCharacter(X, Y).character, bg=Color.Gray}, screenPos.X, screenPos.Y);
        }
        public static void Left() {
            UpdateCursor((-1, 0));
        }
        public static void Right() {
            UpdateCursor((1, 0));
        }
        public static void Up() {
            UpdateCursor((0, -1));
        }
        public static void Down() {
            UpdateCursor((0, 1));
        }

        public static bool IsOnPartSplit(int x, int y, out int index) {
            if (x == 0) { index = 0; return true; }
            List<StyledString> line = Glyph.text[y];
            int splitLine = 0;
            for (int i = 0; i < line.Count; i++) {
                StyledString part = line[i];
                splitLine += part.text.Length;
                if (x==splitLine) {
                    index = i;
                    return true;
                } else if (x<splitLine) {
                    index = i;
                    return false;
                }
            }
            index = -1;
            return false;
        }

        public static int CreateSplit(int part, int x, int y) {
            List<StyledString> line = Glyph.text[y];
            StyledString original = line[part];

            int offsetInChars = 0;
            for (int i = 0; i < part; i++) {
                offsetInChars+=line[i].text.Length;
            }
            offsetInChars = x - offsetInChars;

            StyledString left = new() {
                text = original.text[..offsetInChars],
                style = original.style
            };
            StyledString right = new() {
                text = original.text[offsetInChars..],
                style = original.style
            };

            Glyph.text[y].RemoveAt(part);
            Glyph.text[y].InsertRange(part, [left, right]);
            return part+1;
        }

        public static void NewLine() {
            if (!IsOnPartSplit(X, Y, out int start)) {
                start = CreateSplit(start, X, Y);
            }
            List<StyledString> newLine = Glyph.text[Y].Skip(start).Take(Glyph.text[Y].Count - start).ToList();
            Glyph.text[Y].RemoveRange(start, Glyph.text[Y].Count-start);
            Glyph.text.Insert(Y+1, newLine);
            Down();
        }
        public static void Selection(StyleChanger changer) {
            if (from.X == null || from.Y == null) {return;}

            if (!IsOnPartSplit(from.X.Value, from.Y.Value, out int startX)) {
                startX = CreateSplit(startX, from.X.Value, from.X.Value);
            }
            if (!IsOnPartSplit(X, Y, out int endX)) {
                endX = CreateSplit(endX, from.X.Value, from.X.Value);
            }
            for (int y = from.Y.Value; y < Y+1; y++) {
                List<StyledString> line = Glyph.text[y];
                for (int x = y==from.Y.Value ? startX : 0; x < (y==Y ? endX+1 : line.Count); x++) {
                    StyledString part = line[x];
                    Glyph.text[y][x] = part with {
                        style = changer.Invoke(part, x, y)
                    };
                }
            }
        }
    }
}