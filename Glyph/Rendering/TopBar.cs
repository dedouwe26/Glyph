using LambdaKit.Terminal;

namespace Glyph.Rendering;

public static class TopBar {
	public static readonly (int x, int y) position = (0, 0);
	public static readonly ushort width = Renderer.windowWidth;
	public static readonly ushort height = 1;

	public static void PreLoadRender() {
		Terminal.Set(new string(' ', width), position, new() { BackgroundColor = RGBColor.DarkGray });
		Terminal.Set("Glyph", position, new() { Bold = true, ForegroundColor = RGBColor.Orange, BackgroundColor = RGBColor.DarkGray });
	}
	public static void PostLoadRender() {
		DrawFileName(Glyph.File.Name);
	}
	public static void DrawFileName(string name) {
		Terminal.Set(name, ((int)Terminal.Width / 2 + 1 - name.Length / 2, 0), new Style { Bold = true, ForegroundColor = RGBColor.White, BackgroundColor = RGBColor.DarkGray });
	}

}