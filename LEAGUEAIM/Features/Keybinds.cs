﻿using ImGuiNET;
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
			Keys.End
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
			"Terminate"
		];

		public override void Render()
		{
			ImGui.Combo("##KEYBINDLIST", ref Settings.Menu.CurrentKeybind, MenuOptions, MenuOptions.Length, MenuOptions.Length);
			Drawing.Hotkey(ref MenuKeys[Settings.Menu.CurrentKeybind], new(226 * .65f, 28));
			ImGui.SameLine();
			if (Drawing.IconButton("Reset", IconFonts.FontAwesome6.ArrowRotateLeft, new(226 * .37f, 28), false, ImGui.GetStyle().FrameRounding, 0))
			{
				switch (Settings.Menu.CurrentKeybind)
				{
					case 0:
						MenuKeys[Settings.Menu.CurrentKeybind] = Keys.Insert;
						break;
					case 1:
						MenuKeys[Settings.Menu.CurrentKeybind] = Keys.End;
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
				if (User32.GetAsyncKeyState((Keys)(key.Key)))
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
				if (User32.GetAsyncKeyState(Keybinds.MenuKeys[0]))
				{
					Settings.Engine.IsVisible = !Settings.Engine.IsVisible;
				}
				if (User32.GetAsyncKeyState(Keybinds.MenuKeys[1]))
				{
					Interception.Unload();
					Environment.Exit(0);
				}
				Keybinds.Instance.Run();
				Thread.Sleep(150);
			}
		}
	}
}