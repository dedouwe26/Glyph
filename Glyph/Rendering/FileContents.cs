using System.Diagnostics.CodeAnalysis;
using LambdaKit.Terminal;

namespace Glyph.Rendering;

public static class FileContents
{
	public static (int x, int y) Position { get => (Ruler.position.x + Ruler.Width, Ruler.position.y); }
	public static int Width { get => Renderer.windowWidth - Ruler.Width; }
	public static readonly int height = Ruler.height;
	public static bool GetScreenPos((int x, int y) scrollPos, [NotNullWhen(true)] out (int x, int y)? screenPos) {
		if (scrollPos.x >= Width || scrollPos.y >= height) { screenPos = null; return false; }
		scrollPos.x += Position.x;
		scrollPos.y += Position.y;
		if (scrollPos.x < 0 || scrollPos.y < 0) { screenPos = null; return false; }
		screenPos = scrollPos;
		return true;
	}
	public static (int x, int y) GetScrollPos((int x, int y) screenPos) {
		return (screenPos.x - Position.x, screenPos.y - Position.y);
	}

	public static void Clear() {
		Renderer.ClearRect((Position.x, Position.y), Width, height);
	}
	public static void Render() {
		Clear();
		for (int i = 0; i < Glyph.File.content.Count; i++) {
			RenderLine(i);
		}
	}
	public static void RenderLine(int line) {
		int offset = 0;
		foreach (Fragment fragment in Glyph.File.content[line]) {
			RenderFragment(fragment, (offset, line));
			offset += fragment.text.Length;
		}
	}
	public static void RenderFragment(Fragment fragment, (int x, int y) textPos) {
		if (Scroll.Apply(textPos, out var scrollPos)) {
			if (GetScreenPos(scrollPos.Value, out var screenPos)) {
				Terminal.Set(fragment.text, screenPos.Value, fragment.style);
			}
		}
	}

	public static void RenderChar((int x, int y) screenPos) {
		var c = Glyph.File.GetCharacter(Scroll.Remove(GetScrollPos(screenPos)));
		if (c.HasValue) {
			Terminal.Set(c.Value.Item1, screenPos, c.Value.Item2);
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
}