using System.Dynamic;
using OxDED.Terminal;

namespace Glyph
{
    internal delegate Style StyleChanger (StyledString oldStyle, int X, int Y);
    internal static class Cursor {
        internal static readonly Color FromCursorColor = Color.DarkGray;
        internal static readonly Color CursorColor = new(128, 128, 128);
        internal static int X = 0;
        internal static int Y = 0;
        internal static (int? X, int? Y) from = (null, null);
        internal static void From() {
            if (Y > Glyph.text.Count-1 || Y < 0) {return;}
            if (X > Glyph.text[Y].Count-1 || X < 0) {return;}
            if (from.Y != null && from.X != null) {
                Renderer.DrawChar(from.X.Value, from.Y.Value);
            }
            from = (X, Y);
            Terminal.Set(Renderer.GetCharacter(from.X.Value, from.Y.Value) ?? ' ', Renderer.GetScreenPos((from.X.Value, from.Y.Value)), new Style { BackgroundColor = Cursor.FromCursorColor });
        }
        internal static void UpdateCursor((int X, int Y) offset) { //FIXME: still checking collision as if scroll: 0
            int screenX = X-Scroll.X;
            int screenY = Y-Scroll.Y;
            int newX = X+offset.X;
            int newY = Y+offset.Y;
            
            // Check if out of bounds.
            // Y
             { if (
                newY >= Glyph.text.Count || // cannot leave written text area.
                newY < Scroll.Y || // Cannot go higher than highest.
                Y+Renderer.FrameOffsetY+offset.Y-Scroll.Y > Renderer.FrameSize.height // Cannot leave screen.
            )return; }
            // X
            if (
                (offset.Y == 0 && newX > Renderer.GetLength(newY)) || // cannot leave written text area (for snap reasons: checking for offsetY == 0).
                newX > Renderer.FrameSize.width+Scroll.X || // Cannot go higher than highest.
                newX < Scroll.X // Cannot leave screen.
            ) { return; } 
            
            int length = Renderer.GetLength(newY);

            if (newX >= length) {
                if (
                    length > Renderer.FrameSize.width+Scroll.X || // Cannot go higher than highest.
                    length < Scroll.X // Cannot leave screen.
                ) { return; } 
            }

            // For checking if cursor goes behind the from cursor.
            if (from.Y != null && from.X != null) {
                if (newY < from.Y.Value||(newY == from.Y.Value&&newX < from.X.Value)) { // Check if cursor gets behind from cursor
                    Renderer.DrawChar(from.X.Value, from.Y.Value);
                    from = (null, null);
                }
            }
            
            // Remove highlight from old place.
            if (X==from.X&&Y==from.Y) { // Check if the from cursor was there.
                Renderer.DrawFromCursor();
            } else {
                // Redraw.
                Renderer.DrawChar(screenX, screenY); // FIXME?: error.
            }
            
            // Update Cursor position.
            if (newX >= length) {
                X=length;
            } else {
                X+=offset.X;
            }
            Y += offset.Y;

            Renderer.DrawCursor();
        }
        internal static void Left() {
            UpdateCursor((-1, 0));
        }
        internal static void Right() {
            UpdateCursor((1, 0));
        }
        internal static void Up() {
            UpdateCursor((0, -1));
        }
        internal static void Down() {
            UpdateCursor((0, 1));
        }

        internal static bool IsOnPartSplit(int x, int y, out int index) {
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
            index = -1; // TODO?: wont work with CreateSplit.
            return false;
        }

        internal static int CreateSplit(int part, int x, int y) {
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

        internal static void NewLine() {
            if (!IsOnPartSplit(X, Y, out int start)) {
                start = CreateSplit(start, X, Y);
            }
            if (X == Renderer.GetLength(Y)) {
                Glyph.text.Insert(Y+1, []);
                Renderer.DrawChar(X, Y);
            } else {
                List<StyledString> newLine = Glyph.text[Y].Skip(start).ToList();
                Glyph.text[Y].RemoveRange(start, Glyph.text[Y].Count - start);
                Glyph.text.Insert(Y+1, newLine);
                int offset = X;
                for (int pi = 0; pi < newLine.Count; pi++) {
                    for (int ci = 0; ci < newLine[pi].text.Length; ci++) {
                        Renderer.DrawChar(offset, Y);
                        offset++;
                    }
                }
            }
            X = 0;
            Y++;
        }
        internal static void Selection(StyleChanger changer) {
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