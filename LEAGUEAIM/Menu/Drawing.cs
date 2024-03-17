using ImGuiNET;
using LEAGUEAIM.Features;
using LEAGUEAIM.Win32;
using Script_Engine.Cloud;
using Script_Engine.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static LEAGUEAIM.Settings;

namespace LEAGUEAIM.Utilities
{
	public static class Drawing
	{
		public static Color Lerp(this Color A, Color B, double t) //linear interpolation
		{

			double R = (1 - t) * A.R + B.R * t;
			double G = (1 - t) * A.G + B.G * t;
			double BB = (1 - t) * A.B + B.B * t;
			return Color.FromArgb((int)(255.0f), Convert.ToInt32(R), Convert.ToInt32(G), Convert.ToInt32(BB));
		}

		public static Vector4 LerpedGradientColor()
		{
			float Time = 0.0f;
			Time += 0.001f;

			Vector4 Color1 = new(72f / 255f, 33f / 255f, 243f / 255f, 1f);
			Vector4 Color2 = new(0f, 255f / 255f, 255f / 255f, 1f);
			
			Color lerped = Color.FromArgb(255, Color1.ToColor()).Lerp(Color.FromArgb(255, Color2.ToColor()), Math.Sin(Time) * 0.5f + 0.5f);
			return new Vector4(lerped.R / 255f, lerped.G / 255f, lerped.B / 255f, lerped.A / 255f);
		}
		public static void GradientBar(Vector2 start, Vector2 end)
		{
			ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));
			Vector2 window_pos = ImGui.GetWindowPos();
			Vector2 relative_start = new(window_pos.X + start.X, window_pos.Y + start.Y);
			Vector2 relative_end = new(window_pos.X + end.X, window_pos.Y + end.Y);

			Vector4 lerpedColor = LerpedGradientColor();

			ImGui.GetForegroundDrawList().AddRectFilled(
				relative_start,
				relative_end,
				lerpedColor.ToUInt32()
			);

