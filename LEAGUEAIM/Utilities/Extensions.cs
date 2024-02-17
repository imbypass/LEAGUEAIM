using ImGuiNET;
using LEAGUEAIM.Win32;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace LEAGUEAIM.Utilities
{
	internal static class CursorExtensions
	{
		public static bool IsAtCenter(this Cursor cursor)
		{
			if (cursor == null) return false;
			VirtualInput.GetCursorPos(out Point mLoc);
			Point center = new(Program.ScreenSize.Width / 2, Program.ScreenSize.Height / 2);
			return mLoc != center;
		}
		public static bool IsVisible(this Cursor cursor)
		{
			if (cursor == null) return false;
			User32.CURSORINFO pci = new()
			{
				cbSize = Marshal.SizeOf(typeof(User32.CURSORINFO))
			};
			User32.GetCursorInfo(ref pci);
			bool isVisible = ((pci.flags & 0x01) != 0);
			return isVisible;
		}
		public static bool FailsCheck(this Cursor cursor)
		{
			if (Settings.Engine.CursorCheck)
			{
				bool is_visible = cursor.IsVisible();
				bool is_at_center = cursor.IsAtCenter();
				bool failed_check;
				if (Settings.Engine.CursorCheckType == 0)
				{
					failed_check = is_visible;
				}
				else
				{
					failed_check = !is_at_center;
				}
				return failed_check;
			}
			return false;
		}
	}
	internal static class StringExtensions
	{
		public static string TruncateWord(this string input, int length)
		{
			if (input == null || input.Length <= length)
				return input;

			int iNextSpace = input.LastIndexOf(' ', length);

			return string.Format("{0}..", input[..((iNextSpace > 0) ? iNextSpace : length)].Trim());
		}
		public static string SpliceText(this string input, int length)
		{
			return Regex.Replace(input, "(.{" + length + "})", "$1" + Environment.NewLine);
		}
	}
	internal static class VectorExtensions
	{
		// Convert Windows Classes to Vector2
		public static Vector2 ToVector2(this Size size)
		{
			return new Vector2(size.Width, size.Height);
		}
		public static Vector2 ToVector2(this Point point)
		{
			return new Vector2(point.X, point.Y);
		}
		public static Vector2 ToVector2(this PointF point)
		{
			return new Vector2(point.X, point.Y);
		}

		// Convert Vector2 to Windows Classes
		public static Size ToSize(this Vector2 vector)
		{
			return new Size((int)vector.X, (int)vector.Y);
		}
		public static Point ToPoint(this Vector2 vector)
		{
			return new Point((int)vector.X, (int)vector.Y);
		}
		public static PointF ToPointF(this Vector2 vector)
		{
			return new PointF((int)vector.X, (int)vector.Y);
		}

		// Convert Vector4 to Vector3
		public static Vector3 ToVector3(this Vector4 vector4)
		{
			return new Vector3(vector4.X, vector4.Y, vector4.Z);
		}
	}
	internal static class ColorExtensions
	{
		// Convert ImGui Colors and Windows Colors easily
		public static Vector4 ToVector4(this Color color)
		{
			return new Vector4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
		}
		public static uint ToUInt32(this Color color)
		{
			return ImGui.ColorConvertFloat4ToU32(color.ToVector4());
		}
		public static Color ToColor(this Vector4 vector4)
		{
			return Color.FromArgb((int)(vector4.W * 255), (int)(vector4.X * 255), (int)(vector4.Y * 255), (int)(vector4.Z * 255));
		}
		public static uint ToUInt32(this Vector4 vector4)
		{
			return ImGui.ColorConvertFloat4ToU32(vector4);
		}
		public static Vector4 ToFloat4(this uint color)
		{
			return ImGui.ColorConvertU32ToFloat4(color);
		}
	}
}
