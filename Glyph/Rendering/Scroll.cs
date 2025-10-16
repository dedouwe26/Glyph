using System.Diagnostics.CodeAnalysis;

namespace Glyph.Rendering;

public static class Scroll {
	public static int x;
	public static int y;
	public static (int x, int y) Remove((int x, int y) scrollPos) {
		scrollPos.x += x;
		scrollPos.y += y;
		return scrollPos;
	}
	public static bool Apply((int x, int y) textPos, [NotNullWhen(true)] out (int x, int y)? scrollPos) {
		textPos.x -= x;
		textPos.y -= y;
		if (textPos.x < 0 || textPos.y < 0) { scrollPos = null; return false; }
		scrollPos = textPos;
		return true;
	}
	public static void VerticalScroll(int offset) {
        if (y+offset < 0) return;
		y += offset;
		if (Glyph.cursor.y-y < 0) {
			Glyph.cursor.y = y;
		}
		Renderer.OnVerticalScroll(offset);
	}
	public static void HorizontalScroll(int offset) {
        if (x+offset < 0) return;
		x += offset;
		if (Glyph.cursor.x-x < 0) {
			Glyph.cursor.x = x;
		}
		Renderer.OnHorizontalScroll(offset);
	}
}