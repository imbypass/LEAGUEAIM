﻿using ImGuiNET;
using LEAGUEAIM.Features;
using LEAGUEAIM.Utilities;
using Script_Engine.Utilities;
using System.Numerics;

namespace LEAGUEAIM
{
	internal class MenuSettings
	{
		public static string Base = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LEAGUEAIM");
		public static readonly string[] ControllerSlots =
		[
			"Slot 1",
			"Slot 2",
			"Slot 3",
			"Slot 4",
		];
		public static readonly string[] InputMethods =
		[
			"Windows API (System)",
			"Interception (Driver)",
			//"Logitech G Hub (Safest)"
		];
		internal static readonly string[] CursorChecks =
		[
			"Is Visible",
			"Is At Center"
		];
		public static void Render()
		{
			if (ImGui.BeginTabItem("Settings"))
			{
				ImGui.Text("Settings");

				ImGui.Checkbox("Controller Support", ref Settings.Controller.Enabled);
				Drawing.Tooltip($"Enable LEAGUEAIM to read controller inputs.", Settings.Colors.AccentColor);
				if (Settings.Controller.Enabled)
				{
					ImGui.SameLine();
					if (ImGui.Button("Configure"))
					{
						ImGui.OpenPopup("Controller Settings");
					}
					if (ImGui.BeginPopup("Controller Settings"))
					{
						ImGui.Text("Controller Settings");
						ImGui.Checkbox("Flip Bumpers and Triggers", ref Settings.Controller.FlippedTriggers);
						Drawing.Tooltip("Flip the bumpers and triggers on your controller.", Settings.Colors.AccentColor);
						ImGui.SliderFloat("##PollingRate", ref Settings.Controller.PollingRate, 8, 1000, "%.0f", ImGuiSliderFlags.AlwaysClamp);
						ImGui.SameLine();
						ImGui.Text("Polling Rate (ms)");
						Drawing.Tooltip("How often LEAGUEAIM will read controller inputs.", Settings.Colors.AccentColor);
						if (ImGui.Combo("##CONTROLLERLIST", ref Settings.Controller.Index, ControllerSlots, ControllerSlots.Length, ControllerSlots.Length))
						{
							Engine.ControllerThread.Interrupt();
							Engine.ControllerThread = new Thread(Engine.ControllerLoop) { IsBackground = true, Priority = ThreadPriority.Lowest };
							Engine.ControllerThread.Start();
						}
						ImGui.SameLine();
						ImGui.Text("Controller Slot");

						ImGui.EndPopup();
					}
				}

				ImGui.Checkbox("Stream-Proof", ref Settings.Engine.StreamProof);
				Drawing.Tooltip($"Disable rendering LEAGUEAIM in OBS.", Settings.Colors.AccentColor);

				Watermark.Instance.Render();

				ImGui.Checkbox("Cursor Check", ref Settings.Engine.CursorCheck);
				Drawing.Tooltip($"Prevent LEAGUEAIM from running while the cursor is visible.", Settings.Colors.AccentColor);
				if (Settings.Engine.CursorCheck)
				{
					ImGui.SameLine();
					ImGui.SetNextItemWidth(150);
					ImGui.Combo("##CURSORLIST", ref Settings.Engine.CursorCheckType, CursorChecks, CursorChecks.Length);
				}

				ImGui.Separator();
				ImGui.Text("Input Method");
				Drawing.Tooltip($"LEAGUEAIM can use different input methods to attempt to be safer.", Settings.Colors.AccentColor);
				if (ImGui.Combo("##INPUTLIST", ref Settings.Engine.InputMethod, InputMethods, InputMethods.Length, InputMethods.Length))
				{
					switch (Settings.Engine.InputMethod) // User selected Interception driver
					{
						case 1:
							if (!InputInterceptorNS.InputInterceptor.CheckDriverInstalled())
							{
								ImGui.OpenPopup("Interception Driver");
							}
							break;
						case 2:
							if (!Settings.Engine.HasGhub)
							{
								MessageBox.Show("fuck");
							}
							break;
					}
				}
				bool driver = true;
				Vector2 cMenuPos = ImGui.GetWindowPos();
				Vector2 cMenuSize = ImGui.GetWindowSize();
				ImGui.SetNextWindowPos(new(cMenuPos.X + (cMenuSize.X / 2) - 175, cMenuPos.Y + (cMenuSize.Y / 2) - 55));
				ImGui.SetNextWindowSize(new(350, 110));
				ImGui.PushStyleColor(ImGuiCol.Border, Settings.Colors.AccentColor);
				ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1.5f);
				if (ImGui.BeginPopupModal("Interception Driver", ref driver, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoTitleBar))
				{
					ImGui.Spacing();
					Drawing.TextCentered($"The Interception driver was not found on this machine.\nWould you like to install it now?");
					ImGui.Spacing();
					ImGui.SetCursorPos(new Vector2((ImGui.GetWindowSize().X - 68) * 0.3f, ImGui.GetCursorPosY()));
					if (ImGui.Button("Yes", new(68, 28)))
					{
						InputInterceptorNS.InputInterceptor.InstallDriver();
						ImGui.CloseCurrentPopup();
						ImGui.OpenPopup("Interception Installed");
					}
					ImGui.SameLine();
					ImGui.SetCursorPos(new Vector2((ImGui.GetWindowSize().X - 68) * 0.6f, ImGui.GetCursorPosY()));
					if (ImGui.Button("No", new(68, 28)))
					{
						Settings.Engine.InputMethod = 0;
						ImGui.CloseCurrentPopup();
					}
					ImGui.Spacing();

					ImGui.EndPopup();
				}
				ImGui.PopStyleColor();
				ImGui.PopStyleVar();

				bool installed = true;
				ImGui.SetNextWindowPos(new(cMenuPos.X + (cMenuSize.X / 2) - 175, cMenuPos.Y + (cMenuSize.Y / 2) - 55));
				ImGui.SetNextWindowSize(new(300, 110));
				ImGui.PushStyleColor(ImGuiCol.Border, Settings.Colors.AccentColor);
				ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1.5f);
				if (ImGui.BeginPopupModal("Interception Installed", ref installed, ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoTitleBar))
				{
					ImGui.Spacing();
					Drawing.TextCentered("Interception was successfully installed!\nPlease reboot your PC to complete the process.");
					ImGui.Spacing();
					ImGui.SetCursorPos(new Vector2((ImGui.GetWindowSize().X - 68) * 0.5f, ImGui.GetCursorPosY()));
					if (ImGui.Button("Dismiss", new(68, 28)))
					{
						ImGui.CloseCurrentPopup();
					}

					ImGui.EndPopup();
				}
				ImGui.PopStyleColor();
				ImGui.PopStyleVar();
				ImGui.Separator();
				ImGui.Text("Keybinding");
				Keybinds.Instance.Render();
				ImGui.Separator();
				ImGui.Text("Menu Settings");
				if (Drawing.IconButton("Save", IconFonts.FontAwesome6.FloppyDisk, new(115, 28), true, ImGui.GetStyle().FrameRounding, 0))
				{
					MenuSettings.SaveMenuSettings();
					MenuSettings.SaveKeybinds();
					ImGui.OpenPopup("Settings Saved");
				}
				ImGui.SameLine();
				if (Drawing.IconButton("Load", IconFonts.FontAwesome6.FolderOpen, new(115, 28), true, ImGui.GetStyle().FrameRounding, 0))
				{
					MenuSettings.LoadMenuSettings();
					MenuSettings.LoadKeybinds();
					ImGui.OpenPopup("Settings Loaded");
				}

