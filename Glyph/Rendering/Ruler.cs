using LambdaKit.Terminal;

namespace Glyph.Rendering;

public static class Ruler {
	private const string FilledLineChar = "\u2503";
	private const string EmptyLineChar  = "\u2507";
	public static readonly (int x, int y) position = (0, TopBar.height);
	public static int Width { get => MaxLength + 1; }
	public static readonly int height = Renderer.windowHeight - TopBar.height - BottomBar.height;

	public static void Render() {
		int length = height + position.y;
		for (int line = position.y; line < length; line++) {
			RenderLine(line);
		}
	}
	public static void PreLoadRender() {
		int length = height + position.y;
		for (int line = position.y; line < length; line++) {
			PreRenderLine(line);
		}
	}
	public static void PostLoadRender() {
		int length = height + position.y;
		for (int line = position.y; line < length; line++) {
			PostRenderLine(line);
		}
	}
	private static int GetFileLine(int line) {
		return line - position.y + Scroll.y;
	}
	private static string GetLineNumber(int line) {
		return (GetFileLine(line) + 1).ToString();
	}
	private static int maxLength = 0;
	private static int MaxLength { get => maxLength == 0 ? (maxLength = GetLineNumber(position.y + height).Length + 1) : maxLength; }
	private static int GetSpaces(int line) {
		return MaxLength - GetLineNumber(line).Length;
	}
	public static void RenderLine(int line) {
		Terminal.Set(
			new StyleBuilder().Background(RGBColor.DarkGray).Text(
				GetLineNumber(line) + new string(' ', GetSpaces(line))
			).Text(
				Glyph.File.LineExists(GetFileLine(line)) ? FilledLineChar : EmptyLineChar
			), (0, line)
		);
	}
	public static void PreRenderLine(int line) {
		Terminal.Set(
			new StyleBuilder().Background(RGBColor.DarkGray).Text(
				GetLineNumber(line) + new string(' ', GetSpaces(line))
			), (0, line)
		);
	}
	public static void PostRenderLine(int line) {
		Terminal.Set(
			new StyleBuilder().Background(RGBColor.DarkGray).Text(
				Glyph.File.LineExists(GetFileLine(line)) ? FilledLineChar : EmptyLineChar
			), (MaxLength, line)
		);
	}
	/// <summary>
	/// Gets executed when there has been scrolled up or down.
	/// </summary>
	/// <param name="offset">When it is positive, it has been scrolled down.</param>
	public static void OnVerticalScroll(int offset) {
		maxLength = 0;
		Render();
	}
}