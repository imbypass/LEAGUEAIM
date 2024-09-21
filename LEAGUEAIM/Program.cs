using LEAGUEAIM.Features;
using LEAGUEAIM.Protections;
using LEAGUEAIM.Utilities;
using Script_Engine.Utilities;
using XenForo.NET;
using XenForo.NET.Models;

namespace LEAGUEAIM
{
    internal static class Program
	{
		public static LARenderer Renderer;
		public static DateTime StartTime;
		public static Size ScreenSize;
		public static readonly XenForoConfig _Config = new($"{Settings.API.BaseUri}/x-api/", Settings.API.ClientId, Settings.API.ClientSecret);
		public static XenForoApi _XF;
		public static User _XFUser;

		[STAThread]
		public static void Main(string[] args)
		{
			Logger.Initialize();

			Functions.CheckElevated();

			MenuSettings.LoadMenuSettings();

			MenuSettings.LoadKeybinds();

			Recoil.CreatePatternsDirectory();

			_XF = new(_Config);

			try
			{
				ScreenSize = new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
			}
			catch
			{
				_ = MessageBox.Show("Failed to get screen size, defaulting to 1920x1080.", "LEAGUEAIM", MessageBoxButtons.OK, MessageBoxIcon.Error);
				ScreenSize = new Size(1920, 1080);
			}

			StartTime = DateTime.Now;

			Dependencies.Run();

			Settings.Engine.HasInterception = Interception.Run();

			Settings.Engine.HasGhub = Logitech.Driver.Open();

			string RandomizedTitle = Functions.RandomString(18, true, false);
			Renderer = new(RandomizedTitle, false);

			Renderer.Start().Wait();
			Renderer.Size = ScreenSize;

			Engine.StartHotkeys();
			Engine.StartThreads();

			LARenderer.ApplyStyle();
		}
	}
}

