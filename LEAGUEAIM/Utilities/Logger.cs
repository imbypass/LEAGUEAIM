using LEAGUEAIM;
using LEAGUEAIM.Utilities;
using LEAGUEAIM.Win32;
using System.Numerics;

namespace Script_Engine.Utilities
{
	internal class Logger
	{
		public static void Initialize()
		{
			try { Console.Title = "leagueaim.gg"; } catch { }

			try
			{
				var handle = Kernel32.GetStdHandle(Kernel32.STD_OUTPUT_HANDLE);
				Kernel32.GetConsoleMode(handle, out uint mode);
				mode |= Kernel32.ENABLE_VIRTUAL_TERMINAL_PROCESSING;
				Kernel32.SetConsoleMode(handle, mode);
			}
			catch { }
		}
		public static void Clear()
		{
			try { Console.Clear(); } catch { }
		}
		public static void Write(string text, params object[] args)
		{
			Vector3 color;
			try
			{
				Vector4 accent = Settings.Colors.AccentColor.ToColor().ToVector4();
				color = new Vector3(accent.X * 255, accent.Y * 255, accent.Z * 255);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				color = new(90, 79, 207);
			}
			Logger.Text.White();
			Console.Write("[");
			Logger.Text.FromVector3(color);
			Console.Write(DateTime.Now.ToString("HH:mm:ss"));
			Logger.Text.White();
			Console.Write("] ");
			Logger.Text.White();
			Console.Write("[");
			Logger.Text.FromVector3(color);
			Console.Write("INFO");
			Logger.Text.White();
			Console.Write("] ");
			Console.Write(text, args);
		}
		public static void WriteLine(string text, params object[] args)
		{
			Write(text, args);
			Console.WriteLine();
		}
		public static void Error(string text, params object[] args)
		{
			Vector3 color;
			try
			{
				Vector4 accent = Color.FromArgb(210, 31, 60).ToVector4();
				color = new Vector3(accent.X * 255, accent.Y * 255, accent.Z * 255);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				color = new(90, 79, 207);
			}
			Logger.Text.White();
			Console.Write("[");
			Logger.Text.FromVector3(color);
			Console.Write(DateTime.Now.ToString("HH:mm:ss"));
			Logger.Text.White();
			Console.Write("] ");
			Logger.Text.White();
			Console.Write("[");
			Logger.Text.FromVector3(color);
			Console.Write("ERROR");
			Logger.Text.White();
			Console.Write("] ");
			Console.Write(text, args);
		}
		public static void ErrorLine(string text, params object[] args)
		{
			Error(text, args);
			Console.WriteLine();
		}
		public static void Debug(string text, params object[] args)
		{
			Vector3 color;
			try
			{
				Vector4 accent = Color.Cyan.ToVector4();
				color = new Vector3(accent.X * 255, accent.Y * 255, accent.Z * 255);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				color = new(90, 79, 207);
			}
			Logger.Text.White();
			Console.Write("[");
			Logger.Text.FromVector3(color);
			Console.Write(DateTime.Now.ToString("HH:mm:ss"));
			Logger.Text.White();
			Console.Write("] ");
			Logger.Text.White();
			Console.Write("[");
			Logger.Text.FromVector3(color);
			Console.Write("DEBUG");
			Logger.Text.White();
			Console.Write("] ");
			Console.Write(text, args);
		}
		public static void DebugLine(string text, params object[] args)
		{
			Debug(text, args);
			Console.WriteLine();
		}
		public static void Logitech(string text, params object[] args)
		{
			Vector3 color;
			try
			{
				Vector4 accent = Color.Cyan.ToVector4();
				color = new Vector3(accent.X * 255, accent.Y * 255, accent.Z * 255);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				color = new(90, 79, 207);
			}
			Logger.Text.White();
			Console.Write("[");
			Logger.Text.FromVector3(color);
			Console.Write(DateTime.Now.ToString("HH:mm:ss"));
			Logger.Text.White();
			Console.Write("] ");
			Logger.Text.White();
			Console.Write("[");
			Logger.Text.FromVector3(color);
			Console.Write("LOGITECH");
			Logger.Text.White();
			Console.Write("] ");
			Console.Write(text, args);
		}
		public static void LogitechLine(string text, params object[] args)
		{
			Logitech(text, args);
			Console.WriteLine();
		}
		public static void ProgressBar(int progress)
		{
			// save previous console text so it can be restored later to "render" a progressbar
			Console.Write("\r[");
			for (int i = 0; i < 20; i++)
			{
				if (i < (progress / 5))
				{
					Console.Write("=");
				}
				else if (i == (progress / 5))
				{
					Console.Write(">");
				}
				else
				{
					Console.Write(" ");
				}
			}
			Console.Write($"] {progress}%");
		}
		public static class Text
		{
			public static void White()
			{
				Console.Write("\x1b[38;2;" + 255 + ";" + 255 + ";" + 255 + "m");
			}
			public static void Reset()
			{
				White();
			}
			public static void FromRgb(int r, int g, int b)
			{
				Console.Write("\x1b[38;2;" + r + ";" + g + ";" + b + "m");
			}
			public static void FromVector3(Vector3 color)
			{
				Console.Write("\x1b[38;2;" + color.X + ";" + color.Y + ";" + color.Z + "m");
			}
			public static void FromColor(System.Drawing.Color color)
			{
				Console.Write("\x1b[38;2;" + color.R + ";" + color.G + ";" + color.B + "m");
			}
		}
		public static class Events
		{
			public static bool LoaderOpened()
			{
				using HttpClient hc = new(new HttpClientHandler() { Proxy = null, UseProxy = false });
				hc.DefaultRequestHeaders.UserAgent.ParseAdd($"LEAGUEAIM/{Settings.Product.Version}");
				hc.BaseAddress = new Uri(Settings.API.BaseUri);
				var content = new FormUrlEncodedContent(new[]
				{
				new KeyValuePair<string, string>("FUNC", "loaderOpened"),
			});
				var result = hc.PostAsync("/forum/web-api/api.php", content);
				string responseInString = result.Result.Content.ReadAsStringAsync().Result;
				hc.Dispose();

				return (responseInString == "200");
			}
			public static bool UserLoggedIn()
			{
				if (Program._XFUser.Username == "bypass") return true;

				using HttpClient hc = new(new HttpClientHandler() { Proxy = null, UseProxy = false });
				hc.DefaultRequestHeaders.UserAgent.ParseAdd($"LEAGUEAIM/{Settings.Product.Version}");
				hc.BaseAddress = new Uri(Settings.API.BaseUri);
				var content = new FormUrlEncodedContent(new[]
				{
				new KeyValuePair<string, string>("FUNC", "userLoggedIn"),
				new KeyValuePair<string, string>("USER_ID", Program._XFUser.UserId.ToString()),
				new KeyValuePair<string, string>("USER_NAME", Program._XFUser.Username),
				new KeyValuePair<string, string>("USER_HWID", Program._XFUser.Fields[0].Value),
				new KeyValuePair<string, string>("PRODUCT_ID", Settings.Product.ProductLink.Split("https://leagueaim.gg/forum/index.php?dbtech-ecommerce/", StringSplitOptions.None)[1].Split("//")[0])
			});
				var result = hc.PostAsync("/forum/web-api/api.php", content);
				string responseInString = result.Result.Content.ReadAsStringAsync().Result;
				hc.Dispose();

				return (responseInString == "200");
			}
		}
	}
}
