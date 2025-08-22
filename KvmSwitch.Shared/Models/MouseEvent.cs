using System;
using System.Runtime.InteropServices;
namespace Shared
{
    public class MouseEvent
    {
        [DllImport("user32.dll")] // For the GetCursorPosition function
        private static extern bool GetCursorPos(out POINT lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        public static (int X, int Y) GetCursorPosition()
        {
            if (GetCursorPos(out POINT point)) return (point.X, point.Y);
            throw new InvalidOperationException("Failed to retrieve cursor position.");
        }
        public static int GetX()
        {
            if (GetCursorPos(out POINT point)) return point.X;
            throw new InvalidOperationException("Failed to retrieve cursor position.");
        }
        public static int GetY()
        {
            if (GetCursorPos(out POINT point)) return point.Y;
            throw new InvalidOperationException("Failed to retrieve cursor position.");
        }

    }
    public class MouseMovementEventArgs : EventArgs
    {
        public uint ClickType { get; set; }
        public short ScrollSpeed { get; set; }
        public int VelocityX { get; set; }
        public int VelocityY { get; set; }
        public double TimeDelta { get; set; }
    }
    public class KeyboardInputEventArgs : EventArgs
    {
        public ushort Key { get; set; }
        public ushort KeyInputType { get; set; }
    }
}