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
		public static readonly XenForoConfig _Config = new("http://auth.leagueaim.gg/forum/x-api/", Settings.API.ClientId, Settings.API.ClientSecret);
		public static XenForoApi _XF;
		public static User _XFUser;

		[STAThread]
		public static void Main(string[] args)
		{
			Logger.Initialize();

			AntiTamper.PreLaunch();

			Functions.CreateUrlScheme();

			Functions.CheckForImport(args);

			CheckElevated();

			MenuSettings.LoadMenuSettings();

			MenuSettings.LoadKeybinds();

			Recoil.CreatePatternsDirectory();

			// Functions.CheckForUpdates();

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

			Renderer = new(Scrambled(), false);
			Renderer.Start().Wait();
			Renderer.Size = ScreenSize;

			Engine.StartHotkeys();

			LARenderer.ApplyStyle();

			new Thread(new ThreadStart(LoginHelper.Loop))
			{
				IsBackground = true,
				Priority = ThreadPriority.Lowest
			}.Start();

			AntiTamper.PostLaunch();
		}
		public static string Scrambled()
		{
			return Functions.RandomString(18, true, false);
		}
		public static void CheckElevated()
		{
			Logger.WriteLine("Checking for admin privileges..");
			if (!Functions.IsAdministrator)
			{
				MessageBox.Show("LEAGUEAIM requires administrator privileges to function properly.", "LEAGUEAIM", MessageBoxButtons.OK, MessageBoxIcon.Error);

				Environment.Exit(0);
			}
		}
	}
}

