using Glyph.Rendering;

namespace Glyph;

public static class Glyph {
	private static File? file = null;

	public static File File { get => file ?? throw new InvalidOperationException("The file hasn't been loaded"); }
	public static readonly Cursor cursor = new();
	public static async ValueTask Initialize(string path) {
		ValueTask<File> task = File.Load(path);

		Renderer.PreLoadRender();

		file = await task;

		Renderer.PostLoadRender();
	}
	public static void Stop() {
		Renderer.Stop();
	}
}