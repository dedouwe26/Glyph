using LambdaKit.Terminal;

namespace Glyph.Rendering;

public static class BottomBar {
	public static readonly (int x, int y) position = (0, Renderer.windowHeight-1);
	public static readonly int width = Renderer.windowWidth;
	public static readonly ushort height = 1;
	public static void Render() {
		Terminal.Set(new string(' ', width), position, new() { BackgroundColor = RGBColor.DarkGray });
		// TODO: Render Pallete if neccessary, render cursor position.
		RenderCursorPosition();
	}
	public static void RenderPallete() {

	}
	public static void RenderHexPallete() {

	}
	public static void RenderCursorPosition() {
		string text = $"({Glyph.cursor.x};{Glyph.cursor.y})";
		Terminal.Set(" ", (position.x+width-text.Length-1, position.y), new() {BackgroundColor=RGBColor.DarkGray});
		Terminal.Set(text, (position.x+width-text.Length, position.y), new() {BackgroundColor=RGBColor.DarkGray});
	}

	public static void OnCursorMove() {
		
	}
}