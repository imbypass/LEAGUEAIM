using ImGuiNET;
using LEAGUEAIM.Utilities;
using LEAGUEAIM.Win32;
using System.Numerics;

namespace LEAGUEAIM.Features
{
	internal class Rapidfire : Feature
	{
		public static bool RequireADS = true;
		public static bool ActivateOnPrimary = false;
		public static bool FollowRecoil = false;
		public static float TimeFire = 1.0f;
		public static float VerticalStrength = 0.0f;
		public static Keys KeyActivation = Keys.XButton2;

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

			return ActivateOnPrimary ? isActive : User32.GetAsyncKeyState((Keys)KeyActivation);
		}
		public static bool KeyAlreadyUsed(Keys key)
		{
			return KeyActivation == key;
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

				Drawing.Hotkey(ref KeyActivation, new Vector2(120, 28));
				ImGui.SameLine();
				ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 6);
				ImGui.Text("Activation Key");

				ImGui.SeparatorText("Settings");

				ImGui.Checkbox("Shoot with Mouse 1", ref ActivateOnPrimary);
				Drawing.Tooltip($"Bind a second shoot key to [{"K"}] to use.", Settings.Colors.AccentColor);

				if (Recoil.Instance.Enabled)
				{
					ImGui.SameLine();
					ImGui.Checkbox("Use Recoil Settings", ref FollowRecoil);
					Drawing.Tooltip("Apply recoil control while using rapidfire.", Settings.Colors.AccentColor);
				}
				ImGui.SliderFloat("Fire Speed (ms)", ref TimeFire, 1, 100, "%.2f", ImGuiSliderFlags.AlwaysClamp);

				if (!FollowRecoil || !Recoil.Instance.Enabled)
				{
					ImGui.SliderFloat("Pull Down", ref VerticalStrength, 1, 100, "%.2f", ImGuiSliderFlags.AlwaysClamp);
				}
			}
		}
		public override void Run()
		{
			if (Enabled)
			{
				Keys _shoot = (Keys)new KeysConverter().ConvertFromString("LButton");
				Keys _secondKey = (Keys)new KeysConverter().ConvertFromString("K");
				VirtualInput.DirectInputKey _second = VirtualInput.VkToDik(_secondKey);

				// dont run if the menu is visible and require ads is off
				if (!RequireADS && Settings.Engine.IsVisible)
					return;

				// dont run if we fail the cursor visibility check
				if (Cursor.Current.FailsCheck())
					return;

				if (IsActive())
				{
					if (ActivateOnPrimary)
						VirtualInput.Keyboard.Down(_second);
					else
						VirtualInput.Mouse.Down(_shoot);

					Thread.Sleep((int)TimeFire);

					if (ActivateOnPrimary)
						VirtualInput.Keyboard.Up(_second);
					else
						VirtualInput.Mouse.Up(_shoot);

					Thread.Sleep(1);

					if (FollowRecoil)
					{
						Recoil.Instance.Run();
					}
					else
					{
						int pull = (int)VerticalStrength;
						VirtualInput.Mouse.Move(0, pull);
					}
				}
			}
		}

		public static Rapidfire Instance = new();
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
