using System;
using System.Runtime.InteropServices;
namespace Shared
{
    public class MouseEvent
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct RAWINPUTDEVICE
        {
            public ushort usUsagePage;
            public ushort usUsage;
            public uint dwFlags;
            public IntPtr hwndTarget;
        }

        [DllImport("user32.dll")] // For the GetCursorPosition function
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")] // For the StartTracking function
        private static extern bool RegisterRawInputDevices(RAWINPUTDEVICE[] pRawInputDevices, uint uiNumDevices, uint cbSize);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        public bool StartTracking()
        {
            RAWINPUTDEVICE[] devices = new RAWINPUTDEVICE[1];
            devices[0].usUsagePage = 0x01;
            devices[0].usUsage = 0x02;
            devices[0].dwFlags = 0;
            devices[0].hwndTarget = IntPtr.Zero;
            if (!RegisterRawInputDevices(devices, 1, (uint)Marshal.SizeOf(typeof(RAWINPUTDEVICE))))
                return false;
            return true;
        }

        public static (int X, int Y) GetCursorPosition()
        {
            if (GetCursorPos(out POINT point)) return (point.X, point.Y);
            throw new InvalidOperationException("Failed to retrieve cursor position.");
        }
        public static int DeltaX { get; set; }
        public static int DeltaY { get; set; }
    }
}