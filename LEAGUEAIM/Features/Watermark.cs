using ImGuiNET;
using LEAGUEAIM.Utilities;
using System.Numerics;

namespace LEAGUEAIM.Features
{
	internal class Watermark : Feature
	{
		public override void Render()
		{
			ImGui.Checkbox("Watermark", ref Enabled);
			Drawing.Tooltip($"Show the LEAGUEAIM watermark in the top left corner of your game.", Settings.Colors.AccentColor);
		}
		public override void Run()
		{
			// OVERLAY DRAWING
			if (Enabled)
			{
				string delimitor = "  |  ";
				float drawHeight = 28.0f;
				float rounding = .0f;

				// build overlay text
				string overlayText = $"LEAGUEAIM.gg{delimitor}";

				if (Program._XFUser != null)
					overlayText += $"{Program._XFUser.Username}{delimitor}";

				overlayText += $"{DateTime.Now:hh:mm tt}";

				if (Profiles.CurrentProfile != "None")
					overlayText += $"{delimitor}{Profiles.CurrentProfile}";

				if (Settings.Lua.Enabled && Settings.Lua.CurrentScript >= 0)
					overlayText += $"{delimitor}{Settings.Lua.ScriptName}";

				ImGui.PushFont(Fonts.MenuSm);
				float textWidth = ImGui.CalcTextSize(overlayText).X;

				ImDrawListPtr drawList = ImGui.GetBackgroundDrawList();

				drawList.AddRectFilled(new Vector2(5, 5), new Vector2(textWidth + 30, drawHeight), Settings.Colors.BgColor.ToUInt32(), rounding, ImDrawFlags.RoundCornersAll);

				// top side (2px line)
				drawList.AddLine(new Vector2(6, 6), new Vector2(textWidth + 29, 6), ImGui.ColorConvertFloat4ToU32(Settings.Colors.AccentColor), 2f);

				drawList.AddRect(new Vector2(5, 5), new Vector2(textWidth + 30, drawHeight), new Vector4(0, 0, 0, 0.5f).ToUInt32(), rounding, ImDrawFlags.RoundCornersAll, 1.0f);
				drawList.AddText(new Vector2(18, 10), ImGui.ColorConvertFloat4ToU32(Settings.Colors.TextColor), overlayText);
				ImGui.PopFont();
			}
		}

		public static Watermark Instance = new();
	}
}
