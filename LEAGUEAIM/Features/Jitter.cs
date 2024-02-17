using ImGuiNET;
using LEAGUEAIM.Utilities;
using LEAGUEAIM.Win32;

namespace LEAGUEAIM.Features
{
	internal class Jitter : Feature
	{
		public static bool RequireADS = true;
		public static float HorizontalStrength = 3.0f;
		public static float VerticalStrength = 3.0f;
		public static float TimeSleep = 1.0f;

		private static bool IsActive()
		{
			Keys _aimKey = (Keys)new KeysConverter().ConvertFromString("RButton");
			Keys _shootKey = (Keys)new KeysConverter().ConvertFromString("LButton");
			bool leftClicking = User32.GetAsyncKeyState(_shootKey) || (Settings.Controller.FlippedTriggers ? ControllerInput.IsKeyPressed(ControllerInput.ControllerKeys.RightShoulder) : ControllerInput.IsTriggerHeld(ControllerInput.ControllerTrigger.Right));
			bool rightClicking = User32.GetAsyncKeyState(_aimKey) || (Settings.Controller.FlippedTriggers ? ControllerInput.IsKeyPressed(ControllerInput.ControllerKeys.LeftShoulder) : ControllerInput.IsTriggerHeld(ControllerInput.ControllerTrigger.Left));
			bool isActive;

			if (RequireADS)
				isActive = leftClicking && rightClicking;
			else
				isActive = leftClicking;

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

				ImGui.SeparatorText("Settings");
				ImGui.SliderFloat("Y Strength", ref VerticalStrength, -100, 100, "%.2f", ImGuiSliderFlags.AlwaysClamp);
				ImGui.SliderFloat("X Strength", ref HorizontalStrength, -100, 100, "%.2f", ImGuiSliderFlags.AlwaysClamp);
				ImGui.SliderFloat("Sleep Time (ms)", ref TimeSleep, 1, 100, "%.2f", ImGuiSliderFlags.AlwaysClamp);
			}
		}

		public override void Run()
		{
			if (Enabled)
			{
				// dont run if we fail the cursor visibility check
				if (Cursor.Current.FailsCheck())
					return;

				// dont run if the menu is visible and require ads is off
				if (!RequireADS && Settings.Engine.IsVisible)
					return;

				if (IsActive())
				{
					int xVal = (int)HorizontalStrength;
					int yVal = (int)VerticalStrength;

					VirtualInput.Mouse.Move(0, yVal);
					Thread.Sleep((int)TimeSleep);
					VirtualInput.Mouse.Move(0, -yVal);
					Thread.Sleep((int)TimeSleep);

					if (!IsActive())
						return;

					VirtualInput.Mouse.Move(xVal, 0);
					Thread.Sleep((int)TimeSleep);
					VirtualInput.Mouse.Move(-xVal, 0);
					Thread.Sleep((int)TimeSleep);

					if (!IsActive())
						return;

					VirtualInput.Mouse.Move(0, -yVal);
					Thread.Sleep((int)TimeSleep);
					VirtualInput.Mouse.Move(0, yVal);
					Thread.Sleep((int)TimeSleep);

					if (!IsActive())
						return;

					VirtualInput.Mouse.Move(-xVal, 0);
					Thread.Sleep((int)TimeSleep);
					VirtualInput.Mouse.Move(xVal, 0);
					Thread.Sleep((int)TimeSleep);

					if (!IsActive())
						return;
				}
			}
		}

		public static Jitter Instance = new();
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
