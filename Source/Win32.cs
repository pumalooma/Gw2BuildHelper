using System;
using System.Runtime.InteropServices;
using System.Windows;

public static class Win32 {
	[DllImport("user32.dll", SetLastError = true)]
	public static extern IntPtr FindWindow (string lpClassName, string lpWindowName);
	
    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool GetWindowRect (IntPtr hwnd, out RECT lpRect);

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT {
        public int Left;        // x position of upper-left corner
        public int Top;         // y position of upper-left corner
        public int Right;       // x position of lower-right corner
        public int Bottom;      // y position of lower-right corner
    }


	public const int KEY_DOWN = 0x8000;
	public const int KEY_TOGGLED = 0x0001;

	[DllImport("user32.dll")]
	public static extern short GetKeyState(int nVirtKey);
	[DllImport("user32.dll")]
	public static extern short GetAsyncKeyState(int nVirtKey);

	public static bool IsKeyDown(int key)
	{
		return KEY_DOWN == (GetKeyState(key) & KEY_DOWN);
	}

	public static bool IsKeyToggled(int key)
	{
		return KEY_TOGGLED == (GetAsyncKeyState(key) & KEY_TOGGLED);
	}


	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static extern bool GetCursorPos(ref Win32Point pt);

	[StructLayout(LayoutKind.Sequential)]
	internal struct Win32Point
	{
		public Int32 X;
		public Int32 Y;
	};
	public static Point GetMousePosition()
	{
		Win32Point w32Mouse = new Win32Point();
		GetCursorPos(ref w32Mouse);
		return new Point(w32Mouse.X, w32Mouse.Y);
	}
}
