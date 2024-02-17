using ImGuiNET;
using LEAGUEAIM.Utilities;
using Script_Engine.Cloud;
using Script_Engine.Utilities;
using System.Diagnostics;
using System.Numerics;
using static LEAGUEAIM.Features.Recoil;

namespace LEAGUEAIM.Features
{
	internal class Profiles
	{
		public static string Base = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LEAGUEAIM");
		public static string Location = Path.Combine(Base, "profiles");
		public static string CurrentProfile = "None";
		public static string[] ProfileList;
		public static string[] GetProfileList()
		{
			if (!Directory.Exists(Location)) Directory.CreateDirectory(Location);

			string[] profiles = [];
			string[] files = Directory.GetFiles(Location);
			foreach (string file in files)
			{
				if (file.EndsWith(".ini") && file.Replace(".ini", "").Length > 0)
				{
					Array.Resize(ref profiles, profiles.Length + 1);
					profiles[^1] = file.Replace(Location, "").Replace(".ini", "")[1..];

				}
			}

			if (profiles.Length > 0 && Settings.Menu.CurrentConfig == -1)
				Settings.Menu.CurrentConfig = 0;

			return profiles;
		}
		public static int GetProfileIndex(string name)
		{
			string[] profiles = GetProfileList();
			for (int i = 0; i < profiles.Length; i++)
			{
				if (profiles[i] == name)
					return i;
			}
			return 0;
		}
		public static void LoadProfile(string name)
		{
			Logger.Write($"Loading profile: ");
			Color accent = Settings.Colors.AccentColor.ToColor();
			Logger.Text.FromColor(accent);
			Console.WriteLine(name);
			Logger.Text.Reset();

			if (name.EndsWith(".ini"))
				name = name.Replace(".ini", "");

			IniFile config = new(Path.Combine(Location, name + ".ini"));

			if (config.KeyExists("Enabled", "Recoil")) Recoil.Instance.Enabled = config.Read<bool>("Enabled", "Recoil");
			if (config.KeyExists("RequireADS", "Recoil")) Recoil.RequireADS = config.Read<bool>("RequireADS", "Recoil");
			if (config.KeyExists("RecoilX", "Recoil")) Recoil.Linear.HorizontalStrength = config.Read<float>("RecoilX", "Recoil");
			if (config.KeyExists("RecoilY", "Recoil")) Recoil.Linear.VerticalStrength = config.Read<float>("RecoilY", "Recoil");
			if (config.KeyExists("SleepTime", "Recoil")) Recoil.Linear.TimeSleep = config.Read<float>("SleepTime", "Recoil");
			if (config.Read<bool>("InvertY", "Recoil"))
			{
				if (Recoil.Linear.VerticalStrength > 0)
					Recoil.Linear.VerticalStrength *= -1;
			}
			if (config.Read<bool>("InvertX", "Recoil"))
			{
				if (Recoil.Linear.HorizontalStrength > 0)
					Recoil.Linear.HorizontalStrength *= -1;
			}

			if (config.KeyExists("RecoilType", "Recoil")) Recoil.RecoilType = config.Read<int>("RecoilType", "Recoil");
			if (config.KeyExists("RecoilPattern", "Recoil")) Recoil.Pattern.PatternName = config.Read<string>("RecoilPattern", "Recoil");

			if (Recoil.Pattern.PatternName != null)
				Recoil.Pattern.CurrentPattern = Recoil.GetPatternIndex(Recoil.Pattern.PatternName);

			if (config.KeyExists("PatternMultiplierX", "Recoil")) Recoil.Pattern.XMultiplier = config.Read<float>("PatternMultiplierX", "Recoil");
			if (config.KeyExists("PatternMultiplierY", "Recoil")) Recoil.Pattern.YMultiplier = config.Read<float>("PatternMultiplierY", "Recoil");

			if (config.KeyExists("Enabled", "RapidFire")) Rapidfire.Instance.Enabled = config.Read<bool>("Enabled", "RapidFire");
			if (config.KeyExists("RequireADS", "RapidFire")) Rapidfire.RequireADS = config.Read<bool>("RequireADS", "RapidFire");
			if (config.KeyExists("Primary", "RapidFire")) Rapidfire.ActivateOnPrimary = config.Read<bool>("Primary", "RapidFire");
			if (config.KeyExists("PullDown", "RapidFire")) Rapidfire.VerticalStrength = config.Read<float>("PullDown", "RapidFire");
			if (config.KeyExists("UseRecoil", "RapidFire")) Rapidfire.FollowRecoil = config.Read<bool>("UseRecoil", "RapidFire");
			if (config.Read<bool>("Invert", "RapidFire"))
			{
				if (Rapidfire.VerticalStrength > 0)
					Rapidfire.VerticalStrength *= -1;
			}

			if (config.KeyExists("Enabled", "Jitter")) Jitter.Instance.Enabled = config.Read<bool>("Enabled", "Jitter");
			if (config.KeyExists("RequireADS", "Jitter")) Jitter.RequireADS = config.Read<bool>("RequireADS", "Jitter");
			if (config.KeyExists("JitterX", "Jitter")) Jitter.HorizontalStrength = config.Read<float>("JitterX", "Jitter");
			if (config.KeyExists("JitterY", "Jitter")) Jitter.VerticalStrength = config.Read<float>("JitterY", "Jitter");
			if (config.KeyExists("SleepTime", "Jitter")) Jitter.TimeSleep = config.Read<float>("SleepTime", "Jitter");

			string keybind = config.Read("Hotkey", "RapidFire");
			if (!int.TryParse(keybind, out int key)) Rapidfire.KeyActivation = (Keys)InputKeys.KeyCodeFromName(keybind);
			else Rapidfire.KeyActivation = (Keys)key;

			if (config.KeyExists("AutoNade", "Extras")) AutoNade.Instance.Enabled = config.Read<bool>("AutoNade", "Extras");
			if (config.KeyExists("NadeCookTime", "Extras")) AutoNade.TimeDelay = config.Read<float>("NadeCookTime", "Extras");
			if (config.KeyExists("QuickPeek", "Extras")) QuickPeek.Instance.Enabled = config.Read<bool>("QuickPeek", "Extras");

			// attempt to load quickpeek settings
			try
			{
				if (config.KeyExists("QuickPeekDelayIn", "Extras")) QuickPeek.DelayIn = config.Read<int>("QuickPeekDelayIn", "Extras");
				if (config.KeyExists("QuickPeekDelayOut", "Extras")) QuickPeek.DelayOut = config.Read<int>("QuickPeekDelayOut", "Extras");
			}
			catch { }

			// attempt to load crosshair settings
			try
			{
				if (config.KeyExists("ENABLED", "CROSSHAIR")) Crosshair.Instance.Enabled = config.Read<bool>("ENABLED", "CROSSHAIR");
				if (config.KeyExists("SHOWONADS", "CROSSHAIR")) Crosshair.ShowOnADS = config.Read<bool>("SHOWONADS", "CROSSHAIR");
				if (config.KeyExists("HIDEONADS", "CROSSHAIR")) Crosshair.HideOnADS = config.Read<bool>("HIDEONADS", "CROSSHAIR");
				if (config.KeyExists("SIZE", "CROSSHAIR")) Crosshair.Size = config.Read<float>("SIZE", "CROSSHAIR");
				if (config.KeyExists("THICKNESS", "CROSSHAIR")) Crosshair.Thickness = config.Read<float>("THICKNESS", "CROSSHAIR");
				if (config.KeyExists("GAP", "CROSSHAIR")) Crosshair.Gap = config.Read<float>("GAP", "CROSSHAIR");
				if (config.KeyExists("TOP", "CROSSHAIR")) Crosshair.DrawTop = config.Read<bool>("TOP", "CROSSHAIR");
				if (config.KeyExists("DOT", "CROSSHAIR")) Crosshair.CenterDot = config.Read<bool>("DOT", "CROSSHAIR");
				if (config.KeyExists("DOTSIZE", "CROSSHAIR")) Crosshair.DotSize = config.Read<float>("DOTSIZE", "CROSSHAIR");

				if (config.KeyExists("COLOR_R", "CROSSHAIR") && config.KeyExists("COLOR_G", "CROSSHAIR") && config.KeyExists("COLOR_B", "CROSSHAIR") && config.KeyExists("ALPHA", "CROSSHAIR"))
				{
					Settings.Colors.CrosshairColor = new(
						config.Read<float>("COLOR_R", "CROSSHAIR"),
						config.Read<float>("COLOR_G", "CROSSHAIR"),
						config.Read<float>("COLOR_B", "CROSSHAIR"),
						config.Read<float>("ALPHA", "CROSSHAIR")
					);
				}


				if (int.TryParse(config.Read("STYLE", "CROSSHAIR"), out int style))
					Crosshair.Style = config.Read<int>("STYLE", "CROSSHAIR");
				if (config.KeyExists("RAINBOW", "CROSSHAIR"))
					Crosshair.UseAccent = config.Read<bool>("RAINBOW", "CROSSHAIR");
			}
			catch { }

			Settings.Menu.CurrentHotkey = InputKeys.KeyCodeFromName(config.Read("Hotkey", "Settings").ToLower());
			CurrentProfile = name;
		}
		private static void SaveProfile(string name)
		{
			Logger.Write($"Saving profile: ");
			Color accent = Settings.Colors.AccentColor.ToColor();
			Logger.Text.FromColor(accent);
			Console.WriteLine(name);
			Logger.Text.Reset();

			if (name.EndsWith(".ini"))
				name = name.Replace(".ini", "");

			IniFile config = new(Path.Combine(Location, name + ".ini"));

			config.Write("Enabled", Recoil.Instance.Enabled.ToString(), "Recoil");
			config.Write("RequireADS", Recoil.RequireADS.ToString(), "Recoil");
			config.Write("RecoilX", Recoil.Linear.HorizontalStrength.ToString(), "Recoil");
			config.Write("RecoilY", Recoil.Linear.VerticalStrength.ToString(), "Recoil");
			config.Write("SleepTime", Recoil.Linear.TimeSleep.ToString(), "Recoil");
			config.Write("InvertX", false.ToString(), "Recoil");
			config.Write("InvertY", false.ToString(), "Recoil");
			config.Write("RecoilType", Recoil.RecoilType.ToString(), "Recoil");
			config.Write("RecoilPattern", Recoil.GetPatterns()[Recoil.Pattern.CurrentPattern].ToString(), "Recoil");
			config.Write("PatternMultiplierX", Recoil.Pattern.XMultiplier.ToString(), "Recoil");
			config.Write("PatternMultiplierY", Recoil.Pattern.YMultiplier.ToString(), "Recoil");

			config.Write("Enabled", Rapidfire.Instance.Enabled.ToString(), "RapidFire");
			config.Write("RequireADS", Rapidfire.RequireADS.ToString(), "RapidFire");
			config.Write("Primary", Rapidfire.ActivateOnPrimary.ToString(), "RapidFire");
			config.Write("PullDown", Rapidfire.VerticalStrength.ToString(), "RapidFire");
			config.Write("Hotkey", ((int)Rapidfire.KeyActivation).ToString(), "RapidFire");
			config.Write("UseRecoil", Rapidfire.FollowRecoil.ToString(), "RapidFire");
			config.Write("FireTime", Rapidfire.TimeFire.ToString(), "RapidFire");
			config.Write("Invert", false.ToString(), "RapidFire");

			config.Write("Enabled", Jitter.Instance.Enabled.ToString(), "Jitter");
			config.Write("RequireADS", Jitter.RequireADS.ToString(), "Jitter");
			config.Write("JitterX", Jitter.HorizontalStrength.ToString(), "Jitter");
			config.Write("JitterY", Jitter.VerticalStrength.ToString(), "Jitter");
			config.Write("SleepTime", Jitter.TimeSleep.ToString(), "Jitter");

			config.Write("AutoNade", AutoNade.Instance.Enabled.ToString(), "Extras");
			config.Write("NadeCookTime", AutoNade.TimeDelay.ToString(), "Extras");
			config.Write("QuickPeek", QuickPeek.Instance.Enabled.ToString(), "Extras");
			config.Write("QuickPeekDelayIn", QuickPeek.DelayIn.ToString(), "Extras");
			config.Write("QuickPeekDelayOut", QuickPeek.DelayOut.ToString(), "Extras");

			config.Write("ENABLED", Crosshair.Instance.Enabled.ToString(), "CROSSHAIR");
			config.Write("SHOWONADS", Crosshair.ShowOnADS.ToString(), "CROSSHAIR");
			config.Write("HIDEONADS", Crosshair.HideOnADS.ToString(), "CROSSHAIR");
			config.Write("SIZE", Crosshair.Size.ToString(), "CROSSHAIR");
			config.Write("THICKNESS", Crosshair.Thickness.ToString(), "CROSSHAIR");
			config.Write("GAP", Crosshair.Gap.ToString(), "CROSSHAIR");
			config.Write("TOP", Crosshair.DrawTop.ToString(), "CROSSHAIR");
			config.Write("DOT", Crosshair.CenterDot.ToString(), "CROSSHAIR");
			config.Write("DOTSIZE", Crosshair.DotSize.ToString(), "CROSSHAIR");
			Vector4 cCol = Settings.Colors.CrosshairColor;
			config.Write("COLOR_R", cCol.X.ToString(), "CROSSHAIR");
			config.Write("COLOR_G", cCol.Y.ToString(), "CROSSHAIR");
			config.Write("COLOR_B", cCol.Z.ToString(), "CROSSHAIR");
			config.Write("ALPHA", cCol.W.ToString(), "CROSSHAIR");
			config.Write("STYLE", Crosshair.Style.ToString(), "CROSSHAIR");
			config.Write("RAINBOW", Crosshair.UseAccent.ToString(), "CROSSHAIR");
		}
		private static void CreateProfile(string name)
		{
			string configPath = Path.Combine(Location, name + ".ini");

			if (!Directory.Exists(Location))
			{
				Directory.CreateDirectory(Location);
			}
			if (!File.Exists(configPath))
			{
				File.Create(configPath).Close();
			}

			SaveProfile(name);
		}
		private static void DeleteProfile(string name)
		{
			string configPath = Path.Combine(Location, name + ".ini");
			if (File.Exists(configPath)) File.Delete(configPath);
		}
		public static void Render()
		{
			ImGui.Text("Profiles");
			if (Profiles.CurrentProfile != "None") { 
				ImGui.SameLine();
				ImGui.Text("-");
				ImGui.SameLine();
				ImGui.TextColored(Settings.Colors.AccentColor, $"{CurrentProfile}");
			}
			if (ImGui.ListBox("###CONFIGS", ref Settings.Menu.CurrentConfig, Profiles.ProfileList, Profiles.ProfileList.Length, 4))
			{
				Settings.Menu.NewConfigName = Profiles.ProfileList[Settings.Menu.CurrentConfig].Replace(".ini", "");
			}
			if (ImGui.Button("Load", new(73, 28)))
			{
				if (Settings.Menu.CurrentConfig > -1)
				{
					Profiles.CurrentProfile = Profiles.ProfileList[Settings.Menu.CurrentConfig].Replace(".ini", "");
					Profiles.LoadProfile(Profiles.CurrentProfile);
				}
			}
			ImGui.SameLine();
			if (ImGui.Button("Delete", new(73, 28)))
			{
				if (Settings.Menu.CurrentConfig > -1)
					ImGui.OpenPopup("Delete Config");
			}
			ImGui.SameLine();
			if (ImGui.Button("Save", new Vector2(73, 28)))
			{
				if (Settings.Menu.CurrentConfig > -1)
				{
					Profiles.CurrentProfile = Profiles.ProfileList[Settings.Menu.CurrentConfig].Replace(".ini", "");
					Profiles.SaveProfile(Profiles.CurrentProfile);
				}
			}
			if (Drawing.IconButton("Send to Cloud", IconFonts.FontAwesome6.Upload, new(240, 28), true, ImGui.GetStyle().FrameRounding, 0))
			{
				if (Settings.Menu.CurrentConfig > -1)
					ImGui.OpenPopup("Upload Profile");
			}

			Vector2 cMenuPos;
			Vector2 cMenuSize;

			cMenuPos = ImGui.GetWindowPos();
			cMenuSize = ImGui.GetWindowSize();
			bool delete = true;
			ImGui.SetNextWindowPos(new(cMenuPos.X + (cMenuSize.X / 2) - (375/2), cMenuPos.Y + (cMenuSize.Y / 2) - 55));
			ImGui.SetNextWindowSize(new(375, 110));
			ImGui.PushStyleColor(ImGuiCol.Border, Settings.Colors.AccentColor);
			ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1.5f);
			if (ImGui.BeginPopupModal("Delete Config", ref delete, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoTitleBar))
			{
				string profileName = Profiles.ProfileList[Settings.Menu.CurrentConfig].Replace(".ini", "");
				ImGui.Spacing();
				Drawing.TextCentered($"Are you sure you want to delete profile:  {profileName.TruncateWord(12)}?");
				ImGui.Spacing();
				ImGui.SetCursorPos(new Vector2((ImGui.GetWindowSize().X - 68) * 0.333f, ImGui.GetCursorPosY()));
				if (ImGui.Button("Yes", new(68, 28)))
				{
					Profiles.DeleteProfile(profileName);
					ImGui.CloseCurrentPopup();
				}
				ImGui.SameLine();
				ImGui.SetCursorPos(new Vector2((ImGui.GetWindowSize().X - 68) * 0.666f, ImGui.GetCursorPosY()));
				if (ImGui.Button("No", new(68, 28)))
				{
					ImGui.CloseCurrentPopup();
				}

				ImGui.EndPopup();
			}
			ImGui.PopStyleColor();
			ImGui.PopStyleVar();



