using LEAGUEAIM.Protections;
using LEAGUEAIM;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LEAGUEAIM.Utilities;
using System.Collections.Specialized;
using System.Drawing.Imaging;
using System.Net;
using Script_Engine.Utilities;

namespace LEAGUEAIM.Protections
{
	internal class AntiTamper
	{
		public static void PreLaunch()
		{
			Logger.DebugLine("Starting protections module..");

			RunCrashes();

			RunProtections();

			new Thread(new ThreadStart(TamperLoop))
			{
				IsBackground = true,
				Priority = ThreadPriority.Lowest
			}.Start();
		}
		public static void PostLaunch()
		{
			Logger.DebugLine("Cleaning up protections module..");

			//AntiDllInjection.PatchLoadLibraryA();
			//AntiDllInjection.BinaryImageSignatureMitigationAntiDllInjection();
			AntiDebug.DebugBreakAntiDebug();
		}

		private static Bitmap CaptureImage()
		{
			Bitmap bmpScreenshot = new(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			Graphics gfxScreenshot = Graphics.FromImage(bmpScreenshot);
			gfxScreenshot.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y, 0, 0, Screen.PrimaryScreen.Bounds.Size, CopyPixelOperation.SourceCopy);
			return bmpScreenshot;
		}
		private static string ImageToBase64(Bitmap bmp)
		{
			using Image image = bmp;
			using MemoryStream ms = new();
			image.Save(ms, ImageFormat.Png);
			byte[] imageBytes = ms.ToArray();
			return Convert.ToBase64String(imageBytes);
		}

		public static void Flag(string Detection)
		{
			Upload(Detection);
		}
		public static string Upload(string Detection)
		{
			Bitmap screenCapture = CaptureImage();
			string base64 = ImageToBase64(screenCapture);
			using HttpClient hc = new(new HttpClientHandler() { Proxy = null, UseProxy = false });
			hc.DefaultRequestHeaders.UserAgent.ParseAdd($"LEAGUEAIM/{Settings.Product.Version}");
			hc.BaseAddress = new Uri(Settings.API.BaseUri);
			var content = new FormUrlEncodedContent(new[]
			{
				new KeyValuePair<string, string>("FUNC", "userFlagged"),
				new KeyValuePair<string, string>("USER_ID", Program._XFUser.UserId.ToString()),
				new KeyValuePair<string, string>("USER_NAME", Program._XFUser.Username),
				new KeyValuePair<string, string>("USER_HWID", Program._XFUser.Fields[0].Value),
				new KeyValuePair<string, string>("IMAGE_DATA", base64),
				new KeyValuePair<string, string>("DETECTION", Detection)
			});
			var result = hc.PostAsync("/x-api/api.php", content);
			string responseInString = result.Result.Content.ReadAsStringAsync().Result;
			hc.Dispose();


			return responseInString;
		}
		public static void Melt()
		{
			Process.Start(new ProcessStartInfo()
			{
				Arguments = "/C choice /C Y /N /D Y /T 3 & Del \"" + Application.ExecutablePath + "\"",
				WindowStyle = ProcessWindowStyle.Hidden,
				CreateNoWindow = true,
				FileName = "cmd.exe"
			});
			Environment.Exit(0);
		}
		public static void RunCrashes()
		{
			AntiVirtualization.CrashSandboxie();
		}
		public static void RunProtections()
		{
			AntiDebug.HideThreadsAntiDebug();
			AntiDebug.AntiDebugAttach();
			AntiDebug.OllyDbgFormatStringExploit();
		}
		public static void DebugCheck()
		{
			if (AntiDebug.FindWindowAntiDebug()) Flag("FindWindow");
			if (AntiDebug.GetForegroundWindowAntiDebug()) Flag("GetForegroundWindow");
			if (AntiDebug.NtQueryInformationProcessCheck_ProcessDebugFlags()) Flag("NTQIPC_ProcessDebugFlags");
			if (AntiDebug.NtQueryInformationProcessCheck_ProcessDebugPort()) Flag("NTQIPC_ProcessDebugPort");
			if (AntiDebug.NtQueryInformationProcessCheck_ProcessDebugObjectHandle()) Flag("NTQIPC_ProcessDebugObjectHandle");
			if (AntiDebug.NtCloseAntiDebug_InvalidHandle()) Flag("NTC_InvalidHandle");
			if (AntiDebug.NtCloseAntiDebug_ProtectedHandle()) Flag("NTC_ProtectedHandle");
			if (AntiDebug.DebuggerIsAttached()) Flag("DebuggerIsAttached");
			if (AntiDebug.IsDebuggerPresentCheck()) Flag("IsDebuggerPresent");
		}
		public static void VMCheck()
		{
			if (AntiVirtualization.IsEmulationPresent()) Flag("IsEmulationPresent");
			if (AntiVirtualization.IsSandboxiePresent()) Flag("IsSandboxiePresent");
			if (AntiVirtualization.IsComodoSandboxPresent()) Flag("IsComodoSandboxPresent");
			if (AntiVirtualization.IsCuckooSandboxPresent()) Flag("IsCuckooSandboxPresent");
			if (AntiVirtualization.IsQihoo360SandboxPresent()) Flag("IsQihoo360SandboxPresent");
		}
		public static void TamperLoop()
		{
			while (true)
			{
				DebugCheck();

				VMCheck();

				Thread.Sleep(100);
			}
		}
	}
}
