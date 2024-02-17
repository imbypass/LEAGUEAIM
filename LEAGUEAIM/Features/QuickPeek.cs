using ImGuiNET;
using LEAGUEAIM.Utilities;
using LEAGUEAIM.Win32;
using System.Numerics;

namespace LEAGUEAIM.Features
{
	internal class QuickPeek : Feature
	{
		public static int PeekType = 0;
		public static int DelayIn = 30;
		public static int DelayOut = 150;
		public static Keys KeyLeanLeft = Keys.Q;
		public static Keys KeyLeanRight = Keys.E;
		public static Keys KeyPeekLeft = Keys.XButton1;
		public static Keys KeyPeekRight = Keys.XButton2;

		private static bool leanedLeft = false;
		private static bool leanedRight = false;
		private static bool leaningLeft = false;
		private static bool leaningRight = false;

		private static readonly string[] PeekTypes = ["Beaulo", "Shaiko"];
		public override void Render()
		{
			ImGui.Checkbox("Enabled", ref Enabled);

			if (Enabled)
			{
				ImGui.SeparatorText("Settings");
				ImGui.Combo("Quick Peek Type", ref PeekType, PeekTypes, PeekTypes.Length);
				ImGui.SliderInt("Delay 1 (ms)", ref DelayIn, 0, 200);
				ImGui.SliderInt("Delay 2 (ms)", ref DelayOut, 0, 200);
				ImGui.Spacing();

				ImGui.Separator();
				ImGui.Spacing();

				ImGui.Text("Lean Left");
				ImGui.SameLine(150.0f);
				ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 6);
				Drawing.Hotkey(ref KeyLeanLeft, new Vector2(120, 28));
				ImGui.Spacing();

				ImGui.Text("Lean Right");
				ImGui.SameLine(150.0f);
				ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 6);
				Drawing.Hotkey(ref KeyLeanRight, new Vector2(120, 28));

				ImGui.Separator();
				ImGui.Spacing();

				ImGui.Text("Quick Peek (Left)");
				ImGui.SameLine(150.0f);
				ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 6);
				Drawing.Hotkey(ref KeyPeekLeft, new Vector2(120, 28));
				ImGui.Spacing();