			cMenuPos = ImGui.GetWindowPos();
			cMenuSize = ImGui.GetWindowSize();
			bool upload_profile = true;
			ImGui.SetNextWindowPos(new(cMenuPos.X + (cMenuSize.X / 2) - 175, cMenuPos.Y + (cMenuSize.Y / 2) - 55));
			ImGui.SetNextWindowSize(new(350, 110));
			ImGui.PushStyleColor(ImGuiCol.Border, Settings.Colors.AccentColor);
			ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1.5f);
			if (ImGui.BeginPopupModal("Upload Profile", ref upload_profile, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoTitleBar))
			{
				string profileName = ProfileList[Settings.Menu.CurrentConfig];
				ImGui.Spacing();
				Drawing.TextCentered($"Do you want to upload \"{profileName}\" to the cloud?");
				ImGui.Spacing();
				ImGui.SetCursorPos(new Vector2((ImGui.GetWindowSize().X - 68) * 0.333f, ImGui.GetCursorPosY()));
				if (ImGui.Button("Yes", new(68, 28)))
				{
					CloudMethods.UploadFile("configs", profileName);
					ImGui.CloseCurrentPopup();
				}
				ImGui.SameLine();
				ImGui.SetCursorPos(new Vector2((ImGui.GetWindowSize().X - 68) * 0.666f, ImGui.GetCursorPosY()));
				if (ImGui.Button("No", new(68, 28)))
				{
					ImGui.CloseCurrentPopup();
				}

				ImGui.EndPopup();
			}
			ImGui.PopStyleColor();
			ImGui.PopStyleVar();

			ImGui.InputText("###CONFIGNAME", ref Settings.Menu.NewConfigName, 24);
			if (ImGui.Button("Open Folder", new(115, 28)))
			{
				string folderPath = Profiles.Location;
				if (Directory.Exists(folderPath))
				{
					ProcessStartInfo startInfo = new()
					{
						Arguments = folderPath,
						FileName = "explorer.exe"
					};
					Process.Start(startInfo);
				}
			}
			ImGui.SameLine();
			if (ImGui.Button("Create Profile", new(115, 28)))
			{
				Profiles.CurrentProfile = Settings.Menu.NewConfigName;
				Profiles.CreateProfile(Profiles.CurrentProfile);
				Profiles.LoadProfile(Profiles.CurrentProfile);
			}

			ImGui.Separator();
			ImGui.Text("Hotkey");
			Drawing.Tooltip("Set a key to load a profile without opening the menu.", Settings.Colors.AccentColor);
			Drawing.ConfigKey(new(240, 28));
		}
	}
}
