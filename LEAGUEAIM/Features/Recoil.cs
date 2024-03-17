using ImGuiNET;
using LEAGUEAIM.Utilities;
using LEAGUEAIM.Win32;
using Script_Engine.Cloud;
using Script_Engine.Utilities;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using static LEAGUEAIM.Features.Recoil;

namespace LEAGUEAIM.Features
{
	internal class Recoil : Feature
	{
		public static int RecoilType = 0;
		public static bool RequireADS = true;

		public static class Pattern
		{
			public static string Base = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LEAGUEAIM");
			public static string Location = Path.Combine(Base, "patterns");
			public static float XMultiplier = 1.0f;
			public static float YMultiplier = 1.0f;
			public static string PatternName = string.Empty;
			public static int CurrentPattern = 0;
			public static float[] Data = [];
		}
		public static class Linear
		{
			public static float HorizontalStrength = 0.0f;
			public static float VerticalStrength = 2.0f;
			public static float TimeSleep = 1.0f;
		}
		public static void CreatePatternsDirectory()
		{
			string Base = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LEAGUEAIM");
			string Location = Path.Combine(Base, "patterns");

			if (!Directory.Exists(Location))
				Directory.CreateDirectory(Location);
		}
		public static string[] GetPatterns()
		{
			string Base = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LEAGUEAIM");
			string Location = Path.Combine(Base, "patterns");

			CreatePatternsDirectory();

			List<string> files = [];

			string[] Files = Directory.GetFiles(Location, "*.txt");
			foreach (string file in Files)
			{
				try
				{
					string fileContents = File.ReadAllText(file);

					if (IsPatternValid(file))
						files.Add(Path.GetFileNameWithoutExtension(file));

				} catch (Exception ex)
				{
					Logger.ErrorLine(ex.Message);
				}
			}
			if (files.Count == 0)
			{
				files.Add("No Patterns Found");
			}

			return [.. files];
		}
		private static float[] GetPattern(string name)
		{
			string Base = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LEAGUEAIM");
			string Location = Path.Combine(Base, "patterns");

			string[] Files = Directory.GetFiles(Location, "*.txt");

			foreach (string file in Files)
			{
				if (Path.GetFileNameWithoutExtension(file) == name)
				{
					string[] PatternLines = File.ReadAllLines(file);
					List<float> Data = [];
					foreach (string line in PatternLines)
					{
						// split each line by comma
						string[] split = line.Split(',');
						// convert each split to a float
						foreach (string s in split)
						{
							if (float.TryParse(s, out float result))
								Data.Add(result);
						}
					}
					return [.. Data];
				}
			}
			return [];
		}

		public static string ConvertPatternToBase64(string patternName)
		{
			string file = Path.Combine(Pattern.Location, $"{patternName}.txt");
			string fileContents = File.ReadAllText(file);
			byte[] bytes = Encoding.UTF8.GetBytes(fileContents);
			string encodedPattern = Convert.ToBase64String(bytes);

			string toEncode = $"{patternName}|{encodedPattern}";
			bytes = Encoding.UTF8.GetBytes(toEncode);
			return Convert.ToBase64String(bytes);
		}
		public static object[] ConvertBase64ToPattern(string base64)
		{
			byte[] data = Convert.FromBase64String(base64);
			string decodedString = Encoding.UTF8.GetString(data);
			string[] split = decodedString.Split('|');
			string patternName = split[0];
			string patternData = split[1];
			return [patternName, patternData];
		}

		public static int GetPatternIndex(string name)
		{
			string[] patternList = GetPatterns();
			for (int i = 0; i < patternList.Length; i++)
			{
				if (patternList[i] == name)
					return i;
			}
			return -1;
		}
		private static bool IsPatternValid(string patternFile)
		{
			if (!File.Exists(patternFile))
				return false;

			string[] lines = File.ReadAllLines(patternFile);

			if (lines.Length == 0)
				return false;

			foreach (string line in lines)
			{
				string[] split = line.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

				if (split.Length != 3)
					return false;

				foreach (string s in split)
					if (!float.TryParse(s, out _))
						return false;
			}

			return true;
		}

