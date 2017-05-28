using System;
using System.Runtime.InteropServices;

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
}
