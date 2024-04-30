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
    }
}