		private static readonly string[] RecoilTypes =
		[
			"Linear",
			"Pattern"
		];
		private static bool IsActive()
		{
			Keys _aimKey = (Keys)new KeysConverter().ConvertFromString("RButton");
			Keys _shootKey = (Keys)new KeysConverter().ConvertFromString("LButton");
			bool leftClicking = User32.GetAsyncKeyState(_shootKey) || (Settings.Controller.FlippedTriggers ? ControllerInput.IsKeyPressed(ControllerInput.ControllerKeys.RightShoulder) : ControllerInput.IsTriggerHeld(ControllerInput.ControllerTrigger.Right));
			bool rightClicking = User32.GetAsyncKeyState(_aimKey) || (Settings.Controller.FlippedTriggers ? ControllerInput.IsKeyPressed(ControllerInput.ControllerKeys.LeftShoulder) : ControllerInput.IsTriggerHeld(ControllerInput.ControllerTrigger.Left));
			bool isHoldingRapidFire = User32.GetAsyncKeyState(Rapidfire.KeyActivation) && !Rapidfire.FollowRecoil && Rapidfire.Instance.Enabled;
			bool isActive;

			if (RequireADS)
				isActive = (leftClicking && rightClicking) && !isHoldingRapidFire;
			else
				isActive = leftClicking && !isHoldingRapidFire;

			return isActive;
		}

