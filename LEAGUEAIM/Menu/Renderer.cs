using ClickableTransparentOverlay;
using LEAGUEAIM.Utilities;
using LEAGUEAIM.Features;
using System.Numerics;
using ImGuiNET;
using Vortice.Mathematics;
using Script_Engine.Cloud;

namespace LEAGUEAIM
{
    public class LARenderer(string windowTitle, bool dpiAware) : Overlay(windowTitle, dpiAware)
	{

		Vector2 windowLocation = new(0, 0);
		Vector2 windowSize;

		protected override void Render()
		{
			ImGui.GetIO().IniSavingRate = 0.0f;

			Fonts.Load(this);

			ApplyStyle();

			Profiles.ProfileList = Profiles.GetProfileList();

			LuaEngine.ScriptList = LuaEngine.GetScriptList();

			LoginHelper.Render();

			Menu.Render();

			CloudMenu.Render();

			DrawOverlay();
		}
		private void DrawOverlay()
		{
			Size primaryScreenSize = Screen.PrimaryScreen.Bounds.Size;
			windowSize = new Vector2(primaryScreenSize.Width, primaryScreenSize.Height);

			ImGui.SetNextWindowSize(windowSize);
			ImGui.SetNextWindowPos(windowLocation);
			ImGui.Begin("CROSSHAIR", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);

			Crosshair.Instance.Run();

			Watermark.Instance.Run();

			ImGui.End();
		}
		public static void ApplyStyle()
		{
			ImGuiStylePtr style = ImGui.GetStyle();

			style.WindowRounding = 1.0f;
			style.FrameRounding = 1.0f;
			style.TabRounding = 1.0f;
			style.PopupRounding = 1.0f;
			style.ChildRounding = 1.0f;
			style.GrabRounding = 1.0f;
			style.ScrollbarRounding = 1.0f;
			style.WindowPadding = new Vector2(12, 12);
			style.WindowBorderSize = 1.0f;
			style.PopupBorderSize = 1.0f;
			style.ItemSpacing = new Vector2(10, 10);
			style.ItemInnerSpacing = new Vector2(8, 8);
			style.IndentSpacing = 25.0f;
			style.ScrollbarSize = 3.0f;
			style.GrabMinSize = 3.0f;
			style.FramePadding = new Vector2(6, 6);
			style.ButtonTextAlign = new Vector2(0.5f, 0.1f);
			style.WindowBorderSize = 0f;

			style.Colors[(int)ImGuiCol.TextDisabled] = new Vector4(0.24f, 0.23f, 0.29f, 1.00f);
			style.Colors[(int)ImGuiCol.PopupBg] = new Vector4(0f, 0f, 0f, 1.00f);
			style.Colors[(int)ImGuiCol.ChildBg] = new Vector4(0f, 0f, 0f, 1.00f);
			style.Colors[(int)ImGuiCol.BorderShadow] = new Vector4(0.92f, 0.91f, 0.88f, 1.00f);
			style.Colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.2f, 0.2f, 0.2f, 1.00f);
			style.Colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.5f, 0.5f, 0.5f, 1.00f);
			style.Colors[(int)ImGuiCol.ScrollbarBg] = new Vector4(0.10f, 0.09f, 0.12f, 0f);
			style.Colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.56f, 0.56f, 0.58f, 0f);
			style.Colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(0.06f, 0.05f, 0.07f, 0f);
			style.Colors[(int)ImGuiCol.SliderGrab] = new Vector4(0.80f, 0.80f, 0.83f, 0.31f);
			style.Colors[(int)ImGuiCol.SliderGrabActive] = new Vector4(0.06f, 0.05f, 0.07f, 1.00f);
			style.Colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.24f, 0.23f, 0.29f, 1.00f);
			style.Colors[(int)ImGuiCol.ButtonActive] = new Vector4(0.56f, 0.56f, 0.58f, 1.00f);
			style.Colors[(int)ImGuiCol.TabActive] = new Vector4(0.16f, 0.15f, 0.15f, 1.00f);
			style.Colors[(int)ImGuiCol.TabHovered] = new Vector4(0.26f, 0.25f, 0.25f, 1.00f);
			style.Colors[(int)ImGuiCol.ModalWindowDimBg] = new Vector4(0.06f, 0.06f, 0.06f, 0.6f);

			style.Colors[(int)ImGuiCol.WindowBg] = Settings.Colors.BgColor;
			style.Colors[(int)ImGuiCol.FrameBg] = Settings.Colors.FrameColor;
			style.Colors[(int)ImGuiCol.Tab] = Settings.Colors.FrameColor;
			style.Colors[(int)ImGuiCol.Text] = Settings.Colors.TextColor;
			Vector4 tCol = Settings.Colors.TextColor;
			style.Colors[(int)ImGuiCol.TextDisabled] = new Vector4(tCol.X, tCol.Y, tCol.Z, 0.3f);

		}
		public static void ApplyAccentColor()
		{ 
			ImGuiStylePtr style = ImGui.GetStyle();

			style.Colors[(int)ImGuiCol.ChildBg] = new Vector4(Settings.Colors.BgColor.X, Settings.Colors.BgColor.Y, Settings.Colors.BgColor.Z, 0.4f);
			style.Colors[(int)ImGuiCol.Border] = Colors.Black.ToVector4();
			style.Colors[(int)ImGuiCol.CheckMark] = Settings.Colors.AccentColor;
			style.Colors[(int)ImGuiCol.Button] = Settings.Colors.AccentColor;
			style.Colors[(int)ImGuiCol.ButtonActive] = new Vector4(Settings.Colors.AccentColor.X, Settings.Colors.AccentColor.Y, Settings.Colors.AccentColor.Z, 0.4f);
			style.Colors[(int)ImGuiCol.ButtonHovered] = new Vector4(Settings.Colors.AccentColor.X, Settings.Colors.AccentColor.Y, Settings.Colors.AccentColor.Z, 0.8f);
			style.Colors[(int)ImGuiCol.Header] = Settings.Colors.AccentColor;
			style.Colors[(int)ImGuiCol.HeaderHovered] = new Vector4(Settings.Colors.AccentColor.X, Settings.Colors.AccentColor.Y, Settings.Colors.AccentColor.Z, 0.4f);
			style.Colors[(int)ImGuiCol.HeaderActive] = Settings.Colors.AccentColor;
			style.Colors[(int)ImGuiCol.ResizeGrip] = Settings.Colors.AccentColor;
			style.Colors[(int)ImGuiCol.ResizeGripActive] = new Vector4(Settings.Colors.AccentColor.X, Settings.Colors.AccentColor.Y, Settings.Colors.AccentColor.Z, 0.4f);
			style.Colors[(int)ImGuiCol.ResizeGripHovered] = new Vector4(Settings.Colors.AccentColor.X, Settings.Colors.AccentColor.Y, Settings.Colors.AccentColor.Z, 0.8f);
			style.Colors[(int)ImGuiCol.SliderGrab] = Settings.Colors.AccentColor;
			style.Colors[(int)ImGuiCol.SliderGrabActive] = new Vector4(Settings.Colors.AccentColor.X, Settings.Colors.AccentColor.Y, Settings.Colors.AccentColor.Z, 0.4f);
			style.Colors[(int)ImGuiCol.ScrollbarGrab] = Settings.Colors.AccentColor;
			style.Colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(Settings.Colors.AccentColor.X, Settings.Colors.AccentColor.Y, Settings.Colors.AccentColor.Z, 0.4f);
			style.Colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(Settings.Colors.AccentColor.X, Settings.Colors.AccentColor.Y, Settings.Colors.AccentColor.Z, 0.8f);
			if (Crosshair.UseAccent && Settings.Engine.RainbowMode) Settings.Colors.CrosshairColor = Settings.Colors.AccentColor;
		}
	}
}
