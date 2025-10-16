using LambdaKit.Terminal;

namespace Glyph;

public struct Fragment {
	public required Style style;
	public required string text;
}

public class File {
	public static async ValueTask<File> Load(string path) {
		using StreamReader stream = new(path);

		List<List<Fragment>> result = [[]];
		Fragment fragment = new() {
			style = new() {
				ForegroundColor = RGBColor.White,
				BackgroundColor = RGBColor.Black
			},
			text = ""
		};
		void ResetFragment() {
			if (fragment.text != "") {
				result[^1].Add(fragment);
				fragment.style = fragment.style.CloneStyle();
				fragment.text = "";
			}
		}

		char[] buffer = new char[1];
		char currentChar;

		while ((await stream.ReadAsync(buffer))>0) {
			currentChar = buffer[0];
			if (char.IsControl(currentChar)&&currentChar!='\n') {
				continue;
			}
			switch (currentChar) {
				case '+':
					buffer = new char[6];
					ValueTask<int> task = stream.ReadAsync(buffer);
					ResetFragment();
					if (await task <= 0) { break; }
					fragment.style.ForegroundColor = new RGBColor(new string(buffer));
					if (fragment.style.ForegroundColor.Equals(RGBColor.White)) fragment.style.ForegroundColor = StandardColor.Default;
					buffer = new char[1];
					break;
				case '-':
					buffer = new char[6];
					task = stream.ReadAsync(buffer);
					ResetFragment();
					if (await task <= 0) { break; }
					fragment.style.BackgroundColor = new RGBColor(new string(buffer));
					if (fragment.style.BackgroundColor.Equals(RGBColor.Black)) fragment.style.BackgroundColor = StandardColor.Default;
					buffer = new char[1];
					break;
				case '{':
					ResetFragment();
					fragment.style.Bold = true;
					break;
				case '}':
					ResetFragment();
					fragment.style.Bold = false;
					break;
				case '(':
					ResetFragment();
					fragment.style.Italic = true;
					break;
				case ')':
					ResetFragment();
					fragment.style.Italic = false;
					break;
				case '[':
					ResetFragment();
					fragment.style.Underline = true;
					break;
				case ']':
					ResetFragment();
					fragment.style.Underline = false;
					break;
				case '\n':
					ResetFragment();
					result.Add([]);
					break;
				case '\\':
					if (await stream.ReadAsync(buffer) <= 0) { break; }
					if (buffer[0] == '\n') goto case '\n';
					fragment.text += buffer[0];
					break;
				default:
					fragment.text += currentChar;
					break;
			}
		}
		if (fragment.text!=null) {
			result[^1].Add(fragment);
		}
		return new(path, result);
	}
	public readonly string path;
	public string Name { get => Path.GetFileName(path); }
	public List<List<Fragment>> content;
	private File(string filepath, List<List<Fragment>> lines) {
		path = filepath;
		content = lines;
	}

	public bool LineExists(int line) => content.Count > line;

	public (char, Style)? GetCharacter((int x, int y) textPos) {
		Fragment? fragment = GetFragment(textPos, out int? pos, out int? _);
		if (fragment == null) return null;
		return (fragment.Value.text[textPos.x - pos!.Value], fragment.Value.style);
	}
	public Fragment? GetFragment((int x, int y) textPos, out int? fragmentPosition, out int? fragmentIndex) {
		int length = 0;
		List<Fragment> line = content.Count > textPos.y ? content[textPos.y] : [];
		for (int fragIndex = 0; fragIndex < line.Count; fragIndex++) {
			Fragment fragment = line[fragIndex];
			length += fragment.text.Length;
			if (length > textPos.x) {
				fragmentPosition = length-fragment.text.Length;
				fragmentIndex = fragIndex;
				return fragment;
			}
		}
		fragmentPosition = null;
		fragmentIndex = null;
		return null;
	}
}