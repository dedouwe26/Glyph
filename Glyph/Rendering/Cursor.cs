
using LambdaKit.Terminal;

namespace Glyph.Rendering;

public static class Cursor {
	public static readonly RGBColor FromCursorColor = RGBColor.DarkGray;
	public static readonly RGBColor CursorColor = new(128, 128, 128);
	public static void Render() {
		RenderCursor();
	}
	public static void ClearCursor((int x, int y) textPos) {
		if (Scroll.Apply(textPos, out var scrollPos)) {
			if (FileContents.GetScreenPos(scrollPos.Value, out var screenPos)) {
				FileContents.RenderChar(screenPos.Value);
			}
		}
	}
	public static void RenderCursor() {
		if (Scroll.Apply((Glyph.cursor.x, Glyph.cursor.y), out var scrollPos)) {
			if (FileContents.GetScreenPos(scrollPos.Value, out var screenPos)) {
				var c = Glyph.File.GetCharacter((Glyph.cursor.x, Glyph.cursor.y));
				Terminal.Set(c.HasValue?c.Value.Item1:' ', screenPos.Value, new Style{BackgroundColor = CursorColor});
			}
		}
	}
	public static void RenderFromCursor() {
		if (Glyph.cursor.from == null) return;
		if (Scroll.Apply(Glyph.cursor.from.Value, out var scrollPos)) {
			if (FileContents.GetScreenPos(scrollPos.Value, out var screenPos)) {
				var c = Glyph.File.GetCharacter((Glyph.cursor.x, Glyph.cursor.y));
				Terminal.Set(c.HasValue?c.Value.Item1:' ', screenPos.Value, new Style{BackgroundColor = FromCursorColor});
			}
		}
	}
	/// <summary>
	/// Gets executed when there has been scrolled up or down.
	/// </summary>
	/// <param name="offset">When it is positive, it has been scrolled down.</param>
	public static void OnVerticalScroll(int offset) {
	}
	/// <summary>
	/// Gets executed when there has been scrolled left or right.
	/// </summary>
	/// <param name="offset">When it is positive, it has been scrolled to the right.</param>
	public static void OnHorizontalScroll(int offset) {
	}

	public static void OnCursorMove((int x, int y) offset) {
	}
	public static void OnFromCursor() {
		// TODO: Add params
	}
}