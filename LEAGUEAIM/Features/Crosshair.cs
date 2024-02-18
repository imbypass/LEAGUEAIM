using ImGuiNET;
using LEAGUEAIM.Utilities;
using LEAGUEAIM.Win32;
using System.Numerics;

namespace LEAGUEAIM.Features
{
	internal class Crosshair : Feature
	{
		public static int Style = 0;
		public static float Size = 6.0f;
		public static float Thickness = 2.0f;
		public static float Gap = 4.0f;
		public static float DotSize = 1.0f;
		public static bool CenterDot = false;
		public static bool DrawTop = true;
		public static bool ShowOnADS = false;
		public static bool HideOnADS = true;
		public static bool UseAccent = false;

		public override void Render()
		{
			ImGui.Checkbox("Enabled##CROSSHAIR", ref Enabled);

			if (Enabled)
			{
				ImGui.SameLine();
				if (ImGui.ColorButton("Crosshair Color##CROSSHAIR", Settings.Colors.CrosshairColor, ImGuiColorEditFlags.None, new Vector2(25, 25)))
				{
					ImGui.OpenPopup("Crosshair Color");
				}
				if (ImGui.BeginPopup("Crosshair Color"))
				{
					if (ImGui.ColorPicker4("Crosshair Color", ref Settings.Colors.CrosshairColor))
					{
						Settings.Colors.CrosshairColor = new Vector4(Settings.Colors.CrosshairColor.X, Settings.Colors.CrosshairColor.Y, Settings.Colors.CrosshairColor.Z, Settings.Colors.CrosshairColor.W);
					}

					ImGui.EndPopup();
				}
				if (Settings.Engine.RainbowMode)
				{
					ImGui.SameLine();
					ImGui.Checkbox("Use Rainbow Color##CROSSHAIR", ref UseAccent);
				}
				string[] CrosshairTypes = { "Cross", "Full X", "Dash", "Top X", "Circle", "Dot" };
				ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 3);
				ImGui.Checkbox("Show on ADS##CROSSHAIR", ref ShowOnADS);
				ImGui.SameLine();
				ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 3);
				ImGui.Checkbox("Hide on ADS##CROSSHAIR", ref HideOnADS);
				ImGui.SeparatorText("Settings##CROSSHAIR");
				ImGui.Combo("Style##CROSSHAIR", ref Style, CrosshairTypes, CrosshairTypes.Length);
				ImGui.SliderFloat("Size##CROSSHAIR", ref Size, 1, 200, "%.2f", ImGuiSliderFlags.AlwaysClamp);
				ImGui.SliderFloat("Thickness##CROSSHAIR", ref Thickness, 1, 100, "%.2f", ImGuiSliderFlags.AlwaysClamp);
				ImGui.SliderFloat("Gap##CROSSHAIR", ref Gap, 1, 200, "%.2f", ImGuiSliderFlags.AlwaysClamp);

