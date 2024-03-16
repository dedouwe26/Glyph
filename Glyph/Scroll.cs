namespace Glyph
{
    public static class Scroll {
        public static int X = 0;
        public static int Y = 0;
        public static void Update((int X, int Y) offset) {
            if (X+offset.X < 0 || Y+offset.Y < 0) { return; }
            X+=offset.X;
            Y+=offset.Y;
            Glyph.Draw();
        }
        public static (int X, int Y) GetScreenPos((int X, int Y) textPos) {
            if (textPos.X-X < 0||textPos.Y-Y < 0) {return (-1, -1);}
            return (textPos.X+Glyph.GetOffsetX(textPos.Y), textPos.Y+1-Y);
        }
    }
}