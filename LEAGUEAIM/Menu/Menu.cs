using ImGuiNET;
using LEAGUEAIM.Features;
using LEAGUEAIM.Utilities;
using Script_Engine.Cloud;
using System.Diagnostics;
using System.Numerics;

namespace LEAGUEAIM
{
    internal class Menu
	{
		public static Vector2 Position = new();
		public static Vector2 Size = new();
		public static void Render()
		{
			if (!LoginHelper.IsReady) return;

			LARenderer.ApplyAccentColor();

			if (!Settings.Engine.IsVisible) return;

			if (ImGui.Begin("LEAGUEAIM", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoNavInputs))
			{
				Position = ImGui.GetWindowPos();
				Size = ImGui.GetWindowSize();

				Drawing.TextHeader("LEAGUEAIM", 2f, 5);
				if (Drawing.ProfileIcon(Settings.Engine.AvatarPath, 36.0f))
					CloudMenu.Enabled = !CloudMenu.Enabled;

				ImGui.Spacing();

				// create tabs
				ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(15, 15));
				if (ImGui.BeginTabBar("##TABS", ImGuiTabBarFlags.None))
				{
					// CREATE RECOIL TAB
					if (ImGui.BeginTabItem("Recoil"))
					{
						Recoil.Instance.Render();

						ImGui.EndTabItem();
					}

					// CREATE RAPIDFIRE TAB
					if (ImGui.BeginTabItem("Rapidfire"))
					{
						Rapidfire.Instance.Render();

						ImGui.EndTabItem();
					}

					// CREATE JITTERAIM TAB
					if (ImGui.BeginTabItem("Jitter"))
					{
						Jitter.Instance.Render();

						ImGui.EndTabItem();
					}

					// CREATE EXTRAS TAB
					if (ImGui.BeginTabItem("Extras"))
					{
						if (ImGui.BeginTabBar("##SCRIPTS", ImGuiTabBarFlags.None))
						{
							//if (ImGui.BeginTabItem("Quick Peek"))
							//{
							//	QuickPeek.Instance.Render();

							//	ImGui.EndTabItem();
							//}

							if (ImGui.BeginTabItem("Crosshair"))
							{
								Crosshair.Instance.Render();

								ImGui.EndTabItem();
							}

							if (ImGui.BeginTabItem("Logitech"))
							{
								LuaEngine.Render();

								ImGui.EndTabItem();
							}

							ImGui.EndTabBar();
						}

						ImGui.EndTabItem();
					}

					// CREATE CONFIGS TAB
					if (ImGui.BeginTabItem("Profiles"))
					{
						Profiles.Render();

						ImGui.EndTabItem();
					}

					// CREATE SETTINGS TAB
					if (ImGui.BeginTabItem("Settings"))
					{
						if (ImGui.BeginTabBar("##SETTINGS", ImGuiTabBarFlags.None))
						{
							MenuSettings.Render();

							ImGui.EndTabBar();
						}

						ImGui.EndTabItem();
					}

					ImGui.EndTabBar();
				}

				Drawing.Footer();

				Drawing.AccentBar();

				ImGui.End();
			}
		}
	}
}
