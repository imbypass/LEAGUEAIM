using System.Diagnostics;
using System.Numerics;
using LEAGUEAIM.Features;
using LEAGUEAIM.Utilities;
using SharpDX.XInput;
using LEAGUEAIM.Win32;
using Script_Engine.Cloud;

namespace LEAGUEAIM
{
	internal class Engine {

		private static Vector4 rainbowColor;

		public static bool[] WaitingForKey = new bool[InputKeys.KeyCodes.Length];
		public static bool WaitingForHotkey = false;
		public static bool CapturedInput = true;

		public static Thread LuaThread;
		public static LuaLog Logs = new();
		public static Thread ControllerThread;
		public static void StartHotkeys()
		{
			new Thread(new ThreadStart(Keybinds.Loop)) { IsBackground = true, Priority = ThreadPriority.Lowest }.Start();
		}
		public static void StartThreads()
		{

			ControllerThread = new Thread(new ThreadStart(ControllerLoop)) { IsBackground = true, Priority = ThreadPriority.Highest };
			ControllerThread.Start();

			new Thread(new ThreadStart(ColorLoop)) { IsBackground = true, Priority = ThreadPriority.Lowest }.Start();
			new Thread(new ThreadStart(Recoil.Loop)) { IsBackground = true, Priority = ThreadPriority.Lowest }.Start();
			new Thread(new ThreadStart(Rapidfire.Loop)) { IsBackground = true, Priority = ThreadPriority.Lowest }.Start();
			new Thread(new ThreadStart(Jitter.Loop)) { IsBackground = true, Priority = ThreadPriority.Lowest }.Start();
			new Thread(new ThreadStart(QuickPeek.Loop)) { IsBackground = true, Priority = ThreadPriority.Lowest }.Start();
			new Thread(new ThreadStart(CloudMenu.UpdateLoop)) { IsBackground = true, Priority = ThreadPriority.Lowest }.Start();
		}
		public static void ColorLoop()
		{
			while(true)
			{
				Thread.Sleep(1);

				rainbowColor.X = (float)Math.Sin(Environment.TickCount / (float)(Settings.Engine.RainbowSpeed * 100)) * 0.5f + 0.5f;
				rainbowColor.Y = (float)Math.Sin(Environment.TickCount / (float)(Settings.Engine.RainbowSpeed * 100) + 2) * 0.5f + 0.5f;
				rainbowColor.Z = (float)Math.Sin(Environment.TickCount / (float)(Settings.Engine.RainbowSpeed * 100) + 4) * 0.5f + 0.5f;
				rainbowColor.W = 1.0f;

				if (Settings.Engine.RainbowMode)
				{
					Settings.Colors.AccentColor = rainbowColor;

					// lighter colors
					Settings.Colors.AccentColor.X = Settings.Colors.AccentColor.X * 0.5f + 0.5f;
					Settings.Colors.AccentColor.Y = Settings.Colors.AccentColor.Y * 0.5f + 0.5f;
					Settings.Colors.AccentColor.Z = Settings.Colors.AccentColor.Z * 0.5f + 0.5f;
				}

				_ = User32.SetWindowDisplayAffinity(Process.GetCurrentProcess().MainWindowHandle, Settings.Engine.StreamProof ? User32.WDA_EXCLUDEFROMCAPTURE : User32.WDA_NONE);
			}
		}
		public static void ControllerLoop()
		{
			bool running = true;
			try
			{
				UserIndex ControllerIndex = (UserIndex)(Settings.Controller.Index == 4 ? 255 : Settings.Controller.Index);
				Settings.Controller.CController = new Controller(ControllerIndex);
				while (running)
				{
					Thread.Sleep(Convert.ToInt32(Settings.Controller.PollingRate));
					if (Settings.Controller.Enabled)
					{
						Settings.Controller.CController.GetState(out Settings.Controller.State);
					}
				}
			} catch (ThreadInterruptedException err)
			{
				running = false;
				err.ToString();
			}
		}
	}
}
