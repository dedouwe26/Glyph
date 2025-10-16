using LambdaKit.Terminal;

namespace Glyph
{
	internal delegate void StyleChanger (StyledString oldStyle, int X, int Y);
	internal static class Cursor2 {
		internal static readonly RGBColor FromCursorRGBColor = RGBColor.DarkGray;
		internal static readonly RGBColor CursorRGBColor = new(128, 128, 128);
		internal static int X = 0;
		internal static int Y = 0;
		internal static (int? X, int? Y) from = (null, null);
		internal static void From() {
			if (
				Y > Glyph2.text.Count-1 // Cannot leave screen
				|| Y < 0
			) {return;}
			if (X > Renderer2.GetLength(Y)-1 || X < 0) {return;}
			if (from.Y != null && from.X != null) {
				Renderer2.DrawChar(from.X.Value, from.Y.Value);
			}
			from = (X, Y);
			Renderer2.DrawFromCursor();
		}
		internal static void UpdateCursor((int X, int Y) offset) {
			int screenX = X-Scroll2.X;
			int screenY = Y-Scroll2.Y;
			int newX = X+offset.X;
			int newY = Y+offset.Y;
			
			// Check if out of bounds.
			// Y
			 { if (
				newY >= Glyph2.text.Count || // cannot leave written text area.
				newY < Scroll2.Y || // Cannot go higher than highest.
				Y+Renderer2.FrameOffsetY+offset.Y-Scroll2.Y > Renderer2.FrameSize.height // Cannot leave screen.
			)return; }
			// X
			if (
				(offset.Y == 0 && newX > Renderer2.GetLength(newY)) || // cannot leave written text area (for snap reasons: checking for offsetY == 0).
				newX > Renderer2.FrameSize.width+Scroll2.X || // Cannot go higher than highest.
				newX < Scroll2.X // Cannot leave screen.
			) { return; } 
			
			int length = Renderer2.GetLength(newY);

			if (newX >= length) {
				if (
					length > Renderer2.FrameSize.width+Scroll2.X || // Cannot go higher than highest.
					length < Scroll2.X // Cannot leave screen.
				) { return; } 
			}

			// For checking if cursor goes behind the from cursor.
			if (from.Y != null && from.X != null) {
				if (newY < from.Y.Value||(newY == from.Y.Value&&newX < from.X.Value)) { // Check if cursor gets behind from cursor
					Renderer2.DrawChar(from.X.Value, from.Y.Value);
					from = (null, null);
				}
			}
			
			// Remove highlight from old place.
			if (X==from.X&&Y==from.Y) { // Check if the from cursor was there.
				Renderer2.DrawFromCursor();
			} else {
				// Redraw.
				Renderer2.DrawChar(screenX, screenY); // FIXME?: error.
			}
			
			// Update Cursor position.
			if (newX >= length) {
				X=length;
			} else {
				X+=offset.X;
			}
			Y += offset.Y;

			Renderer2.DrawCursor();
		}
		internal static void Left() {
			UpdateCursor((-1, 0));
		}
		internal static void Right() {
			UpdateCursor((1, 0));
		}
		internal static void Up() {
			UpdateCursor((0, -1));
		}
		internal static void Down() {
			UpdateCursor((0, 1));
		}

		internal static bool IsOnPartSplit(int x, int y, out int index) {
			if (x == 0) { index = 0; return true; }
			List<StyledString> line = Glyph2.text[y];
			int splitLine = 0;
			for (int i = 0; i < line.Count; i++) {
				StyledString part = line[i];
				splitLine += part.text.Length;
				if (x==splitLine) {
					index = i+1;
					return true;
				} else if (x<splitLine) {
					index = i;
					return false;
				}
			}
			index = -1;
			return false;
		}

		internal static int CreateSplit(int part, int x, int y) {
			List<StyledString> line = Glyph2.text[y];
			StyledString original = line[part];

			int offsetInChars = 0;
			for (int i = 0; i < part; i++) {
				offsetInChars+=line[i].text.Length;
			}
			offsetInChars = x - offsetInChars;

			StyledString left = new() {
				text = original.text[..offsetInChars],
				style = original.style.CloneStyle()
			};
			StyledString right = new() {
				text = original.text[offsetInChars..],
				style = original.style.CloneStyle()
			};

			Glyph2.text[y].RemoveAt(part);
			Glyph2.text[y].InsertRange(part, [left, right]);
			return part+1;
		}

		internal static void NewLine() {
			if (!IsOnPartSplit(X, Y, out int start)) {
				start = CreateSplit(start, X, Y);
			}
			if (X == Renderer2.GetLength(Y)) {
				Glyph2.text.Insert(Y+1, []);
				Renderer2.DrawChar(X, Y);
			} else {
				List<StyledString> newLine = Glyph2.text[Y].Skip(start).ToList();
				Glyph2.text[Y].RemoveRange(start, Glyph2.text[Y].Count - start);
				Glyph2.text.Insert(Y+1, newLine);
				int offset = X;
				for (int pi = 0; pi < newLine.Count; pi++) {
					for (int ci = 0; ci < newLine[pi].text.Length; ci++) {
						Renderer2.DrawChar(offset, Y);
						offset++;
					}
				}
			}
			UpdateCursor((-X, 1));
			if (Glyph2.text.Count > Y+1) {
				Renderer2.FullDraw();
			} else {
				Renderer2.Draw();
			}
		}
		internal static void Selection(StyleChanger changer) {
			if (from.X == null || from.Y == null) {return;}

			if (!IsOnPartSplit(from.X.Value, from.Y.Value, out int startX)) {
				startX = CreateSplit(startX, from.X.Value, from.Y.Value);
			}
			if (!IsOnPartSplit(X, Y, out int endX)) {
				endX = CreateSplit(endX, X, Y);
			}
			for (int y = from.Y.Value; y < Y+1; y++) {
				List<StyledString> line = Glyph2.text[y];
				for (int x = y==from.Y.Value ? startX : 0; x < (y==Y ? endX : line.Count); x++) {
					changer.Invoke(line[x], x, y);
				}
			}
		}
	}
}