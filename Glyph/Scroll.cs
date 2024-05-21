namespace Glyph
{
    internal static class Scroll {
        internal static int X = 0;
        internal static int Y = 0;
        internal static void Update((int X, int Y) offset) { // TODO: fix scrolling.
            if (X+offset.X < 0 || Y+offset.Y < 0) { return; }
            X+=offset.X;
            Y+=offset.Y;
            if (Cursor.X-X < 0) {
                Cursor.X = X;
            }
            if (Cursor.Y-Y < 0) {
                Cursor.Y = Y;
            }
            Renderer.FullDraw();
        }
    }
}