			ImGui.PopStyleVar();
		}
		public static bool ProfileIcon(string path, float diameter, int clickAction = 0)
		{
			ImGui.SameLine();
			ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 42);
			ImGui.SetCursorPosX(ImGui.GetContentRegionAvail().X - 17);
			Vector2 p_min = CircleImage(path, diameter);
			if (ImGui.IsItemHovered())
			{
				ImGui.GetWindowDrawList().AddCircle(new(p_min.X + (diameter * 0.5f), p_min.Y + (diameter * 0.5f)), diameter * 0.5f, Settings.Colors.AccentColor.ToUInt32(), 0, 1.5f);
				ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
				ImGui.PushStyleColor(ImGuiCol.Border, Settings.Colors.AccentColor);
				ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1.5f);
				if (ImGui.BeginItemTooltip())
				{
					ImGui.PushFont(Fonts.MenuLg);
					ImGui.TextColored(Settings.Colors.AccentColor, Program._XFUser.Username);
					ImGui.PopFont();
					ImGui.PushFont(Fonts.MenuSm);
					ImGui.Text("Open LEAGUEAIM cloud storage.");
					ImGui.PopFont();
					ImGui.EndTooltip();
				}
				ImGui.PopStyleVar();
				ImGui.PopStyleColor();
			}
			return ImGui.IsItemClicked();
		}
		public static Vector2 CircleImage(string path, float diameter)
		{
			Program.Renderer.AddOrGetImagePointer(path, true, out IntPtr imgPtr, out _, out _);
			Vector2 p_min = ImGui.GetCursorScreenPos();
			Vector2 p_max = new(p_min.X + diameter, p_min.Y + diameter);
			ImGui.GetWindowDrawList().AddImageRounded(imgPtr, p_min, p_max, new(0, 0), new(1, 1), ImGui.GetColorU32(new Vector4(1, 1, 1, 1)), diameter * 0.5f);
			ImGui.Dummy(new(diameter, diameter));
			ImGui.GetWindowDrawList().AddCircle(new(p_min.X + (diameter * 0.5f), p_min.Y + (diameter * 0.5f)), diameter * 0.5f, Color.FromArgb(200, Settings.Colors.AccentColor.ToColor()).ToUInt32(), 0, 1.5f);
			return p_min;
		}
		public static void TextHeader(string label, float lineThickness, float padding = 0)
		{
			ImGui.NewLine();
			ImGui.SetCursorPosX(ImGui.GetCursorPosX() + padding);
			ImGui.PushFont(Fonts.Header);
			ImGui.Text(label);
			Vector2 textStart = ImGui.GetWindowPos() + new Vector2(padding + 14, ImGui.GetCursorPosY() - 8);
			Vector2 textEnd = new(textStart.X + ImGui.CalcTextSize(label).X, textStart.Y);
			ImGui.GetWindowDrawList().AddLine(textStart, textEnd, ImGui.ColorConvertFloat4ToU32(new(1, 1, 1, 0.4f)), lineThickness);
			ImGui.GetWindowDrawList().AddLine(textStart, new(textStart.X + (ImGui.CalcTextSize(label).X / 2), textEnd.Y), ImGui.ColorConvertFloat4ToU32(Settings.Colors.AccentColor), lineThickness);
			ImGui.PopFont();
			ImGui.Spacing();
		}
		public static void Image(string path, Vector2 size)
		{
			Program.Renderer.AddOrGetImagePointer(path, true, out IntPtr headerPtr, out _, out _);
			ImGui.Image(headerPtr, size, new(0, 0), new(1, 1));
		}
		public static void Tooltip(string text, Vector4 color = new(), float pad = 0)
		{
			Vector4 empty = new();
			if (color == empty) color = Settings.Colors.AccentColor;
			ImGui.SameLine();
			if (pad > 0) ImGui.SetCursorPosX(pad);

			ImGui.PushFont(Fonts.Icons);
			ImGui.TextColored(color, IconFonts.FontAwesome6.CircleQuestion);
			ImGui.PopFont();

			ImGui.PushStyleColor(ImGuiCol.Border, Settings.Colors.AccentColor);
			ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1.5f);
			if (ImGui.BeginItemTooltip())
			{
				ImGui.Text(text);
				ImGui.EndTooltip();
			}
			ImGui.PopStyleVar();
			ImGui.PopStyleColor();
		}
		public static void ColorPicker(string label, ref Vector4 var, ImGuiCol[] style)
		{
			if (ImGui.ColorButton(label, var, ImGuiColorEditFlags.None, new Vector2(25, 25)))
			{
				ImGui.OpenPopup(label);
			}
			if (ImGui.BeginPopup(label))
			{
				if (ImGui.ColorPicker4(label, ref var))
				{
					var = new Vector4(var.X, var.Y, var.Z, var.W);

					foreach (ImGuiCol col in style)
					{
						switch (col)
						{
							case ImGuiCol.ButtonActive:
							case ImGuiCol.HeaderActive:
								ImGui.PushStyleColor(col, new Vector4(var.X, var.Y, var.Z, 0.4f));
								break;
							case ImGuiCol.ButtonHovered:
							case ImGuiCol.HeaderHovered:
								ImGui.PushStyleColor(col, new Vector4(var.X, var.Y, var.Z, 0.8f));
								break;
							default:
								ImGui.PushStyleColor(col, var);
								break;
						}
					}
				}

				ImGui.EndPopup();
			}
			ImGui.SameLine();
			ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 4);
			ImGui.Text(label);
		}
		public static void Hotkey(ref Keys key, Vector2 size_arg, string seed = "")
		{
			if (Engine.WaitingForKey[(int)key] && !Engine.CapturedInput)
			{
				IconButton("...", string.Empty, size_arg, true, ImGui.GetStyle().FrameRounding, 2);
				foreach (Keys Key in InputKeys.KeyCodes.Select(v => (Keys)v))
				{
					if (User32.GetAsyncKeyState(Key) && Key != Keys.LButton) //  && Key != Keys.LButton
					{
						if (!QuickPeek.KeyAlreadyUsed(Key) && !Rapidfire.KeyAlreadyUsed(Key))
						{
							if (Key == Keys.Delete)
							{
								key = Keys.None;
								Engine.WaitingForKey[(int)key] = false;
								Engine.CapturedInput = true;
							}
							else if (Key == Keys.Escape)
							{
								Engine.WaitingForKey[(int)key] = false;
								Engine.CapturedInput = true;
							}
							else
							{
								key = Key;
								Engine.WaitingForKey[(int)key] = false;
								Engine.CapturedInput = true;
							}
						}
						else
						{
							MessageBox.Show("Key already in use. Please select another key.", "LEAGUEAIM", MessageBoxButtons.OK);
						}
					}
				}
			}
			else
			{
				bool isMouseButton = VirtualInput.IsKeyMouseButton(key);
				if (IconButton(InputKeys.KeyNames[(int)key].ToReadableKey(), isMouseButton ? IconFonts.FontAwesome6.ComputerMouse : IconFonts.FontAwesome6.Keyboard, size_arg, true, ImGui.GetStyle().FrameRounding, 0))
				{
					Engine.WaitingForKey[(int)key] = true;
					Engine.CapturedInput = false;
				}
			}
		}
		public static void ConfigKey(Vector2 size_arg)
		{
			Keys key;
			if (Engine.WaitingForHotkey && !Engine.CapturedInput)
			{
				ImGui.Button("...", size_arg);
				foreach (Keys Key in InputKeys.KeyCodes.Select(v => (Keys)v))
				{
					if (Win32.User32.GetAsyncKeyState(Key) && Key != Keys.LButton)
					{
						if (Key == Keys.Delete)
						{
							key = Keys.None;
							Engine.WaitingForHotkey = false;
							Engine.CapturedInput = true;
						}
						else if (Key == Keys.Escape)
						{
							Engine.WaitingForHotkey = false;
							Engine.CapturedInput = true;
						}
						else
						{ 
							Logger.WriteLine($"Hotkey for {Profiles.CurrentProfile} set to {Key}!");
							key = Key;
							IniFile curP = new(Path.Combine(Profiles.Location, Profiles.CurrentProfile + ".ini"));
							curP.Write("Hotkey", key.ToString(), "Settings");
							Profiles.LoadProfile(Profiles.CurrentProfile);
							Engine.WaitingForHotkey = false;
							Engine.CapturedInput = true;
						}
					}
				}
			}
			else
			{
				string keyName;
				try
				{
					keyName = InputKeys.KeyNames[Settings.Menu.CurrentHotkey].ToReadableKey();
				}
				catch
				{
					keyName = "Unknown Key";
				}
				if (IconButton(keyName, IconFonts.FontAwesome6.Keyboard, size_arg, true, ImGui.GetStyle().FrameRounding, 0))
				{
					Engine.WaitingForHotkey = true;
					Engine.CapturedInput = false;
				}
			}
		}
		public static void TextCentered(string text)
		{
			var windowWidth = ImGui.GetWindowSize().X;
			var textWidth = ImGui.CalcTextSize(text).X;

			ImGui.SetCursorPosX((windowWidth - textWidth) * 0.5f);
			ImGui.Text(text);
		}
		public static bool IconButton(string label, string icon, Vector2 size, bool centered = false, float rounding = 4, float thickness = 2)
		{
			ImDrawListPtr drawList = ImGui.GetWindowDrawList();
			Vector2 p = ImGui.GetCursorScreenPos();
			bool res = ImGui.InvisibleButton(label + icon + DateTime.Now.ToString(), size);
			ImGuiStylePtr style = ImGui.GetStyle();
			uint col_bg;
			if (ImGui.IsItemHovered())
				col_bg = style.Colors[(int)ImGuiCol.ButtonHovered].ToUInt32();
			else
				col_bg = style.Colors[(int)ImGuiCol.Button].ToUInt32();
			if (ImGui.IsItemActive())
				col_bg = style.Colors[(int)ImGuiCol.ButtonActive].ToUInt32();
			drawList.AddRectFilled(p, new Vector2(p.X + size.X, p.Y + size.Y), col_bg, rounding, ImDrawFlags.RoundCornersAll);
			if (thickness > 0)
				drawList.AddRectFilled(new(p.X + thickness, p.Y + thickness), new(p.X + size.X - thickness, p.Y + size.Y - thickness), new Vector4(0.0f, 0.0f, 0.0f, 0.8f).ToUInt32(), rounding, ImDrawFlags.RoundCornersAll);
			ImGui.PushFont(Fonts.Icons);
			float textWidth = ImGui.CalcTextSize(icon).X;
			float textHeight = ImGui.CalcTextSize(icon).Y;
			drawList.AddText(new(9 + p.X, p.Y + size.Y / 2 - textHeight / 2), Settings.Colors.TextColor.ToUInt32(), icon);
			ImGui.PopFont();
			ImGui.PushFont(Fonts.Menu);
			if (centered)
			{
				float textWidth2 = ImGui.CalcTextSize(label).X;
				float textHeight2 = ImGui.CalcTextSize(label).Y;
				drawList.AddText(new(p.X + size.X / 2 - textWidth2 / 2, p.Y + size.Y / 2 - textHeight2 / 2 - 1), Settings.Colors.TextColor.ToUInt32(), label);
			}
			else
			{
				drawList.AddText(new(5 + p.X + textWidth + 12, p.Y + size.Y / 2 - textHeight / 2), Settings.Colors.TextColor.ToUInt32(), label);
			}
			ImGui.PopFont();
			return res;
		}

		public static void AccentBar()
		{
			Vector2 window_pos = ImGui.GetWindowPos();
			ImGui.GetWindowDrawList().AddRectFilled(new Vector2(window_pos.X + 1, window_pos.Y + 1), window_pos + new Vector2(ImGui.GetWindowSize().X - 1, 4), Settings.Colors.AccentColor.ToUInt32(), 0, ImDrawFlags.None);
			//ImGui.GetWindowDrawList().AddRect(window_pos, ImGui.GetWindowSize(), new Vector4(0, 0, 0, 1.0f).ToUInt32(), 0, ImDrawFlags.RoundCornersAll, 1.0f);
		}
		public static void Footer()
		{
			//if (Settings.Engine.ShowFooter)
			//{
			//	//ImGui.Separator();
			//	//{
			//	//	ImGui.Text("");
			//	//	ImGui.SameLine();
			//	//	ImGui.PushFont(Fonts.Icons);
			//	//	ImGui.Text(IconFonts.FontAwesome6.User);
			//	//	ImGui.PopFont();
			//	//	ImGui.SameLine();
			//	//	ImGui.Text("User:");
			//	//	ImGui.SameLine();
			//	//	ImGui.TextColored(Settings.Colors.AccentColor, Program._XFUser.Username);
			//	//	ImGui.SameLine();
			//	//	ImGui.SetCursorPosX(ImGui.GetWindowWidth() - 175);
			//	//	ImGui.Text(" ");
			//	//	ImGui.SameLine();
			//	//	ImGui.PushFont(Fonts.Icons);
			//	//	ImGui.Text(IconFonts.FontAwesome6.Box);
			//	//	ImGui.PopFont();
			//	//	ImGui.SameLine();
			//	//	ImGui.Text("Profile:");
			//	//	ImGui.SameLine();
			//	//	ImGui.TextColored(Settings.Colors.AccentColor, Profiles.CurrentProfile);
			//	//}
			//}
		}
	}
}
