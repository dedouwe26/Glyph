namespace Glyph
{
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
    }
}