				ImGui.Text("Quick Peek (Right)");
				ImGui.SameLine(150.0f);
				ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 6);
				Drawing.Hotkey(ref KeyPeekRight, new Vector2(120, 28));
			}
		}
		public override void Run()
		{
			if (Enabled)
			{
				// dont run if we fail the cursor visibility check
				if (Cursor.Current.FailsCheck())
					return;

				Keys _peekLeft = KeyPeekLeft;
				Keys _peekRight = KeyPeekRight;
				leaningLeft = User32.GetAsyncKeyState(_peekLeft);
				leaningRight = User32.GetAsyncKeyState(_peekRight);

				Keys _moveLeft = Keys.A;
				VirtualInput.DirectInputKey moveLeft = VirtualInput.VkToDik(_moveLeft);

				Keys _moveRight = Keys.D;
				VirtualInput.DirectInputKey moveRight = VirtualInput.VkToDik(_moveRight);

				Keys _leanLeft = KeyLeanLeft;
				VirtualInput.DirectInputKey leanLeft = VirtualInput.VkToDik(_leanLeft);

				Keys _leanRight = KeyLeanRight;
				VirtualInput.DirectInputKey leanRight = VirtualInput.VkToDik(_leanRight);

				switch(PeekType)
				{
					case 0:
						PerformPeekBeaulo();
						break;
					case 1:
						PerformPeekShaiko();
						break;
				}

				if (!leaningLeft && !leaningRight)
				{
					leanedLeft = false;
					leanedRight = false;
				}
			}
		}
		public static bool KeyAlreadyUsed(Keys key)
		{
			return KeyLeanLeft == key || KeyLeanRight == key || KeyPeekLeft == key || KeyPeekRight == key;
		}
		private static void PerformPeekBeaulo()
		{

			Keys _moveLeft = Keys.A;
			VirtualInput.DirectInputKey moveLeft = VirtualInput.VkToDik(_moveLeft);

			Keys _moveRight = Keys.D;
			VirtualInput.DirectInputKey moveRight = VirtualInput.VkToDik(_moveRight);

			Keys _leanLeft = KeyLeanLeft;
			VirtualInput.DirectInputKey leanLeft = VirtualInput.VkToDik(_leanLeft);

			Keys _leanRight = KeyLeanRight;
			VirtualInput.DirectInputKey leanRight = VirtualInput.VkToDik(_leanRight);

			if (leaningLeft && !leanedLeft)
			{
				VirtualInput.Keyboard.Down(moveLeft);
				Thread.Sleep(DelayIn);
				VirtualInput.Keyboard.Down(leanLeft);
				Thread.Sleep(1);
				VirtualInput.Keyboard.Up(leanLeft);
				Thread.Sleep(DelayOut);
				VirtualInput.Keyboard.Up(moveLeft);
				leanedLeft = true;
			}
			if (leanedLeft && !leaningLeft)
			{
				VirtualInput.Keyboard.Down(moveRight);
				Thread.Sleep((int)(DelayOut * .75f));
				VirtualInput.Keyboard.Down(leanLeft);
				Thread.Sleep(1);
				VirtualInput.Keyboard.Up(leanLeft);
				Thread.Sleep(DelayIn);
				VirtualInput.Keyboard.Up(moveRight);
			}

			if (leaningRight && !leanedRight)
			{
				VirtualInput.Keyboard.Down(moveRight);
				Thread.Sleep(DelayIn);
				VirtualInput.Keyboard.Down(leanRight);
				Thread.Sleep(1);
				VirtualInput.Keyboard.Up(leanRight);
				Thread.Sleep(DelayOut);
				VirtualInput.Keyboard.Up(moveRight);
				leanedRight = true;
			}
			if (leanedRight && !leaningRight)
			{
				VirtualInput.Keyboard.Down(moveLeft);
				Thread.Sleep(DelayIn);
				VirtualInput.Keyboard.Down(leanRight);
				Thread.Sleep(1);
				VirtualInput.Keyboard.Up(leanRight);
				Thread.Sleep(DelayOut);
				VirtualInput.Keyboard.Up(moveLeft);
			}
		}
		private static void PerformPeekShaiko()
		{

			Keys _moveLeft = Keys.A;
			VirtualInput.DirectInputKey moveLeft = VirtualInput.VkToDik(_moveLeft);

			Keys _moveRight = Keys.D;
			VirtualInput.DirectInputKey moveRight = VirtualInput.VkToDik(_moveRight);

			Keys _leanLeft = KeyLeanLeft;
			VirtualInput.DirectInputKey leanLeft = VirtualInput.VkToDik(_leanLeft);

			Keys _leanRight = KeyLeanRight;
			VirtualInput.DirectInputKey leanRight = VirtualInput.VkToDik(_leanRight);

			// pressed, not leaning left
			if (leaningLeft && !leanedLeft)
			{
				VirtualInput.Keyboard.Down(leanLeft);
				Thread.Sleep(1);
				VirtualInput.Keyboard.Up(leanLeft);
				leanedLeft = true;
			}

			// not pressed, leaning left
			if (leanedLeft && !leaningLeft)
			{
				VirtualInput.Keyboard.Down(moveLeft);
				Thread.Sleep(DelayIn);
				VirtualInput.Keyboard.Up(moveLeft);
				VirtualInput.Keyboard.Down(leanRight);
				VirtualInput.Keyboard.Down(moveRight);
				Thread.Sleep((int)(DelayOut));
				VirtualInput.Keyboard.Up(leanRight);
				VirtualInput.Keyboard.Up(moveRight);
				VirtualInput.Keyboard.Press(moveLeft);
			}

			// pressed, not leaning right
			if (leaningRight && !leanedRight)
			{
				VirtualInput.Keyboard.Down(leanRight);
				Thread.Sleep(1);
				VirtualInput.Keyboard.Up(leanRight);
				leanedRight = true;
			}

			// not pressed, leaning right
			if (leanedRight && !leaningRight)
			{
				VirtualInput.Keyboard.Down(moveRight);
				Thread.Sleep(DelayIn);
				VirtualInput.Keyboard.Up(moveRight);
				VirtualInput.Keyboard.Down(leanLeft);
				VirtualInput.Keyboard.Down(moveLeft);
				Thread.Sleep((int)(DelayOut));
				VirtualInput.Keyboard.Up(leanLeft);
				VirtualInput.Keyboard.Up(moveLeft);
				VirtualInput.Keyboard.Press(moveRight);
			}
		}

		public static QuickPeek Instance = new();
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
