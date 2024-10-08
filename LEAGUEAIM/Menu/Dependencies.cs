﻿using Script_Engine.Utilities;

namespace LEAGUEAIM.Utilities
{
	internal class Dependencies
	{
		private static readonly Dictionary<string, string> Assets = new()
		{
			{
				Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LEAGUEAIM", "fonts", "FontAwesome.ttf"),
				$"{Settings.API.BaseUri}/fonts/FontAwesome.ttf"
			},
			{
				Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LEAGUEAIM", "fonts", "ClashDisplay-Bold.ttf"),
				$"{Settings.API.BaseUri}/fonts/ClashDisplay-Bold.ttf"
			},
			{
				Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LEAGUEAIM", "fonts", "ClashDisplay-Semibold.ttf"),
				$"{Settings.API.BaseUri}/fonts/ClashDisplay-Semibold.ttf"
			},
			{
				Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LEAGUEAIM", "fonts", "FiraCode.ttf"),
				$"{Settings.API.BaseUri}/fonts/FiraCode.ttf"
			},
		};
		public static void Run()
		{
			Logger.WriteLine("Checking dependencies..");

			using var p = new ProgressBar();

			string fontsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LEAGUEAIM", "fonts");
			if (!Directory.Exists(fontsFolder))
			{
				Directory.CreateDirectory(fontsFolder);
			}

            foreach (var item in Assets)
			{
				DownloadAsset(item.Key, item.Value);
				p.Report((float)Assets.Keys.ToList().IndexOf(item.Key) / Assets.Count);
			}
        }

		private static void DownloadAsset(string localPath, string remotePath)
		{
			if (File.Exists(localPath))
				return;

			using HttpClient hc = new(new HttpClientHandler() { Proxy = null, UseProxy = false,  });
			hc.DefaultRequestHeaders.UserAgent.ParseAdd($"LEAGUEAIM/{Settings.Product.Version}");
			using HttpResponseMessage response = hc.GetAsync(remotePath).Result;
			using HttpContent content = response.Content;
			using Stream stream = content.ReadAsStreamAsync().Result;
			using FileStream fs = new(localPath, FileMode.Create, FileAccess.Write);
			stream.CopyTo(fs);
			stream.Dispose();
			content.Dispose();
			response.Dispose();
			hc.Dispose();
		}
	}
}
