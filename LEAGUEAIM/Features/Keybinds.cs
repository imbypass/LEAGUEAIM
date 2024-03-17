using ClickableTransparentOverlay;
using ClickableTransparentOverlay.Win32;
using ImGuiNET;
using LEAGUEAIM.Utilities;
using LEAGUEAIM.Win32;
using Script_Engine.Utilities;

namespace LEAGUEAIM.Features
{
	internal class Keybinds : Feature
	{
		public static Keys[] MenuKeys =
		{
			Keys.Insert,
			Keys.End,
			Keys.F1,
			Keys.F2
		};
		public static void UpdateHotkeyList()
		{
			KeyList = [];
			string[] profiles = Profiles.GetProfileList();
			foreach (string profile in profiles)
			{
				IniFile p = new(Path.Combine(Profiles.Location, profile + ".ini"));
				string hotkey = p.Read("Hotkey", "Settings");
				int convertedKey = InputKeys.KeyCodeFromName(hotkey);
				if (convertedKey != 0)
					KeyList.TryAdd(convertedKey, profile);
			}
		}

		private static Dictionary<int, string> KeyList;
		private static readonly string[] MenuOptions =
		[
			"Toggle Menu",
			"Terminate",
			"Toggle Recoil",
			"Toggle Rapidfire",
		];

		public override void Render()
		{
			ImGui.Combo("##KEYBINDLIST", ref Settings.Menu.CurrentKeybind, MenuOptions, MenuOptions.Length, MenuOptions.Length);
			Drawing.Hotkey(ref MenuKeys[Settings.Menu.CurrentKeybind], new((242 * .65f) - 1, 28));
			ImGui.SameLine();
			if (Drawing.IconButton("Reset", IconFonts.FontAwesome6.ArrowRotateLeft, new(242 * .37f, 28), false, ImGui.GetStyle().FrameRounding, 0))
			{
				switch (Settings.Menu.CurrentKeybind)
				{
					case 0:
						MenuKeys[Settings.Menu.CurrentKeybind] = Keys.Insert;
						break;
					case 1:
						MenuKeys[Settings.Menu.CurrentKeybind] = Keys.End;
						break;
					case 2:
						MenuKeys[Settings.Menu.CurrentKeybind] = Keys.F1;
						break;
					case 3:
						MenuKeys[Settings.Menu.CurrentKeybind] = Keys.F2;
						break;
				}
			}
		}
		public override void Run()
		{
			if (KeyList == null)
				UpdateHotkeyList();

			foreach (KeyValuePair<int, string> key in KeyList)
			{
				Keys kb = (Keys)key.Key;
				if (kb != Keys.None && Utils.IsKeyPressedAndNotTimeout((ClickableTransparentOverlay.Win32.VK)kb))
				{
					Settings.Menu.CurrentConfig = Profiles.GetProfileIndex(key.Value);
					Profiles.LoadProfile(key.Value);
					break;
				}
			}
		}
		public static Keybinds Instance = new();
		public static void Loop()
		{
			while (true)
			{
				if (MenuKeys[0] != Keys.None && Utils.IsKeyPressedAndNotTimeout((ClickableTransparentOverlay.Win32.VK)MenuKeys[0]))
				{
					Settings.Engine.IsVisible = !Settings.Engine.IsVisible;
				}
				if (MenuKeys[1] != Keys.None && Utils.IsKeyPressedAndNotTimeout((ClickableTransparentOverlay.Win32.VK)Keybinds.MenuKeys[1], 10))
				{
					Interception.Unload();
					Environment.Exit(0);
				}
				if (MenuKeys[2] != Keys.None && Utils.IsKeyPressedAndNotTimeout((ClickableTransparentOverlay.Win32.VK)Keybinds.MenuKeys[2]))
				{
					Recoil.Instance.Enabled = !Recoil.Instance.Enabled;
				}
				if (MenuKeys[3] != Keys.None && Utils.IsKeyPressedAndNotTimeout((ClickableTransparentOverlay.Win32.VK)Keybinds.MenuKeys[3]))
				{
					Rapidfire.Instance.Enabled = !Rapidfire.Instance.Enabled;
				}
				Keybinds.Instance.Run();
				Thread.Sleep(150);
			}
		}
	}
}
