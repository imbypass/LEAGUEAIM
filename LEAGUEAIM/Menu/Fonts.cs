using ImGuiNET;

namespace LEAGUEAIM.Utilities
{
	public static class Fonts
	{
		public static ImFontPtr Icons;
		public static ImFontPtr IconsSm;
		public static ImFontPtr IconsLg;
		public static ImFontPtr Header;
		public static ImFontPtr Menu;
		public static ImFontPtr MenuSm;
		public static ImFontPtr MenuLg;
		public static bool Replaced = false;

		public unsafe static bool Load(LARenderer renderer)
		{
			if (!Fonts.Replaced)
			{
				bool res = renderer.ReplaceFont(config =>
				{
					string menuFontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LEAGUEAIM", "fonts", "Rubik.ttf");
					string headerFontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LEAGUEAIM", "fonts", "SharpGrotesk.ttf");
					string iconFontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LEAGUEAIM", "fonts", "FontAwesome.ttf");
					ushort[] customRange = [IconFonts.FontAwesome6.IconMin, IconFonts.FontAwesome6.IconMax, 0];
					var io = ImGui.GetIO();


					if (File.Exists(menuFontPath))
						Menu = io.Fonts.AddFontFromFileTTF(menuFontPath, 15, config, io.Fonts.GetGlyphRangesDefault());

					if (File.Exists(menuFontPath))
						MenuSm = io.Fonts.AddFontFromFileTTF(menuFontPath, 12, config, io.Fonts.GetGlyphRangesDefault());

					if (File.Exists(menuFontPath))
						MenuLg = io.Fonts.AddFontFromFileTTF(menuFontPath, 16, config, io.Fonts.GetGlyphRangesDefault());

					fixed (ushort* p = &customRange[0])
						if (File.Exists(iconFontPath))
						{
							Icons = io.Fonts.AddFontFromFileTTF(iconFontPath, 16, config, new IntPtr(p));
							IconsSm = io.Fonts.AddFontFromFileTTF(iconFontPath, 12, config, new IntPtr(p));
							IconsLg = io.Fonts.AddFontFromFileTTF(iconFontPath, 20, config, new IntPtr(p));
						}

					if (File.Exists(headerFontPath))
						Header = io.Fonts.AddFontFromFileTTF(headerFontPath, 30, config, io.Fonts.GetGlyphRangesDefault());
				});

				Fonts.Replaced = true;

				return res;
			}

			return false;
		}
	}
}
