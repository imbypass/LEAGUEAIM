using ImGuiNET;
using LEAGUEAIM.Utilities;
using System.Numerics;
using LEAGUEAIM.Win32;

namespace LEAGUEAIM.Features
{
	internal class AutoNade : Feature
	{
		public static Keys KeyThrow = Keys.G;
		public static float TimeDelay = 3000.0f;

		private static bool _isCooking = false;

		public override void Render()
		{
			ImGui.Checkbox("Enabled", ref Enabled);

			if (Enabled)
			{
				ImGui.Text("Key");
				ImGui.SameLine();
				Drawing.Hotkey(ref KeyThrow, new Vector2(120, 28));
				ImGui.Spacing();
				ImGui.SliderFloat("Delay (ms)##AUTONADE", ref TimeDelay, 1, 7500, "%.0f", ImGuiSliderFlags.AlwaysClamp);
			}
		}

		public override void Run()
		{
			Keys _nadeKey = KeyThrow;
			bool _cookingNade = User32.GetAsyncKeyState(_nadeKey);
			if (Enabled)
			{
				if (_cookingNade && !_isCooking)
				{
					_isCooking = true;
					Thread.Sleep((int)TimeDelay);
					VirtualInput.Keyboard.Up(VirtualInput.VkToDik(_nadeKey));
					_isCooking = false;
				}
			}
		}

		public static AutoNade Instance = new();

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