		public override void Render()
		{
			ImGui.Checkbox("Enabled", ref Enabled);

			if (Enabled)
			{
				ImGui.SameLine();
				ImGui.Checkbox("Require ADS", ref RequireADS);
				if (!RequireADS)
					Drawing.Tooltip("Functionality is suspended while menu is open.", Settings.Colors.AccentColor);

				ImGui.Combo("Recoil Type", ref RecoilType, RecoilTypes, RecoilTypes.Length);

				ImGui.SeparatorText("Settings");

				switch(RecoilType)
				{
					case 0:
						RenderLinear();
						break;
					case 1:
						RenderPattern();
						break;
				}
			}
		}
		private static void RenderPattern()
		{
			string[] patternList = GetPatterns();
			if (Pattern.CurrentPattern < 0 || Pattern.CurrentPattern >= patternList.Length)
				Pattern.CurrentPattern = 0;

			string currentPattern = patternList[Pattern.CurrentPattern];
			ImGui.Text($"Current Pattern:");
			if (Pattern.CurrentPattern >= 0)
			{
				ImGui.SameLine();
				ImGui.TextColored(Settings.Colors.AccentColor, $"{currentPattern}");
			}
			ImGui.Combo("Pattern", ref Pattern.CurrentPattern, patternList, patternList.Length, 6);
			ImGui.SliderFloat("Y Multiplier", ref Pattern.YMultiplier, 0.1f, 5, "x%.2f", ImGuiSliderFlags.AlwaysClamp);
			ImGui.SliderFloat("X Multiplier", ref Pattern.XMultiplier, 0.1f, 5, "x%.2f", ImGuiSliderFlags.AlwaysClamp);
			if (Drawing.IconButton("Open", IconFonts.FontAwesome6.FolderOpen, new(Settings.ButtonSizes.Half, 28), true, ImGui.GetStyle().FrameRounding, 0))
			{
				string folderPath = Pattern.Location;
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
			if (Drawing.IconButton("Delete", IconFonts.FontAwesome6.TrashCan, new(Settings.ButtonSizes.Half, 28), true, ImGui.GetStyle().FrameRounding, 0))
			{
				ImGui.OpenPopup("Delete Pattern");
			}
			if (Drawing.IconButton("Send to Cloud", IconFonts.FontAwesome6.Upload, new(Settings.ButtonSizes.Full, 28), true, ImGui.GetStyle().FrameRounding, 0))
			{
				if (GetPatterns()[0] != "No Patterns Found")
					ImGui.OpenPopup("Upload Pattern");
			}

			Vector2 cMenuPos;
			Vector2 cMenuSize;

			cMenuPos = ImGui.GetWindowPos();
			cMenuSize = ImGui.GetWindowSize();
			bool delete_pattern = true;
			ImGui.SetNextWindowPos(new(cMenuPos.X + (cMenuSize.X / 2) - 175, cMenuPos.Y + (cMenuSize.Y / 2) - 55));
			ImGui.SetNextWindowSize(new(350, 110));
			ImGui.PushStyleColor(ImGuiCol.Border, Settings.Colors.AccentColor);
			ImGui.PushStyleColor(ImGuiCol.PopupBg, Settings.Colors.BgColor);
			ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1.5f);
			if (ImGui.BeginPopupModal("Delete Pattern", ref delete_pattern, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoTitleBar))
			{
				string patternName = patternList[Pattern.CurrentPattern];
				ImGui.Spacing();
				Drawing.TextCentered($"Are you sure you want to delete pattern:  {patternName.TruncateWord(12)}?");
				ImGui.Spacing();
				ImGui.SetCursorPos(new Vector2((ImGui.GetWindowSize().X - 68) * 0.333f, ImGui.GetCursorPosY()));
				if (ImGui.Button("Yes", new(68, 28)))
				{
					string file = Path.Combine(Pattern.Location, $"{patternName}.txt");

					if (File.Exists(file))
						File.Delete(file);

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
			ImGui.PopStyleColor();
			ImGui.PopStyleVar();

			cMenuPos = ImGui.GetWindowPos();
			cMenuSize = ImGui.GetWindowSize();
			bool upload_pattern = true;
			ImGui.SetNextWindowPos(new(cMenuPos.X + (cMenuSize.X / 2) - 175, cMenuPos.Y + (cMenuSize.Y / 2) - 55));
			ImGui.SetNextWindowSize(new(350, 110));
			ImGui.PushStyleColor(ImGuiCol.Border, Settings.Colors.AccentColor);
			ImGui.PushStyleColor(ImGuiCol.PopupBg, Settings.Colors.BgColor);
			ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1.5f);
			if (ImGui.BeginPopupModal("Upload Pattern", ref upload_pattern, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoTitleBar))
			{
				string patternName = patternList[Pattern.CurrentPattern];
				ImGui.Spacing();
				Drawing.TextCentered($"Do you want to upload \"{patternName}\" to the cloud?");
				ImGui.Spacing();
				ImGui.SetCursorPos(new Vector2((ImGui.GetWindowSize().X - 68) * 0.333f, ImGui.GetCursorPosY()));
				if (ImGui.Button("Yes", new(68, 28)))
				{
					CloudMethods.UploadFile("patterns", patternName);
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
			ImGui.PopStyleColor();
			ImGui.PopStyleVar();
		}
		private static void RenderLinear()
		{
			ImGui.SliderFloat("Y Strength", ref Linear.VerticalStrength, -100, 100, "%.2f", ImGuiSliderFlags.AlwaysClamp);
			ImGui.SliderFloat("X Strength", ref Linear.HorizontalStrength, -100, 100, "%.2f", ImGuiSliderFlags.AlwaysClamp);
			ImGui.SliderFloat("Sleep Time (ms)", ref Linear.TimeSleep, 1, 100, "%.2f", ImGuiSliderFlags.AlwaysClamp);
		}


		public override void Run()
		{
			if (Enabled)
			{
				// dont run if the menu is visible and require ads is off
				if (!RequireADS && Settings.Engine.IsVisible)
					return;

				// dont run if we fail the cursor visibility check
				if (Cursor.Current.FailsCheck())
					return;

				if (IsActive())
				{
					switch (RecoilType)
					{
						case 0: // Linear
							RunLinear();
							break;
						case 1: // Pattern
							RunPattern();
							break;
					}
				}
			}
		}
		private static void RunLinear()
		{
			int xVal = (int)Linear.HorizontalStrength;
			int yVal = (int)Linear.VerticalStrength;

			VirtualInput.Mouse.Move(xVal, yVal);
			Thread.Sleep((int)Linear.TimeSleep - 1);
		}
		private static void RunPattern()
		{
			string[] patternList = GetPatterns();

			if (Recoil.Pattern.CurrentPattern < 0 || Recoil.Pattern.CurrentPattern >= patternList.Length)
				return;

			string currentPattern = patternList[Recoil.Pattern.CurrentPattern];
			float[] Pattern = GetPattern(currentPattern);

			for (int i = 0; i < Pattern.Length; i += 3)
			{
				if (!IsActive() || (i + 2) >= Pattern.Length)
					break;

				float xVal = ((Pattern[i]));
				float yVal = ((Pattern[i + 1]));

				float sleep = Pattern[i + 2];

				xVal *= Recoil.Pattern.XMultiplier;
				yVal *= Recoil.Pattern.YMultiplier;

				int iValX = (int)Math.Round(xVal, 0);
				int iValY = (int)Math.Round(yVal, 0);
				int iValS = (int)Math.Round(sleep, 0);

				VirtualInput.Mouse.Move(iValX, iValY);
				Thread.Sleep(Convert.ToInt32(iValS));
			}
			VirtualInput.Mouse.Up(Keys.LButton);
		}


		public static Recoil Instance = new();
		public static void Loop()
		{
			while (true)
			{
				Thread.Sleep(1);
				Instance.Run();
			}
		}
	}
}
