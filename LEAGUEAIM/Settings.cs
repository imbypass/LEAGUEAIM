using SharpDX.XInput;
using System.Numerics;

namespace LEAGUEAIM
{
	internal class Settings
	{
		public static class Product
		{
			public static string Version = "7-17-20240316-official-clashgrotesk";
		}
		public static class API
		{
			public static string BaseUri = "http://auth.leagueaim.gg";
			public static string ClientId = "tqBLp7ee1D";
			public static string ClientSecret = "bPvRxhqYRiKATD2";
		}
		public static class Engine
		{
			public static bool IsVisible = true;
			public static bool ShowFooter = false;
			public static bool ShowWatermark = true;
			public static bool ControllerSupport = false;
			public static bool StreamProof = false;
			public static bool HideFromTaskbar = false;
			public static bool RainbowMode = false;
			public static int RainbowSpeed = 30;
			public static bool HasInterception = false;
			public static bool HasGhub = false;
			public static int InputMethod = 0;
			public static string AvatarPath = string.Empty;
			public static bool CursorCheck = false;
			public static int CursorCheckType = 0;
		}
		public static class Colors
		{
			public static Vector4 BgColor = new(0.09f, 0.09f, 0.09f, 0.95f);
			public static Vector4 TextColor = new(1.0f, 1.0f, 1.0f, 1.0f);
			public static Vector4 AccentColor = new (0.51f, 0.43f, 0.88f, 1.0f);
			public static Vector4 FrameColor = new(1f, 1f, 1f, 0.06f);
			public static Vector4 CrosshairColor = new(0.0f, 1.0f, 0.0f, 1.0f);
		}
		public static class Controller
		{
			public static float PollingRate = 9;
			public static int Index = 0;
			public static bool Enabled = false;
			public static bool FlippedTriggers = false;
			public static State State = new();
			public static SharpDX.XInput.Controller CController;
		}
		public static class Lua
		{
			public static bool Enabled = false;
			public static int CurrentScript = -1;
			public static string ScriptName = string.Empty;
			public static bool PrimaryMouseButtonEvents = true;
			public static bool SecondaryMouseButtonEvents = true;
			public static bool CoreFunctionality = true;
			public static string CurrentLog = string.Empty;
		}
		public static class Menu
		{
			public static int CurrentMenu = 0;
			public static int CurrentHotkey = 0;
			public static int CurrentKeybind = 0;
			public static int CurrentScript = 0;
			public static int CurrentConfig = -1;
			public static string NewConfigName = "New Profile";
			public static string NewStyleName = "New Style";
		}

		public static class ButtonSizes
		{
			public static int Full = 240; //256
			public static int Half = 115; //123
			public static int Third = 73; //79
		}
	}
}
