using System;
using System.Runtime.InteropServices;
namespace Shared
{
    public static class MouseEvent
    {
        [DllImport("user32.dll")]
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
        public static int DeltaX { get; set; }
         public static int DeltaY { get; set; }
    }
}