				if (Drawing.IconButton("Switch Account", IconFonts.FontAwesome6.Users, new(240, 28), true, ImGui.GetStyle().FrameRounding, 0))
				{
					ImGui.OpenPopup("Switch Account");
				}

				bool saved = true;
				ImGui.SetNextWindowPos(new(cMenuPos.X + (cMenuSize.X / 2) - 150, cMenuPos.Y + (cMenuSize.Y / 2) - 55));
				ImGui.SetNextWindowSize(new(300, 110));
				ImGui.PushStyleColor(ImGuiCol.Border, Settings.Colors.AccentColor);
				ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1.5f);
				if (ImGui.BeginPopupModal("Settings Saved", ref saved, ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoTitleBar))
				{
					ImGui.Spacing();
					Drawing.TextCentered("Menu settings saved!");
					ImGui.Spacing();
					ImGui.SetCursorPos(new Vector2((ImGui.GetWindowSize().X - 68) * 0.5f, ImGui.GetCursorPosY()));
					if (ImGui.Button("Dismiss", new(68, 28)))
					{
						ImGui.CloseCurrentPopup();
					}

					ImGui.EndPopup();
				}
				ImGui.PopStyleVar();
				ImGui.PopStyleColor();
				bool loaded = true;
				ImGui.SetNextWindowPos(new(cMenuPos.X + (cMenuSize.X / 2) - 150, cMenuPos.Y + (cMenuSize.Y / 2) - 55));
				ImGui.SetNextWindowSize(new(300, 110));
				ImGui.PushStyleColor(ImGuiCol.Border, Settings.Colors.AccentColor);
				ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1.5f);
				if (ImGui.BeginPopupModal("Settings Loaded", ref loaded, ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoTitleBar))
				{
					ImGui.Spacing();
					Drawing.TextCentered("Menu settings loaded!");
					ImGui.Spacing();
					ImGui.SetCursorPos(new Vector2((ImGui.GetWindowSize().X - 68) * 0.5f, ImGui.GetCursorPosY()));
					if (ImGui.Button("Dismiss", new(68, 28)))
					{
						ImGui.CloseCurrentPopup();
					}

					ImGui.EndPopup();
				}
				ImGui.PopStyleVar();
				ImGui.PopStyleColor();
				bool acct = true;
				ImGui.SetNextWindowPos(new(cMenuPos.X + (cMenuSize.X / 2) - 175, cMenuPos.Y + (cMenuSize.Y / 2) - 55));
				ImGui.SetNextWindowSize(new(350, 110));
				ImGui.PushStyleColor(ImGuiCol.Border, Settings.Colors.AccentColor);
				ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1.5f);
				if (ImGui.BeginPopupModal("Switch Account", ref acct, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoTitleBar))
				{
					ImGui.Spacing();
					Drawing.TextCentered($"Are you sure you want to switch accounts?");
					ImGui.Spacing();
					ImGui.SetCursorPos(new Vector2((ImGui.GetWindowSize().X - 68) * 0.3f, ImGui.GetCursorPosY()));
					if (ImGui.Button("Yes", new(68, 28)))
					{
						LoginHelper.SaveCredentials(string.Empty, string.Empty);
						LoginHelper.IsSaved = false;
						LoginHelper.IsReady = false;
						Application.Restart();
						Environment.Exit(0);
					}
					ImGui.SameLine();
					ImGui.SetCursorPos(new Vector2((ImGui.GetWindowSize().X - 68) * 0.6f, ImGui.GetCursorPosY()));
					if (ImGui.Button("No", new(68, 28)))
					{
						ImGui.CloseCurrentPopup();
					}
					ImGui.Spacing();

					ImGui.EndPopup();
				}
				ImGui.PopStyleColor();
				ImGui.PopStyleVar();

				ImGui.EndTabItem();
			}

			if (ImGui.BeginTabItem("Menu Style"))
			{
				ImGui.Text("Menu Style");

				ImGui.Checkbox("Rainbow Mode", ref Settings.Engine.RainbowMode);
				Drawing.Tooltip($"Give your accent color a smooth RGB cycle.", Settings.Colors.AccentColor);
				if (Settings.Engine.RainbowMode)
				{
					ImGui.SameLine();
					if (ImGui.Button("Configure"))
					{
						ImGui.OpenPopup("Rainbow Settings");
					}
					if (ImGui.BeginPopup("Rainbow Settings"))
					{
						ImGui.Text("Rainbow Settings");
						ImGui.SliderInt("##FadeSpeed", ref Settings.Engine.RainbowSpeed, 1, 100);
						ImGui.SameLine();
						ImGui.Text("Fade Speed");
						Drawing.Tooltip("How fast LEAGUEAIM cycles the RGB fade animation.", Settings.Colors.AccentColor);

						ImGui.EndPopup();
					}
				}

				ImGui.Separator();
				ImGui.Text("Menu Colors");

				Drawing.ColorPicker("Accent Color", ref Settings.Colors.AccentColor, new ImGuiCol[] { ImGuiCol.CheckMark, ImGuiCol.Border, ImGuiCol.SliderGrab, ImGuiCol.Header, ImGuiCol.HeaderActive, ImGuiCol.HeaderHovered, ImGuiCol.Button, ImGuiCol.ButtonActive, ImGuiCol.ButtonHovered });

				Drawing.ColorPicker("Background Color", ref Settings.Colors.BgColor, new ImGuiCol[] { ImGuiCol.WindowBg, ImGuiCol.ChildBg, ImGuiCol.PopupBg });

				Drawing.ColorPicker("Frame Color", ref Settings.Colors.FrameColor, new ImGuiCol[] { ImGuiCol.FrameBg, ImGuiCol.FrameBgHovered, ImGuiCol.FrameBgActive });

				Drawing.ColorPicker("Text Color", ref Settings.Colors.TextColor, new ImGuiCol[] { ImGuiCol.Text });

				ImGui.EndTabItem();
			}
		}
		public static void SaveMenuSettings()
		{
			Logger.WriteLine("Saving menu settings..");
			IniFile iniFile = new(Path.Combine(Base, "MenuSettings.ini"));

			iniFile.Write("StreamProof", Settings.Engine.StreamProof.ToString(), "General");
			iniFile.Write("ShowFooter", Settings.Engine.ShowFooter.ToString(), "General");
			iniFile.Write("ShowWatermark", Watermark.Instance.Enabled.ToString(), "General");
			iniFile.Write("CursorCheck", Settings.Engine.CursorCheck.ToString(), "General");
			iniFile.Write("CursorCheckType", Settings.Engine.CursorCheckType.ToString(), "General");
			iniFile.Write("KeyToggleMenu", ((int)Keybinds.MenuKeys[0]).ToString(), "General");
			iniFile.Write("KeyTerminate", ((int)Keybinds.MenuKeys[1]).ToString(), "General");
			iniFile.Write("ControllerSupport", Settings.Controller.Enabled.ToString(), "General");
			iniFile.Write("ControllerSlot", Settings.Controller.Index.ToString(), "General");
			iniFile.Write("FlippedTriggers", Settings.Controller.FlippedTriggers.ToString(), "General");
			iniFile.Write("PollingRate", Settings.Controller.PollingRate.ToString(), "General");
			iniFile.Write("InputMethod", Settings.Engine.InputMethod.ToString(), "General");

			iniFile.Write("RainbowMode", Settings.Engine.RainbowMode.ToString(), "Style");
			iniFile.Write("AccentColor", Settings.Colors.AccentColor.ToUInt32().ToString(), "Style");
			iniFile.Write("FrameColor", Settings.Colors.FrameColor.ToUInt32().ToString(), "Style");
			iniFile.Write("TextColor", Settings.Colors.TextColor.ToUInt32().ToString(), "Style");
			iniFile.Write("BgColor", Settings.Colors.BgColor.ToUInt32().ToString(), "Style");
			iniFile.Write("RainbowSpeed", Settings.Engine.RainbowSpeed.ToString(), "Style");

		}
		public static void LoadMenuSettings()
		{
			Logger.WriteLine("Loading menu settings..");
			IniFile iniFile = new(Path.Combine(Base, "MenuSettings.ini"));

			if (!File.Exists(Path.Combine(Base, "MenuSettings.ini")))
				SaveMenuSettings();

			if (iniFile.KeyExists("KeyToggleMenu", "General"))
				Keybinds.MenuKeys[0] = (Keys)iniFile.Read<int>("KeyToggleMenu", "General");

			if (iniFile.KeyExists("KeyTerminate", "General"))
				Keybinds.MenuKeys[1] = (Keys)iniFile.Read<int>("KeyTerminate", "General");

			if (iniFile.KeyExists("ShowWatermark", "General"))
				Watermark.Instance.Enabled = iniFile.Read<bool>("ShowWatermark", "General");

			if (iniFile.KeyExists("StreamProof", "General"))
				Settings.Engine.StreamProof = iniFile.Read<bool>("StreamProof", "General");

			if (iniFile.KeyExists("ShowFooter", "General"))
				Settings.Engine.ShowFooter = iniFile.Read<bool>("ShowFooter", "General");
			
			if (iniFile.KeyExists("CursorCheck", "General"))
				Settings.Engine.CursorCheck = iniFile.Read<bool>("CursorCheck", "General");

			if (iniFile.KeyExists("CursorCheckType", "General"))
				Settings.Engine.CursorCheckType = iniFile.Read<int>("CursorCheckType", "General");

			if (iniFile.KeyExists("ControllerSupport", "General"))
				Settings.Controller.Enabled = iniFile.Read<bool>("ControllerSupport", "General");

			if (iniFile.KeyExists("ControllerSlot", "General"))
				Settings.Controller.Index = iniFile.Read<int>("ControllerSlot", "General");

			if (iniFile.KeyExists("FlippedTriggers", "General"))
				Settings.Controller.FlippedTriggers = iniFile.Read<bool>("FlippedTriggers", "General");

			if (iniFile.KeyExists("PollingRate", "General"))
				Settings.Controller.PollingRate = iniFile.Read<int>("PollingRate", "General");

			if (iniFile.KeyExists("InputMethod", "General"))
				Settings.Engine.InputMethod = iniFile.Read<int>("InputMethod", "General");

			if (iniFile.KeyExists("RainbowMode", "Style"))
				Settings.Engine.RainbowMode = iniFile.Read<bool>("RainbowMode", "Style");

			if (iniFile.KeyExists("RainbowSpeed", "Style"))
				Settings.Engine.RainbowSpeed = iniFile.Read<int>("RainbowSpeed", "Style");

			if (iniFile.KeyExists("AccentColor", "Style"))
				Settings.Colors.AccentColor = iniFile.Read<uint>("AccentColor", "Style").ToFloat4();

			if (iniFile.KeyExists("FrameColor", "Style"))
				Settings.Colors.FrameColor = iniFile.Read<uint>("FrameColor", "Style").ToFloat4();

			if (iniFile.KeyExists("TextColor", "Style"))
				Settings.Colors.TextColor = iniFile.Read<uint>("TextColor", "Style").ToFloat4();

			if (iniFile.KeyExists("BgColor", "Style"))
				Settings.Colors.BgColor = iniFile.Read<uint>("BgColor", "Style").ToFloat4();

			try { LARenderer.ApplyStyle(); } catch { }
		}
		public static void SaveKeybinds()
		{
			Logger.WriteLine("Saving keybinds..");
			IniFile iniFile = new(Path.Combine(Base, "Keybinds.ini"));

			iniFile.Write("Rapidfire", ((int)Rapidfire.KeyActivation).ToString(), "Keybinds");
			iniFile.Write("LeanLeft", ((int)QuickPeek.KeyLeanLeft).ToString(), "Keybinds");
			iniFile.Write("LeanRight", ((int)QuickPeek.KeyLeanRight).ToString(), "Keybinds");
			iniFile.Write("PeekLeft", ((int)QuickPeek.KeyPeekLeft).ToString(), "Keybinds");
			iniFile.Write("PeekRight", ((int)QuickPeek.KeyPeekRight).ToString(), "Keybinds");
		}
		public static void LoadKeybinds()
		{
			Logger.WriteLine("Loading keybinds..");
			IniFile iniFile = new(Path.Combine(Base, "Keybinds.ini"));

			if (!File.Exists(Path.Combine(Base, "Keybinds.ini")))
				SaveKeybinds();

			try
			{
				if (iniFile.KeyExists("Rapidfire", "Keybinds"))
					Rapidfire.KeyActivation = (Keys)iniFile.Read<int>("Rapidfire", "Keybinds");

				if (iniFile.KeyExists("LeanLeft", "Keybinds"))
					QuickPeek.KeyLeanLeft = (Keys)iniFile.Read<int>("LeanLeft", "Keybinds");

				if (iniFile.KeyExists("LeanRight", "Keybinds"))
					QuickPeek.KeyLeanRight = (Keys)iniFile.Read<int>("LeanRight", "Keybinds");

				if (iniFile.KeyExists("PeekLeft", "Keybinds"))
					QuickPeek.KeyPeekLeft = (Keys)iniFile.Read<int>("PeekLeft", "Keybinds");

				if (iniFile.KeyExists("PeekRight", "Keybinds"))
					QuickPeek.KeyPeekRight = (Keys)iniFile.Read<int>("PeekRight", "Keybinds");

			}
			catch
			{
				File.Delete(Path.Combine(Base, "Keybinds.ini"));
				LoadKeybinds();
			}
		}
	}
}