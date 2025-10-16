using LambdaKit.Terminal;

namespace Glyph.Rendering;

public static class Renderer {
	public static readonly ushort windowWidth = (ushort)Terminal.Width;
	public static readonly ushort windowHeight = (ushort)Terminal.Height;
	public static void Clear() {
		Terminal.ClearScreen();
	}
	public static void ClearRect((int x, int y) start, int width, int height) {
		string filler = new(' ', width);
		int h = height - start.y;
		for (int y = start.y; y < h; y++) {
			Terminal.Set(filler, (start.x, start.y+y));
		}
	}
	// public static Box ScreenBox { get => new() {width=Convert.ToUInt16(Terminal.Width), height=Convert.ToUInt16(Terminal.Height)}; }
	// public static (int x, int y) CalculatePosition(Box content, Point local, Box reference, (int x, int y) refPos, Point refPoint) {
	// }
	// public static void Draw(object text, Box reference, Point local, Point global) {
	// 	string? content = text.ToString();
	// 	if (content == null) return;
	// 	Terminal.Set(text, CalculatePosition(new() {width=text}))
	// }
	// public static void Draw(object text, Point local, Point global) {
	// 	Draw(text, ScreenBox, local, global);
	// }
	public static void PreLoadRender() {
		Terminal.UseAltScreenAndSaveCursor();
		Terminal.HideCursor = true;

		TopBar.PreLoadRender();
		Ruler.PreLoadRender();
		BottomBar.Render();
	}
	public static void PostLoadRender() {
		TopBar.PostLoadRender();
		Ruler.PostLoadRender();
		FileContents.Render();
		Cursor.Render();
	}
	public static void Stop() {
		Terminal.UseNormScreenAndRestoreCursor();
	}
	/// <summary>
	/// Gets executed when there has been scrolled up or down.
	/// </summary>
	/// <param name="offset">When it is positive, it has been scrolled down.</param>
	public static void OnVerticalScroll(int offset) {
		Ruler.OnVerticalScroll(offset);
		FileContents.OnVerticalScroll(offset);
		Cursor.OnVerticalScroll(offset);
	}
	/// <summary>
	/// Gets executed when there has been scrolled left or right.
	/// </summary>
	/// <param name="offset">When it is positive, it has been scrolled to the right.</param>
	public static void OnHorizontalScroll(int offset) {
		FileContents.OnHorizontalScroll(offset);
		Cursor.OnHorizontalScroll(offset);
	}
	public static void OnCursorMove((int x, int y) offset) {
		Cursor.OnCursorMove(offset);
		BottomBar.OnCursorMove();
	}
}