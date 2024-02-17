using InputInterceptorNS;
using Script_Engine.Utilities;
using SharpDX.XInput;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace LEAGUEAIM.Utilities
{
	internal static class VirtualInput
	{
		public enum DirectInputKey : byte
		{
			ESCAPE = 0x01,
			ONE = 0x02,
			TWO = 0x03,
			THREE = 0x04,
			FOUR = 0x05,
			FIVE = 0x06,
			SIX = 0x07,
			SEVEN = 0x08,
			EIGHT = 0x09,
			NINE = 0x0A,
			ZERO = 0x0B,
			MINUS = 0x0C,
			EQUALS = 0x0D,
			BACK = 0x0E,
			TAB = 0x0F,
			Q = 0x10,
			W = 0x11,
			E = 0x12,
			R = 0x13,
			T = 0x14,
			Y = 0x15,
			U = 0x16,
			I = 0x17,
			O = 0x18,
			P = 0x19,
			LBRACKET = 0x1A,
			RBRACKET = 0x1B,
			RETURN = 0x1C,
			LCONTROL = 0x1D,
			A = 0x1E,
			S = 0x1F,
			D = 0x20,
			F = 0x21,
			G = 0x22,
			H = 0x23,
			J = 0x24,
			K = 0x25,
			L = 0x26,
			SEMICOLON = 0x27,
			APOSTROPHE = 0x28,
			GRAVE = 0x29,
			LSHIFT = 0x2A,
			BACKSLASH = 0x2B,
			Z = 0x2C,
			X = 0x2D,
			C = 0x2E,
			V = 0x2F,
			B = 0x30,
			N = 0x31,
			M = 0x32,
			COMMA = 0x33,
			PERIOD = 0x34,
			SLASH = 0x35,
			RSHIFT = 0x36,
			MULTIPLY = 0x37,
			LMENU = 0x38,
			SPACE = 0x39,
			CAPITAL = 0x3A,
			F1 = 0x3B,
			F2 = 0x3C,
			F3 = 0x3D,
			F4 = 0x3E,
			F5 = 0x3F,
			F6 = 0x40,
			F7 = 0x41,
			F8 = 0x42,
			F9 = 0x43,
			F10 = 0x44,
			NUMLOCK = 0x45,
			SCROLL = 0x46,
			NUMPAD7 = 0x47,
			NUMPAD8 = 0x48,
			NUMPAD9 = 0x49,
			SUBTRACT = 0x4A,
			NUMPAD4 = 0x4B,
			NUMPAD5 = 0x4C,
			NUMPAD6 = 0x4D,
			ADD = 0x4E,
			NUMPAD1 = 0x4F,
			NUMPAD2 = 0x50,
			NUMPAD3 = 0x51,
			NUMPAD0 = 0x52,
			DECIMAL = 0x53,
			F11 = 0x57,
			F12 = 0x58,
			F13 = 0x64,
			F14 = 0x65,
			F15 = 0x66,
			NUMPADEQUALS = 0x8D,
			AT = 0x91,
			COLON = 0x92,
			UNDERLINE = 0x93,
			NUMPADENTER = 0x9C,
			RCONTROL = 0x9D,
			NUMPADCOMMA = 0xB3,
			DIVIDE = 0xB5,
			SYSRQ = 0xB7,
			RMENU = 0xB8,
			PAUSE = 0xC5,
			HOME = 0xC7,
			UP = 0xC8,
			PRIOR = 0xC9,
			LEFT = 0xCB,
			RIGHT = 0xCD,
			END = 0xCF,
			DOWN = 0xD0,
			NEXT = 0xD1,
			INSERT = 0xD2,
			DELETE = 0xD3,
			LWIN = 0xDB,
			RWIN = 0xDC,
			APPS = 0xDD,
			POWER = 0xDE,
			SLEEP = 0xDF,
		}
		public enum MouseKey
		{
			LButton = 0,
			MButton = 1,
			RButton = 2,
			XButton1 = 3,
			XButton2 = 4,
		}
		public static bool IsKeyMouseButton(Keys key)
		{
			return (key == Keys.LButton || key == Keys.RButton || key == Keys.MButton || key == Keys.XButton1 || key == Keys.XButton2);
		}
		[StructLayout(LayoutKind.Sequential)]
		private struct KeyboardInput
		{
			public ushort wVk;
			public ushort wScan;
			public uint dwFlags;
			public uint time;
			public IntPtr dwExtraInfo;
		}
		[StructLayout(LayoutKind.Sequential)]
		private struct MouseInput
		{
			public int dx;
			public int dy;
			public uint mouseData;
			public uint dwFlags;
			public uint time;
			public IntPtr dwExtraInfo;
		}
		[StructLayout(LayoutKind.Sequential)]
		private struct HardwareInput
		{
			public uint uMsg;
			public ushort wParamL;
			public ushort wParamH;
		}
		[StructLayout(LayoutKind.Explicit)]
		private struct InputUnion
		{
			[FieldOffset(0)] public MouseInput mi;
			[FieldOffset(0)] public KeyboardInput ki;
			[FieldOffset(0)] public HardwareInput hi;
		}
		private struct Input
		{
			public int type;
			public InputUnion u;
		}
		[Flags]
		private enum InputType
		{
			Mouse = 0,
			Keyboard = 1,
			Hardware = 2
		}
		[Flags]
		private enum KeyEventF
		{
			KeyDown = 0x0000,
			ExtendedKey = 0x0001,
			KeyUp = 0x0002,
			Unicode = 0x0004,
			Scancode = 0x0008
		}
		[Flags]
		private enum MouseEventF
		{
			Absolute = 0x8000,
			HWheel = 0x01000,
			Move = 0x0001,
			MoveNoCoalesce = 0x2000,
			LeftDown = 0x0002,
			LeftUp = 0x0004,
			RightDown = 0x0008,
			RightUp = 0x0010,
			MiddleDown = 0x0020,
			MiddleUp = 0x0040,
			VirtualDesk = 0x4000,
			Wheel = 0x0800,
			XDown = 0x0080,
			XUp = 0x0100
		}
		[DllImport("user32.dll", SetLastError = true)]
		private static extern uint SendInput(uint nInputs, Input[] pInputs, int cbSize);
		[DllImport("user32.dll", EntryPoint = "BlockInput")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool BlockInput([MarshalAs(UnmanagedType.Bool)] bool fBlockIt);
		[DllImport("user32.dll")]
		private static extern IntPtr GetMessageExtraInfo();
		[DllImport("user32.dll")]
		public static extern bool GetAsyncKeyState(Keys keys);
		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
		public static extern short GetKeyState(int keyCode);
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GetCursorPos(out Point point);

		[DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
		public static extern int BitBlt(IntPtr hDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);
		public static string ToReadableKey(this string key)
		{
			Dictionary<string, string> friendlyValues = new Dictionary<string, string>
			{
				{ "vk_lbutton", "Mouse 1" },
				{ "vk_rbutton", "Mouse 2" },
				{ "vk_mbutton", "Mouse 3" },
				{ "vk_xbutton2", "Mouse 5" },
				{ "vk_xbutton1", "Mouse 4" },
				{ "pgup", "Page Up" },
				{ "pgdn", "Page Down" },
				{ "pageup", "Page Up" },
				{ "pagedn", "Page Down" },
				{ "prev", "Page Up" },
				{ "next", "Page Down" },
				{ "ins", "Insert" },
				{ "oem5", "Backslash (\\)" },
				{ "oemopenbrackets", "[" },
				{ "oem6", "]" },
				{ "oem1", "Semi-Colon (;)" },
				{ "oem7", "Apostrophe (')" },
				{ "oemclear", "Clear" },
				{ "oemcomma", "Comma (,)" },
				{ "oemperiod", "Period (.)" },
				{ "oemquestion", "Question (?)" },
				{ "oemminus", "Minus (-)" },
				{ "oemplus", "Plus (+)" },
				{ "oemtilde", "Tilde (`)" },
				{ "controlkey", "Control" },
				{ "shiftkey", "Shift" },
				{ "menu", "Alt" },
				{ "lwin", "Windows" },
				{ "capital", "Caps Lock" },
				{ "vk_insert", "Insert" },
				{ "vk_delete", "Delete" },
				{ "vk_home", "Home" },
				{ "vk_end", "End" },
				{ "vk_prior", "Page Up" },
				{ "vk_next", "Page Down" },
				{ "vk_up", "Up" },
				{ "vk_down", "Down" },
				{ "vk_left", "Left" },
				{ "vk_right", "Right" },
				{ "vk_numlock", "Num Lock" },
				{ "vk_divide", "Numpad /" },
				{ "vk_multiply", "Numpad *" },
				{ "vk_subtract", "Numpad -" },
				{ "vk_add", "Numpad +" },
				{ "vk_decimal", "Numpad ." },
				{ "vk_numpad0", "Numpad 0" },
				{ "vk_numpad1", "Numpad 1" },
				{ "vk_numpad2", "Numpad 2" },
				{ "vk_numpad3", "Numpad 3" },
				{ "vk_numpad4", "Numpad 4" },
				{ "vk_numpad5", "Numpad 5" },
				{ "vk_numpad6", "Numpad 6" },
				{ "vk_numpad7", "Numpad 7" },
				{ "vk_numpad8", "Numpad 8" },
				{ "vk_numpad9", "Numpad 9"},
				{ "off", "None" }
			};
			bool hasValue = friendlyValues.TryGetValue(key.ToLower(), out string value);
			return (hasValue && (value != "Error")) ? value : key.Replace("VK_", "").Replace("vk_", "");
		}
		public static DirectInputKey VkToDik(Keys vK)
		{
			Dictionary<Keys, DirectInputKey> keyValues = new Dictionary<Keys, DirectInputKey>()
			{
				{Keys.A, DirectInputKey.A},
				{Keys.B, DirectInputKey.B},
				{Keys.C, DirectInputKey.C},
				{Keys.D, DirectInputKey.D},
				{Keys.E, DirectInputKey.E},
				{Keys.F, DirectInputKey.F},
				{Keys.G, DirectInputKey.G},
				{Keys.H, DirectInputKey.H},
				{Keys.I, DirectInputKey.I},
				{Keys.J, DirectInputKey.J},
				{Keys.K, DirectInputKey.K},
				{Keys.L, DirectInputKey.L},
				{Keys.M, DirectInputKey.M},
				{Keys.N, DirectInputKey.N},
				{Keys.O, DirectInputKey.O},
				{Keys.P, DirectInputKey.P},
				{Keys.Q, DirectInputKey.Q},
				{Keys.R, DirectInputKey.R},
				{Keys.S, DirectInputKey.S},
				{Keys.T, DirectInputKey.T},
				{Keys.U, DirectInputKey.U},
				{Keys.V, DirectInputKey.V},
				{Keys.W, DirectInputKey.W},
				{Keys.X, DirectInputKey.X},
				{Keys.Y, DirectInputKey.Y},
				{Keys.Z, DirectInputKey.Z},

				{Keys.D0, DirectInputKey.ZERO},
				{Keys.D1, DirectInputKey.ONE},
				{Keys.D2, DirectInputKey.TWO},
				{Keys.D3, DirectInputKey.THREE},
				{Keys.D4, DirectInputKey.FOUR},
				{Keys.D5, DirectInputKey.FIVE},
				{Keys.D6, DirectInputKey.SIX},
				{Keys.D7, DirectInputKey.SEVEN},
				{Keys.D8, DirectInputKey.EIGHT},
				{Keys.D9, DirectInputKey.NINE},

				{Keys.OemMinus, DirectInputKey.MINUS },
				{Keys.Oemplus, DirectInputKey.EQUALS },
				{Keys.OemOpenBrackets, DirectInputKey.LBRACKET },
				{Keys.Oem6, DirectInputKey.RBRACKET },
				{Keys.Oemcomma, DirectInputKey.COMMA },
				{Keys.OemPeriod, DirectInputKey.PERIOD },
				{Keys.OemQuestion, DirectInputKey.SLASH },
				{Keys.Oem1, DirectInputKey.SEMICOLON },
				{Keys.Oem7, DirectInputKey.APOSTROPHE },
				{Keys.LMenu, DirectInputKey.LMENU },
				{Keys.RMenu, DirectInputKey.RMENU },
				{Keys.LControlKey, DirectInputKey.LCONTROL },
				{Keys.RControlKey, DirectInputKey.RCONTROL },
				{Keys.LShiftKey, DirectInputKey.LSHIFT },
				{Keys.RShiftKey, DirectInputKey.RSHIFT },

				{Keys.Up, DirectInputKey.UP },
				{Keys.Down, DirectInputKey.DOWN },
				{Keys.Left, DirectInputKey.LEFT },
				{Keys.Right, DirectInputKey.RIGHT },
				{Keys.Enter, DirectInputKey.RETURN },
				{Keys.Escape, DirectInputKey.ESCAPE },
				{Keys.Back, DirectInputKey.BACK },
			};
			bool hasValue = keyValues.TryGetValue(vK, out DirectInputKey value);
			return (hasValue) ? value : DirectInputKey.INSERT;
		}
		public static DirectInputKey StrToDik(String key)
		{
			Dictionary<string, DirectInputKey> keyValues = new Dictionary<string, DirectInputKey>()
			{
				{ "0", VirtualInput.DirectInputKey.ZERO },
				{ "1", VirtualInput.DirectInputKey.ONE },
				{ "2", VirtualInput.DirectInputKey.TWO },
				{ "3", VirtualInput.DirectInputKey.THREE },
				{ "4", VirtualInput.DirectInputKey.FOUR },
				{ "5", VirtualInput.DirectInputKey.FIVE },
				{ "6", VirtualInput.DirectInputKey.SIX },
				{ "7", VirtualInput.DirectInputKey.SEVEN },
				{ "8", VirtualInput.DirectInputKey.EIGHT },
				{ "9", VirtualInput.DirectInputKey.NINE },
				{ "a", VirtualInput.DirectInputKey.A },
				{ "b", VirtualInput.DirectInputKey.B },
				{ "c", VirtualInput.DirectInputKey.C },
				{ "d", VirtualInput.DirectInputKey.D },
				{ "e", VirtualInput.DirectInputKey.E },
				{ "f", VirtualInput.DirectInputKey.F },
				{ "g", VirtualInput.DirectInputKey.G },
				{ "h", VirtualInput.DirectInputKey.H },
				{ "i", VirtualInput.DirectInputKey.I },
				{ "j", VirtualInput.DirectInputKey.J },
				{ "k", VirtualInput.DirectInputKey.K },
				{ "l", VirtualInput.DirectInputKey.L },
				{ "m", VirtualInput.DirectInputKey.M },
				{ "n", VirtualInput.DirectInputKey.N },
				{ "o", VirtualInput.DirectInputKey.O },
				{ "p", VirtualInput.DirectInputKey.P },
				{ "q", VirtualInput.DirectInputKey.Q },
				{ "r", VirtualInput.DirectInputKey.R },
				{ "s", VirtualInput.DirectInputKey.S },
				{ "t", VirtualInput.DirectInputKey.T },
				{ "u", VirtualInput.DirectInputKey.U },
				{ "v", VirtualInput.DirectInputKey.V },
				{ "w", VirtualInput.DirectInputKey.W },
				{ "x", VirtualInput.DirectInputKey.X },
				{ "y", VirtualInput.DirectInputKey.Y },
				{ "z", VirtualInput.DirectInputKey.Z },
				{ "minus", VirtualInput.DirectInputKey.MINUS },
				{ "equals", VirtualInput.DirectInputKey.EQUALS },
				{ "lbracket", VirtualInput.DirectInputKey.LBRACKET },
				{ "rbracket", VirtualInput.DirectInputKey.RBRACKET },
				{ "comma", VirtualInput.DirectInputKey.COMMA },
				{ "period", VirtualInput.DirectInputKey.PERIOD },
				{ "slash", VirtualInput.DirectInputKey.SLASH },
				{ "semicolon", VirtualInput.DirectInputKey.SEMICOLON },
				{ "apostrophe", VirtualInput.DirectInputKey.APOSTROPHE },
				{ "windows", VirtualInput.DirectInputKey.LWIN },
				{ "alt", VirtualInput.DirectInputKey.LMENU },
				{ "menu", VirtualInput.DirectInputKey.LMENU },
				{ "lmenu", VirtualInput.DirectInputKey.LMENU },
				{ "rmenu", VirtualInput.DirectInputKey.RMENU },
				{ "ctrl", VirtualInput.DirectInputKey.LCONTROL },
				{ "control", VirtualInput.DirectInputKey.LCONTROL },
				{ "lcontrol", VirtualInput.DirectInputKey.LCONTROL },
				{ "rcontrol", VirtualInput.DirectInputKey.RCONTROL },
				{ "up", VirtualInput.DirectInputKey.UP },
				{ "down",   VirtualInput.DirectInputKey.DOWN },
				{ "left",   VirtualInput.DirectInputKey.LEFT },
				{ "right",  VirtualInput.DirectInputKey.RIGHT },
				{ "esc",    VirtualInput.DirectInputKey.ESCAPE },
				{ "backspace",  VirtualInput.DirectInputKey.BACK },
				{ "enter",  VirtualInput.DirectInputKey.RETURN },
				{ "tab",    VirtualInput.DirectInputKey.TAB },
				{ "f1", VirtualInput.DirectInputKey.F1 },
				{ "f2", VirtualInput.DirectInputKey.F2 },
				{ "f3", VirtualInput.DirectInputKey.F3 },
				{ "f4", VirtualInput.DirectInputKey.F4 },
				{ "f5", VirtualInput.DirectInputKey.F5 },
				{ "f6", VirtualInput.DirectInputKey.F6 },
				{ "f7", VirtualInput.DirectInputKey.F7 },
				{ "f8", VirtualInput.DirectInputKey.F8 },
				{ "f9", VirtualInput.DirectInputKey.F9 },
				{ "f10", VirtualInput.DirectInputKey.F10 },
				{ "f11", VirtualInput.DirectInputKey.F11 },
				{ "f12", VirtualInput.DirectInputKey.F12 },
				{ "f13", VirtualInput.DirectInputKey.F13 },
				{ "f14", VirtualInput.DirectInputKey.F14 },
				{ "f15", VirtualInput.DirectInputKey.F15 },
				{ "pause", VirtualInput.DirectInputKey.PAUSE },
				{ "lwin", VirtualInput.DirectInputKey.LWIN },
				{ "rwin", VirtualInput.DirectInputKey.RWIN },
				{ "apps", VirtualInput.DirectInputKey.APPS },
				{ "sleep", VirtualInput.DirectInputKey.SLEEP },
				{ "num0", VirtualInput.DirectInputKey.NUMPAD0 },
				{ "num1", VirtualInput.DirectInputKey.NUMPAD1 },
				{ "num2", VirtualInput.DirectInputKey.NUMPAD2 },
				{ "scrolllock", VirtualInput.DirectInputKey.SCROLL },
				{ "capslock", VirtualInput.DirectInputKey.CAPITAL },
				{ "numlock", VirtualInput.DirectInputKey.NUMLOCK },
				{ "shift", VirtualInput.DirectInputKey.LSHIFT },
				{ "lshift", VirtualInput.DirectInputKey.LSHIFT },
				{ "rshift", VirtualInput.DirectInputKey.RSHIFT },
				{ "space", VirtualInput.DirectInputKey.SPACE },
				{ "[", VirtualInput.DirectInputKey.LBRACKET },
				{ "]", VirtualInput.DirectInputKey.RBRACKET },
				{ "\\", VirtualInput.DirectInputKey.BACKSLASH },
				{ "pgup", VirtualInput.DirectInputKey.PRIOR },
				{ "pgdn", VirtualInput.DirectInputKey.NEXT },
				{ "insert", VirtualInput.DirectInputKey.INSERT },
				{ "delete", VirtualInput.DirectInputKey.DELETE },
				{ "home", VirtualInput.DirectInputKey.HOME },
				{ "end", VirtualInput.DirectInputKey.END },
				{ "-", VirtualInput.DirectInputKey.MINUS },
				{ "=", VirtualInput.DirectInputKey.EQUALS },
				{ ";", VirtualInput.DirectInputKey.SEMICOLON },
				{ "'", VirtualInput.DirectInputKey.APOSTROPHE },
				{ ",", VirtualInput.DirectInputKey.COMMA },
				{ ".", VirtualInput.DirectInputKey.PERIOD },
				{ "/", VirtualInput.DirectInputKey.SLASH },
				{ "`", VirtualInput.DirectInputKey.GRAVE },
			};
			bool hasValue = keyValues.TryGetValue(key, out DirectInputKey value);
			return (hasValue) ? value : VkToDik((Keys)Enum.Parse(typeof(Keys), key, true));
		}
		public static string CleanInput(this string input)
		{
			string res = input;
			switch (input.ToString())
			{
				case "D0": res = "0"; break;
				case "D1": res = "1"; break;
				case "D2": res = "2"; break;
				case "D3": res = "3"; break;
				case "D4": res = "4"; break;
				case "D5": res = "5"; break;
				case "D6": res = "6"; break;
				case "D7": res = "7"; break;
				case "D8": res = "8"; break;
				case "D9": res = "9"; break;
				case "Capital": res = "CapsLock"; break;
			}
			if (res.EndsWith("Key")) res = res.Substring(0, res.Length - 3);

			return res;
		}
		public static bool CharIsCaptial(char c)
		{
			return (c >= 'A' && c <= 'Z');
		}
		public static bool CharIsPunctuation(char c)
		{
			return (c == '.' || c == ',' || c == ';' || c == ':' || c == '!' || c == '?' || c == '\'' || c == '\"' || c == '(' || c == ')' || c == '[' || c == ']' || c == '{' || c == '}' || c == '<' || c == '>');
		}
		public static string GetUnshiftedChar(char c)
		{
			Dictionary<char, string> keyValues = new Dictionary<char, string>()
			{
				{'!', "1"},
				{'@', "2"},
				{'#', "3"},
				{'$', "4"},
				{'%', "5"},
				{'^', "6"},
				{'&', "7"},
				{'*', "8"},
				{'(', "9"},
				{')', "0"},
				{'_', "-"},
				{'+', "="},
				{'{', "["},
				{'}', "]"},
				{'|', "\\"},
				{':', ";"},
				{'"', "'"},
				{'<', ","},
				{'>', "."},
				{'?', "/"}
			};
			bool hasValue = keyValues.TryGetValue(c, out string value);
			return (hasValue) ? value : string.Empty;
		}

		internal class Mouse
		{
			public static uint Move(int x, int y)
			{
				switch(Settings.Engine.InputMethod)
				{
					case 0: // SendInput()
						Input[] inputs = [
							new Input
							{
								type = (int) InputType.Mouse,
								u = new InputUnion
								{
									mi = new MouseInput
									{
										dx = x,
										dy = y,
										dwFlags = (uint)MouseEventF.Move,
										dwExtraInfo = GetMessageExtraInfo()
									}
								}
							}
						];
						return SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));
					case 1: // Interception Driver
						Interception.Mouse = new(null);
						bool res = Interception.Mouse.MoveCursorBy(x, y, false);
						Interception.Mouse.Dispose();
						return (res) ? 1u : 0u;
					case 2: // Logitech CVE
						if (Settings.Engine.HasGhub)
							Logitech.Driver.Move(0, x, y, 0);
						return 1u;
				}
				return 0u;
			}
			public static uint Move(Vector2 target)
			{
				return Move((int)target.X, (int)target.Y);
			}
			public static uint MoveTo(int x, int y)
			{
				Input[] inputs = new Input[]
				{
					new Input
					{
						type = (int) InputType.Mouse,
						u = new InputUnion
						{
							mi = new MouseInput
							{
								dx = x,
								dy = y,
								dwFlags = (uint)MouseEventF.Move | (uint)MouseEventF.VirtualDesk | (uint)MouseEventF.Absolute,
								dwExtraInfo = GetMessageExtraInfo()
							}
						}
					}
				};

				return SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));
			}
			public static uint MoveWheel(int value)
			{
				int WHEEL_DELTA = 120;
				Point Location;
				GetCursorPos(out Location);
				Input[] inputs = new Input[]
				{
					new Input
					{
						type = (int) InputType.Mouse,
						u = new InputUnion
						{
							mi = new MouseInput
							{
								dx = Location.X,
								dy = Location.Y,
								mouseData = (uint)(value * WHEEL_DELTA),
								dwFlags = (uint)MouseEventF.Wheel,
								dwExtraInfo = GetMessageExtraInfo()
							}
						}
					}
				};

				return SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));
			}
			public static uint Click(MouseKey mk)
			{
				MouseEventF mefd;
				MouseEventF mefu;
				switch (mk)
				{
					case MouseKey.LButton:
						mefd = MouseEventF.LeftDown;
						mefu = MouseEventF.LeftUp;
						break;
					case MouseKey.RButton:
						mefd = MouseEventF.RightDown;
						mefu = MouseEventF.RightUp;
						break;
					case MouseKey.MButton:
						mefd = MouseEventF.MiddleDown;
						mefu = MouseEventF.MiddleUp;
						break;
					default:
						mefd = MouseEventF.LeftDown;
						mefu = MouseEventF.LeftUp;
						break;
				}
				Input[] inputs = new Input[]
				{
					new Input
					{
						type = (int) InputType.Mouse,
						u = new InputUnion
						{
							mi = new MouseInput
							{
								dwFlags = (uint)mefd,
								dwExtraInfo = GetMessageExtraInfo()
							}
						}
					},
					new Input
					{
						type = (int) InputType.Mouse,
						u = new InputUnion
						{
							mi = new MouseInput
							{
								dwFlags = (uint)mefu,
								dwExtraInfo = GetMessageExtraInfo()
							}
						}
					}
				};
				return SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));
			}
			public static uint Click(Keys mk)
			{
				MouseKey mK = MouseKey.LButton;
				switch (mk)
				{
					case Keys.LButton: mK = MouseKey.LButton; break;
					case Keys.RButton: mK = MouseKey.RButton; break;
					case Keys.MButton: mK = MouseKey.MButton; break;
					case Keys.XButton1: mK = MouseKey.XButton1; break;
					case Keys.XButton2: mK = MouseKey.XButton2; break;
				}
				return Click(mK);
			}
			public static uint Down(MouseKey mk)
			{
				MouseEventF mefd;
				switch (mk)
				{
					case MouseKey.LButton:
						mefd = MouseEventF.LeftDown;
						break;
					case MouseKey.RButton:
						mefd = MouseEventF.RightDown;
						break;
					case MouseKey.MButton:
						mefd = MouseEventF.MiddleDown;
						break;
					default:
						mefd = MouseEventF.LeftDown;
						break;
				}
				Input[] inputs = new Input[]
				{
					new Input
					{
						type = (int) InputType.Mouse,
						u = new InputUnion
						{
							mi = new MouseInput
							{
								dwFlags = (uint)mefd,
								dwExtraInfo = GetMessageExtraInfo()
							}
						}
					}
				};
				return SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));
			}
			public static uint Down(Keys mk)
			{
				if (Settings.Engine.InputMethod == 1)
				{
					bool res = false;
					switch (mk)
					{
						case Keys.LButton:
							res = Interception.Mouse.SimulateLeftButtonDown();
							break;
						case Keys.MButton:
							res = Interception.Mouse.SetMouseState(MouseState.MiddleButtonDown);
							break;
						case Keys.RButton:
							res = Interception.Mouse.SetMouseState(MouseState.RightButtonDown);
							break;
					}
					return res ? 1u : 0u;
				}
				else
				{
					MouseKey mK = MouseKey.LButton;
					switch (mk)
					{
						case Keys.LButton: mK = MouseKey.LButton; break;
						case Keys.RButton: mK = MouseKey.RButton; break;
						case Keys.MButton: mK = MouseKey.MButton; break;
						case Keys.XButton1: mK = MouseKey.XButton1; break;
						case Keys.XButton2: mK = MouseKey.XButton2; break;
					}
					return Down(mK);
				}
			}
			public static uint Up(MouseKey mk)
			{
				MouseEventF mefu;
				switch (mk)
				{
					case MouseKey.LButton:
						mefu = MouseEventF.LeftUp;
						break;
					case MouseKey.RButton:
						mefu = MouseEventF.RightUp;
						break;
					case MouseKey.MButton:
						mefu = MouseEventF.MiddleUp;
						break;
					default:
						mefu = MouseEventF.LeftUp;
						break;
				}
				Input[] inputs = new Input[]
				{
					new Input
					{
						type = (int) InputType.Mouse,
						u = new InputUnion
						{
							mi = new MouseInput
							{
								dwFlags = (uint)mefu,
								dwExtraInfo = GetMessageExtraInfo()
							}
						}
					}
				};
				return SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));
			}
			public static uint Up(Keys mk)
			{

				if (Settings.Engine.InputMethod == 1)
				{
					bool res = false;
					switch (mk)
					{
						case Keys.LButton:
							res = Interception.Mouse.SimulateLeftButtonUp();
							break;
						case Keys.MButton:
							res = Interception.Mouse.SetMouseState(MouseState.MiddleButtonUp);
							break;
						case Keys.RButton:
							res = Interception.Mouse.SetMouseState(MouseState.RightButtonUp);
							break;
					}
					return res ? 1u : 0u;
				}
				else
				{
					MouseKey mK = MouseKey.LButton;
					switch (mk)
					{
						case Keys.LButton: mK = MouseKey.LButton; break;
						case Keys.RButton: mK = MouseKey.RButton; break;
						case Keys.MButton: mK = MouseKey.MButton; break;
						case Keys.XButton1: mK = MouseKey.XButton1; break;
						case Keys.XButton2: mK = MouseKey.XButton2; break;
					}
					return Up(mK);
				}
			}
			public static Point GetPosition()
			{
				if (Settings.Engine.InputMethod == 1)
				{
					Win32Point iCurPos = Interception.Mouse.GetCursorPosition();
					return new Point(iCurPos.X, iCurPos.Y);
				}
				else
				{
					GetCursorPos(out Point loc);
					return loc;
				}
			}
		}
		internal class Keyboard
		{
			public static uint Press(DirectInputKey dik)
			{
				Input[] inputs = new Input[]
				{
					new Input
					{
						type = (int)InputType.Keyboard,
						u = new InputUnion
						{
							ki = new KeyboardInput
							{
								wVk = 0,
								wScan = (byte)dik,
								dwFlags = (uint)(KeyEventF.KeyDown | KeyEventF.Scancode),
								dwExtraInfo = GetMessageExtraInfo()
							}
						}
					},
					new Input
					{
						type = (int)InputType.Keyboard,
						u = new InputUnion
						{
							ki = new KeyboardInput
							{
								wVk = 0,
								wScan = (byte)dik,
								dwFlags = (uint)(KeyEventF.KeyUp | KeyEventF.Scancode),
								dwExtraInfo = GetMessageExtraInfo()
							}
						}
					}
				};

				return SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));
			}
			public static uint Press(Keys mk)
			{
				DirectInputKey mK = VkToDik(mk);
				return Press(mK);
			}
			public static uint Down(DirectInputKey dik)
			{
				Input[] inputs = new Input[]
				{
					new Input
					{
						type = (int)InputType.Keyboard,
						u = new InputUnion
						{
							ki = new KeyboardInput
							{
								wVk = 0,
								wScan = (byte)dik,
								dwFlags = (uint)(KeyEventF.KeyDown | KeyEventF.Scancode),
								dwExtraInfo = GetMessageExtraInfo()
							}
						}
					}
				};

				return SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));
			}
			public static uint Down(Keys mk)
			{
				DirectInputKey mK = VkToDik(mk);
				return Down(mK);
			}
			public static uint Up(DirectInputKey dik)
			{
				Input[] inputs = new Input[]
				{
					new Input
					{
						type = (int)InputType.Keyboard,
						u = new InputUnion
						{
							ki = new KeyboardInput
							{
								wVk = 0,
								wScan = (byte)dik,
								dwFlags = (uint)(KeyEventF.KeyUp | KeyEventF.Scancode),
								dwExtraInfo = GetMessageExtraInfo()
							}
						}
					}
				};

				return SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));
			}
			public static uint Up(Keys mk)
			{
				DirectInputKey mK = VkToDik(mk);
				return Up(mK);
			}
		}
		internal class Screen
		{
			public static Color GetPixelColor(Point location)
			{
				Bitmap screenPixel = new Bitmap(1, 1, PixelFormat.Format32bppArgb);
				using (Graphics gdest = Graphics.FromImage(screenPixel))
				{
					using (Graphics gsrc = Graphics.FromHwnd(IntPtr.Zero))
					{
						IntPtr hSrcDC = gsrc.GetHdc();
						IntPtr hDC = gdest.GetHdc();
						int retval = BitBlt(hDC, 0, 0, 1, 1, hSrcDC, location.X, location.Y, (int)CopyPixelOperation.SourceCopy);
						gdest.ReleaseHdc();
						gsrc.ReleaseHdc();
					}
				}

				return screenPixel.GetPixel(0, 0);
			}
		}
	}
	internal static class ControllerInput
	{
		public enum ControllerStick
		{
			LeftX,
			LeftY,
			RightX,
			RightY
		}
		public enum ControllerTrigger
		{
			Left,
			Right
		}
		public enum ControllerKeys
		{
			A,
			B,
			X,
			Y,
			DPadUp,
			DPadDown,
			DPadLeft,
			DPadRight,
			Start,
			Back,
			LeftThumb,
			RightThumb,
			LeftShoulder,
			RightShoulder
		}
		private static bool IsSupportEnabled()
		{
			return Settings.Controller.Enabled;
		}
		public static bool IsWootingKeyboard()
		{
			State cState = new Controller(UserIndex.One).GetState();
			return cState.Gamepad.LeftThumbX == 0.00002;
		}
		public static bool IsTriggerHeld(ControllerTrigger cTrig)
		{
			if (!IsSupportEnabled()) return false;
			switch (cTrig)
			{
				case ControllerTrigger.Left:
					return Settings.Controller.State.Gamepad.LeftTrigger > 0;
				case ControllerTrigger.Right:
					return Settings.Controller.State.Gamepad.RightTrigger > 0;
				default:
					return false;
			}
		}
		public static bool IsKeyPressed(ControllerKeys cKey)
		{
			if (!IsSupportEnabled()) return false;
			switch (cKey)
			{
				case ControllerKeys.A:
					return Settings.Controller.State.Gamepad.Buttons.HasFlag(GamepadButtonFlags.A);
				case ControllerKeys.B:
					return Settings.Controller.State.Gamepad.Buttons.HasFlag(GamepadButtonFlags.B);
				case ControllerKeys.X:
					return Settings.Controller.State.Gamepad.Buttons.HasFlag(GamepadButtonFlags.X);
				case ControllerKeys.Y:
					return Settings.Controller.State.Gamepad.Buttons.HasFlag(GamepadButtonFlags.Y);
				case ControllerKeys.DPadUp:
					return Settings.Controller.State.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadUp);
				case ControllerKeys.DPadDown:
					return Settings.Controller.State.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadDown);
				case ControllerKeys.DPadLeft:
					return Settings.Controller.State.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadLeft);
				case ControllerKeys.DPadRight:
					return Settings.Controller.State.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadRight);
				case ControllerKeys.Start:
					return Settings.Controller.State.Gamepad.Buttons.HasFlag(GamepadButtonFlags.Start);
				case ControllerKeys.Back:
					return Settings.Controller.State.Gamepad.Buttons.HasFlag(GamepadButtonFlags.Back);
				case ControllerKeys.LeftThumb:
					return Settings.Controller.State.Gamepad.Buttons.HasFlag(GamepadButtonFlags.LeftThumb);
				case ControllerKeys.RightThumb:
					return Settings.Controller.State.Gamepad.Buttons.HasFlag(GamepadButtonFlags.RightThumb);
				case ControllerKeys.LeftShoulder:
					return Settings.Controller.State.Gamepad.Buttons.HasFlag(GamepadButtonFlags.LeftShoulder);
				case ControllerKeys.RightShoulder:
					return Settings.Controller.State.Gamepad.Buttons.HasFlag(GamepadButtonFlags.RightShoulder);
				default:
					return false;
			}
		}
		public static int GetHorizontalDelta(ControllerStick cStick)
		{
			if (!IsSupportEnabled()) return 0;
			switch (cStick)
			{
				case ControllerStick.LeftX:
					return Settings.Controller.State.Gamepad.LeftThumbX / 1000;
				case ControllerStick.RightX:
					return Settings.Controller.State.Gamepad.RightThumbX / 1000;
				default:
					return 0;
			}
		}
		public static int GetVerticalDelta(ControllerStick cStick)
		{
			if (!IsSupportEnabled()) return 0;
			switch (cStick)
			{
				case ControllerStick.LeftY:
					return -(Settings.Controller.State.Gamepad.LeftThumbY / 1000);
				case ControllerStick.RightY:
					return -(Settings.Controller.State.Gamepad.RightThumbY / 1000);
				default:
					return 0;
			}
		}
		public static ControllerKeys GetKeyFromString(string keyName)
		{
			switch (keyName)
			{
				case "A":
					return ControllerKeys.A;
				case "B":
					return ControllerKeys.B;
				case "X":
					return ControllerKeys.X;
				case "Y":
					return ControllerKeys.Y;
				case "DPadUp":
					return ControllerKeys.DPadUp;
				case "DPadDown":
					return ControllerKeys.DPadDown;
				case "DPadLeft":
					return ControllerKeys.DPadLeft;
				case "DPadRight":
					return ControllerKeys.DPadRight;
				case "Start":
					return ControllerKeys.Start;
				case "Select":
				case "Back":
					return ControllerKeys.Back;
				case "LeftThumb":
					return ControllerKeys.LeftThumb;
				case "RightThumb":
					return ControllerKeys.RightThumb;
				case "L1":
				case "LB":
				case "LeftShoulder":
					return ControllerKeys.LeftShoulder;
				case "R1":
				case "RB":
				case "RightShoulder":
					return ControllerKeys.RightShoulder;
				default:
					return ControllerKeys.A;
			}
		}
		public static ControllerTrigger GetTriggerFromString(string trigName)
		{
			switch (trigName)
			{
				case "L2":
				case "LT":
				case "Left":
					return ControllerTrigger.Left;
				case "R2":
				case "RT":
				case "Right":
					return ControllerTrigger.Right;
				default:
					return ControllerTrigger.Left;
			}
		}
	}
	internal static class InputKeys
	{
		public static string[] KeyNames = {
			"OFF",
			"VK_LBUTTON",
			"VK_RBUTTON",
			"VK_CANCEL",
			"VK_MBUTTON",
			"VK_XBUTTON1",
			"VK_XBUTTON2",
			"Unknown",
			"VK_BACK",
			"VK_TAB",
			"Unknown",
			"Unknown",
			"VK_CLEAR",
			"VK_RETURN",
			"Unknown",
			"Unknown",
			"VK_SHIFT",
			"VK_CONTROL",
			"VK_MENU",
			"VK_PAUSE",
			"VK_CAPITAL",
			"VK_KANA",
			"Unknown",
			"VK_JUNJA",
			"VK_FINAL",
			"VK_KANJI",
			"Unknown",
			"VK_ESCAPE",
			"VK_CONVERT",
			"VK_NONCONVERT",
			"VK_ACCEPT",
			"VK_MODECHANGE",
			"VK_SPACE",
			"VK_PRIOR",
			"VK_NEXT",
			"VK_END",
			"VK_HOME",
			"VK_LEFT",
			"VK_UP",
			"VK_RIGHT",
			"VK_DOWN",
			"VK_SELECT",
			"VK_PRINT",
			"VK_EXECUTE",
			"VK_SNAPSHOT",
			"VK_INSERT",
			"VK_DELETE",
			"VK_HELP",
			"0",
			"1",
			"2",
			"3",
			"4",
			"5",
			"6",
			"7",
			"8",
			"9",
			"Unknown",
			"Unknown",
			"Unknown",
			"Unknown",
			"Unknown",
			"Unknown",
			"Unknown",
			"A",
			"B",
			"C",
			"D",
			"E",
			"F",
			"G",
			"H",
			"I",
			"J",
			"K",
			"L",
			"M",
			"N",
			"O",
			"P",
			"Q",
			"R",
			"S",
			"T",
			"U",
			"V",
			"W",
			"X",
			"Y",
			"Z",
			"VK_LWIN",
			"VK_RWIN",
			"VK_APPS",
			"Unknown",
			"VK_SLEEP",
			"VK_NUMPAD0",
			"VK_NUMPAD1",
			"VK_NUMPAD2",
			"VK_NUMPAD3",
			"VK_NUMPAD4",
			"VK_NUMPAD5",
			"VK_NUMPAD6",
			"VK_NUMPAD7",
			"VK_NUMPAD8",
			"VK_NUMPAD9",
			"VK_MULTIPLY",
			"VK_ADD",
			"VK_SEPARATOR",
			"VK_SUBTRACT",
			"VK_DECIMAL",
			"VK_DIVIDE",
			"VK_F1",
			"VK_F2",
			"VK_F3",
			"VK_F4",
			"VK_F5",
			"VK_F6",
			"VK_F7",
			"VK_F8",
			"VK_F9",
			"VK_F10",
			"VK_F11",
			"VK_F12",
			"VK_F13",
			"VK_F14",
			"VK_F15",
			"VK_F16",
			"VK_F17",
			"VK_F18",
			"VK_F19",
			"VK_F20",
			"VK_F21",
			"VK_F22",
			"VK_F23",
			"VK_F24",
			"Unknown",
			"Unknown",
			"Unknown",
			"Unknown",
			"Unknown",
			"Unknown",
			"Unknown",
			"Unknown",
			"VK_NUMLOCK",
			"VK_SCROLL",
			"VK_OEM_NEC_EQUAL",
			"VK_OEM_FJ_MASSHOU",
			"VK_OEM_FJ_TOUROKU",
			"VK_OEM_FJ_LOYA",
			"VK_OEM_FJ_ROYA",
			"Unknown",
			"Unknown",
			"Unknown",
			"Unknown",
			"Unknown",
			"Unknown",
			"Unknown",
			"Unknown",
			"Unknown",
			"VK_LSHIFT",
			"VK_RSHIFT",
			"VK_LCONTROL",
			"VK_RCONTROL",
			"VK_LMENU",
			"VK_RMENU"
		};

		public static int[] KeyCodes = {
			0x0,  //Undefined
			0x01,
			0x02,
			0x03,
			0x04,
			0x05,
			0x06,
			0x07, //Undefined
			0x08,
			0x09,
			0x0A, //Reserved
			0x0B, //Reserved
			0x0C,
			0x0D,
			0x0E, //Undefined
			0x0F, //Undefined
			0x10,
			0x11,
			0x12,
			0x13,
			0x14,
			0x15,
			0x16, //IME On
			0x17,
			0x18,
			0x19,
			0x1A, //IME Off
			0x1B,
			0x1C,
			0x1D,
			0x1E,
			0x1F,
			0x20,
			0x21,
			0x22,
			0x23,
			0x24,
			0x25,
			0x26,
			0x27,
			0x28,
			0x29,
			0x2A,
			0x2B,
			0x2C,
			0x2D,
			0x2E,
			0x2F,
			0x30,
			0x31,
			0x32,
			0x33,
			0x34,
			0x35,
			0x36,
			0x37,
			0x38,
			0x39,
			0x3A, //Undefined
			0x3B, //Undefined
			0x3C, //Undefined
			0x3D, //Undefined
			0x3E, //Undefined
			0x3F, //Undefined
			0x40, //Undefined
			0x41,
			0x42,
			0x43,
			0x44,
			0x45,
			0x46,
			0x47,
			0x48,
			0x49,
			0x4A,
			0x4B,
			0x4C,
			0x4B,
			0x4E,
			0x4F,
			0x50,
			0x51,
			0x52,
			0x53,
			0x54,
			0x55,
			0x56,
			0x57,
			0x58,
			0x59,
			0x5A,
			0x5B,
			0x5C,
			0x5D,
			0x5E, //Rservered
			0x5F,
			0x60, //Numpad1
			0x61, //Numpad2
			0x62, //Numpad3
			0x63, //Numpad4
			0x64, //Numpad5
			0x65, //Numpad6
			0x66, //Numpad7
			0x67, //Numpad8
			0x68, //Numpad8
			0x69, //Numpad9
			0x6A,
			0x6B,
			0x6C,
			0x6D,
			0x6E,
			0x6F,
			0x70, //F1
			0x71, //F2
			0x72, //F3
			0x73, //F4
			0x74, //F5
			0x75, //F6
			0x76, //F7
			0x77, //F8
			0x78, //F9
			0x79, //F10
			0x7A, //F11
			0x7B, //F12
			0x7C, //F13
			0x7D, //F14
			0x7E, //F15
			0x7F, //F16
			0x80, //F17
			0x81, //F18
			0x82, //F19
			0x83, //F20
			0x84, //F21
			0x85, //F22
			0x86, //F23
			0x87, //F24
			0x88, //Unkown
			0x89, //Unkown
			0x8A, //Unkown
			0x8B, //Unkown
			0x8C, //Unkown
			0x8D, //Unkown
			0x8E, //Unkown
			0x8F, //Unkown
			0x90,
			0x91,
			0x92, //OEM Specific
			0x93, //OEM Specific
			0x94, //OEM Specific
			0x95, //OEM Specific
			0x96, //OEM Specific
			0x97, //Unkown
			0x98, //Unkown
			0x99, //Unkown
			0x9A, //Unkown
			0x9B, //Unkown
			0x9C, //Unkown
			0x9D, //Unkown
			0x9E, //Unkown 
			0x9F, //Unkown
			0xA0,
			0xA1,
			0xA2,
			0xA3,
			0xA4,
			0xA5
		};
		public static int KeyCodeFromName(string name)
		{
			Dictionary<string, Keys> keys = new()
			{
				{ "xbutton1", Keys.XButton1 },
				{ "xbutton2", Keys.XButton2 },
				{ "mouse 1", Keys.LButton },
				{ "mouse 2", Keys.RButton },
				{ "mouse 3", Keys.MButton },
				{ "f1", Keys.F1 },
				{ "f2", Keys.F2 },
				{ "f3", Keys.F3 },
				{ "f4", Keys.F4 },
				{ "f5", Keys.F5 },
				{ "f6", Keys.F6 },
				{ "f7", Keys.F7 },
				{ "f8", Keys.F8 },
				{ "f9", Keys.F9 },
				{ "f10", Keys.F10 },
				{ "f11", Keys.F11 },
				{ "f12", Keys.F12 },
				{ "f13", Keys.F13 },
				{ "f14", Keys.F14 },
				{ "f15", Keys.F15 },
				{ "f16", Keys.F16 },
				{ "f17", Keys.F17 },
				{ "f18", Keys.F18 },
				{ "f19", Keys.F19 },
				{ "f20", Keys.F20 },
				{ "f21", Keys.F21 },
				{ "f22", Keys.F22 },
				{ "f23", Keys.F23 },
				{ "f24", Keys.F24 },
				{ "numlock", Keys.NumLock },
				{ "scroll", Keys.Scroll },
				{ "lshift", Keys.LShiftKey },
				{ "rshift", Keys.RShiftKey },
				{ "lcontrol", Keys.LControlKey },
				{ "rcontrol", Keys.RControlKey },
				{ "lalt", Keys.LMenu },
				{ "ralt", Keys.RMenu },
				{ "lwin", Keys.LWin },
				{ "rwin", Keys.RWin },
				{ "apps", Keys.Apps },
				{ "capslock", Keys.CapsLock },
				{ "tab", Keys.Tab },
				{ "backspace", Keys.Back },
				{ "enter", Keys.Enter },
				{ "escape", Keys.Escape },
				{ "space", Keys.Space },
				{ "pageup", Keys.PageUp },
				{ "pagedown", Keys.PageDown },
				{ "end", Keys.End },
				{ "home", Keys.Home },
				{ "insert", Keys.Insert },
				{ "delete", Keys.Delete },
				{ "left", Keys.Left },
				{ "right", Keys.Right },
				{ "up", Keys.Up },
				{ "down", Keys.Down },
				{ "0", Keys.D0 },
				{ "1", Keys.D1 },
				{ "2", Keys.D2 },
				{ "3", Keys.D3 },
				{ "4", Keys.D4 },
				{ "5", Keys.D5 },
				{ "6", Keys.D6 },
				{ "7", Keys.D7 },
				{ "8", Keys.D8 },
				{ "9", Keys.D9 },
				{ "a", Keys.A },
				{ "b", Keys.B },
				{ "c", Keys.C },
				{ "d", Keys.D },
				{ "e", Keys.E },
				{ "f", Keys.F },
				{ "g", Keys.G },
				{ "h", Keys.H },
				{ "i", Keys.I },
				{ "j", Keys.J },
				{ "k", Keys.K },
				{ "l", Keys.L },
				{ "m", Keys.M },
				{ "n", Keys.N},
				{ "o", Keys.O },
				{ "p", Keys.P },
				{ "q", Keys.Q },
				{ "r", Keys.R },
				{ "s", Keys.S },
				{ "t", Keys.T },
				{ "u", Keys.U },
				{ "v", Keys.V },
				{ "w", Keys.W },
				{ "x", Keys.X },
				{ "y", Keys.Y },
				{ "z", Keys.Z },
				{ "numpad 0", Keys.NumPad0 },
				{ "numpad 1", Keys.NumPad1 },
				{ "numpad 2", Keys.NumPad2 },
				{ "numpad 3", Keys.NumPad3 },
				{ "numpad 4", Keys.NumPad4 },
				{ "numpad 5", Keys.NumPad5 },
				{ "numpad 6", Keys.NumPad6 },
				{ "numpad 7", Keys.NumPad7 },
				{ "numpad 8", Keys.NumPad8 },
				{ "numpad 9", Keys.NumPad9 },
				{ "multiply", Keys.Multiply },
				{ "add", Keys.Add },
				{ "subtract", Keys.Subtract },
				{ "decimal", Keys.Decimal },
				{ "divide", Keys.Divide },
				{ "oem1", Keys.Oem1 },
				{ "oemplus", Keys.Oemplus },
				{ "oemcomma", Keys.Oemcomma },
				{ "oemminus", Keys.OemMinus },
				{ "oemperiod", Keys.OemPeriod },
				{ "oemclear", Keys.OemClear },
				{ "oem 2", Keys.Oem2 },
				{ "oem 3", Keys.Oem3 },
				{ "oem 4", Keys.Oem4 },
				{ "oem 5", Keys.Oem5 },
				{ "oem 6", Keys.Oem6 },
				{ "oem 7", Keys.Oem7 },
				{ "oem 8", Keys.Oem8 },
				{ "oem 102", Keys.Oem102 },
			};

			if (keys.ContainsKey(name.ToLower()))
			{
				return (int)keys[name.ToLower()];
			}
			else
			{
				return 0x00;
			}
		}
	}
}
