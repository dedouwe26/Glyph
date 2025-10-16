namespace Glyph
{
    internal static class Scroll2 {
        internal static int X = 0;
        internal static int Y = 0;
        internal static void Update((int X, int Y) offset) { // TODO: REWORK
            if (X+offset.X < 0 || Y+offset.Y < 0) { return; }
            X+=offset.X;
            Y+=offset.Y;
            if (Cursor2.X-X < 0) {
                Cursor2.X = X;
            }
            if (Cursor2.Y-Y < 0) {
                Cursor2.Y = Y;
            }
            Renderer2.FullDraw();
        }
    }
}