namespace Glyph
{
    internal static class Scroll {
        internal static int X = 0;
        internal static int Y = 0;
        internal static void Update((int X, int Y) offset) {
            if (X+offset.X < 0 || Y+offset.Y < 0) { return; }
            X+=offset.X;
            Y+=offset.Y;
            Renderer.Draw();
        }
    }
}