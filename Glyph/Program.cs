using LambdaKit.Terminal.Arguments;

namespace Glyph;

static class Program {
	internal static async Task Main(string[] args) {
		ArgumentParser parser = new ArgumentFormatter()
			.Name("Glyph").Version("3.0.0")
			.Description("Glyph is a console text editor with text decorations.")

			.General()
				.Argument()
					.Name("Path")
					.Description("The path to the file to open.")
				.Finish()
				.VersionOption().Finish()
				.HelpOption().Finish()
			.Finish()
		.Finish();
		parser.Parse(args);

		string path = parser.GetArgument(0)!.Content;
		if (!System.IO.File.Exists(path)) {
			Console.WriteLine($"File not found: {path}");
			Environment.Exit(1);
			return;
		}

		await Glyph.Initialize(path);

		await Task.Delay(1000*6);

		Glyph.Stop();
	}
}