				ImGui.Checkbox("Center Dot##CROSSHAIR", ref CenterDot);
				ImGui.SameLine();
				ImGui.Checkbox("Draw Top##CROSSHAIR", ref DrawTop);
				if (CenterDot)
				{
					ImGui.SliderFloat("Dot Size##CROSSHAIR", ref DotSize, 1, 100, "%.2f", ImGuiSliderFlags.AlwaysClamp);
				}
			}
		}

		public override void Run()
		{
			Size primaryScreenSize = Screen.PrimaryScreen.Bounds.Size;
			Vector2 windowSize = new(primaryScreenSize.Width, primaryScreenSize.Height);
			Vector2 windowCenter = new(windowSize.X / 2, windowSize.Y / 2);

			int x = (int)windowCenter.X;
			int y = (int)windowCenter.Y;

			ImDrawListPtr drawList = ImGui.GetBackgroundDrawList();

			uint crosshairColor = ImGui.ColorConvertFloat4ToU32(Settings.Colors.CrosshairColor);
			if (Enabled)
			{
				// dont run if we fail the cursor visibility check
				if (Cursor.Current.FailsCheck())
					return;

				if (ShowOnADS)
				{
					bool mouseADS = User32.GetAsyncKeyState(Keys.RButton);
					bool controllerADS = Settings.Controller.FlippedTriggers ? ControllerInput.IsKeyPressed(ControllerInput.ControllerKeys.LeftShoulder) : ControllerInput.IsTriggerHeld(ControllerInput.ControllerTrigger.Left);
					if (!(mouseADS || controllerADS))
					{
						return;
					}
				}

				if (HideOnADS && (User32.GetAsyncKeyState(Keys.RButton) || (Settings.Controller.FlippedTriggers ? ControllerInput.IsKeyPressed(ControllerInput.ControllerKeys.LeftShoulder) : ControllerInput.IsTriggerHeld(ControllerInput.ControllerTrigger.Left)))) return;

				if (CenterDot)
					drawList.AddEllipseFilled(new(x, y), DotSize, DotSize, crosshairColor);

				switch (Style)
				{
					case 0:
						Vector2 pt1_1 = new(x - (int)Gap, y);
						Vector2 pt2_1 = new(x - (int)Gap - (int)Size, y);
						Vector2 pt1_2 = new(x + (int)Gap, y);
						Vector2 pt2_2 = new(x + (int)Gap + (int)Size, y);
						Vector2 pt1_3 = new(x, y - (int)Gap);
						Vector2 pt2_3 = new(x, y - (int)Gap - (int)Size);
						Vector2 pt1_4 = new(x, y + (int)Gap);
						Vector2 pt2_4 = new(x, y + (int)Gap + (int)Size);

						drawList.AddLine(pt1_1, pt2_1, crosshairColor, Thickness);
						drawList.AddLine(pt1_2, pt2_2, crosshairColor, Thickness);
						drawList.AddLine(pt1_4, pt2_4, crosshairColor, Thickness);

						if (DrawTop)
							drawList.AddLine(pt1_3, pt2_3, crosshairColor, Thickness);
						break;
					case 1:
						Vector2 pt1_5 = new(x - (int)Gap, y - (int)Gap);
						Vector2 pt2_5 = new(x - (int)Gap - (int)Size, y - (int)Gap - (int)Size);
						Vector2 pt1_6 = new(x + (int)Gap, y + (int)Gap);
						Vector2 pt2_6 = new(x + (int)Gap + (int)Size, y + (int)Gap + (int)Size);
						Vector2 pt1_7 = new(x - (int)Gap, y + (int)Gap);
						Vector2 pt2_7 = new(x - (int)Gap - (int)Size, y + (int)Gap + (int)Size);
						Vector2 pt1_8 = new(x + (int)Gap, y - (int)Gap);
						Vector2 pt2_8 = new(x + (int)Gap + (int)Size, y - (int)Gap - (int)Size);

						drawList.AddLine(pt1_6, pt2_6, crosshairColor, Thickness);
						drawList.AddLine(pt1_7, pt2_7, crosshairColor, Thickness);
						if (DrawTop)
						{
							drawList.AddLine(pt1_5, pt2_5, crosshairColor, Thickness);
							drawList.AddLine(pt1_8, pt2_8, crosshairColor, Thickness);
						}
						break;
					case 2:
						Vector2 pt1_9 = new(x - (int)Gap, y);
						Vector2 pt2_9 = new(x - (int)Gap - (int)Size, y);
						Vector2 pt1_10 = new(x + (int)Gap, y);
						Vector2 pt2_10 = new(x + (int)Gap + (int)Size, y);

						drawList.AddLine(pt1_9, pt2_9, crosshairColor, Thickness);
						drawList.AddLine(pt1_10, pt2_10, crosshairColor, Thickness);
						break;
					case 3:
						Vector2 pt1_11 = new(x - (int)Gap, y - (int)Gap);
						Vector2 pt2_11 = new(x - (int)Gap - (int)Size, y - (int)Gap - (int)Size);
						Vector2 pt1_12 = new(x + (int)Gap, y - (int)Gap);
						Vector2 pt2_12 = new(x + (int)Gap + (int)Size, y - (int)Gap - (int)Size);

						drawList.AddLine(pt1_11, pt2_11, crosshairColor, Thickness);
						drawList.AddLine(pt1_12, pt2_12, crosshairColor, Thickness);
						break;
					case 4:
						drawList.AddCircle(new Vector2(x, y), Size, crosshairColor, 0, Thickness);
						break;
					case 5:
						drawList.AddEllipseFilled(new(x, y), Size, Size, crosshairColor);
						break;
				}
			}
		}

		public static Crosshair Instance = new();
	}
}
