using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LEAGUEAIM.Win32
{
	internal class User32
	{
		public const ulong WDA_EXCLUDEFROMCAPTURE = 0x00000011;
		public const ulong WDA_NONE = 0x00000000;
		public const int WM_SYSCOMMAND = 0x0112;
		public const int SC_CLOSE = 0xF060;

		[StructLayout(LayoutKind.Sequential)]
		public struct POINT
		{
			public Int32 x;
			public Int32 y;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct CURSORINFO
		{
			public Int32 cbSize;        // Specifies the size, in bytes, of the structure.
										// The caller must set this to Marshal.SizeOf(typeof(CURSORINFO)).
			public Int32 flags;         // Specifies the cursor state. This parameter can be one of the following values:
										//    0             The cursor is hidden.
										//    CURSOR_SHOWING    The cursor is showing.
										//    CURSOR_SUPPRESSED    (Windows 8 and above.) The cursor is suppressed. This flag indicates that the system is not drawing the cursor because the user is providing input through touch or pen instead of the mouse.
			public IntPtr hCursor;          // Handle to the cursor.
			public POINT ptScreenPos;       // A POINT structure that receives the screen coordinates of the cursor.
		}

		[DllImport("kernel32.dll")]
		public static extern bool FreeConsole();
		[DllImport("user32.dll")]
		public static extern bool GetAsyncKeyState(Keys Key);
		[DllImport("user32.dll")]
		public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
		[DllImport("user32.dll")]
		public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
		[DllImport("user32.dll")]
		public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
		[DllImport("user32.dll")]
		public static extern int FindWindow(string lpClassName, string lpWindowName);
		[DllImport("user32.dll")]
		public static extern int SendMessage(int hWnd, uint Msg, int wParam, int lParam);
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
		[DllImport("user32.dll", EntryPoint = "GetWindowLong")]
		public static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);
		[DllImport("user32.dll")]
		public static extern int SetWindowLong(IntPtr hWnd, int nIndex, long dwNewLong);
		[DllImport("user32.dll")]
		public static extern uint SetWindowDisplayAffinity(IntPtr hWnd, ulong dwAffinity);
		[DllImport("user32.dll")]
		public static extern bool GetCursorInfo(ref CURSORINFO pci);
	}
}
