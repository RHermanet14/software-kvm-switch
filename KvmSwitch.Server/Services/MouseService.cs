using System.Windows.Forms;
using System.Drawing;
using Shared;
using System.Runtime.InteropServices;
using System.Configuration;
namespace services
{
    public static class MouseService
    {
        #region RAWMOUSE click types
        private const ushort RI_MOUSE_LEFT_BUTTON_DOWN = 0x0001;
        private const ushort RI_MOUSE_LEFT_BUTTON_UP = 0x0002;
        private const ushort RI_MOUSE_RIGHT_BUTTON_DOWN = 0x0004;
        private const ushort RI_MOUSE_RIGHT_BUTTON_UP = 0x0008;
        private const ushort RI_MOUSE_MIDDLE_BUTTON_DOWN = 0x0010;
        private const ushort RI_MOUSE_MIDDLE_BUTTON_UP = 0x0020;
        private const ushort RI_MOUSE_BUTTON_4_DOWN = 0x0040;
        private const ushort RI_MOUSE_BUTTON_4_UP = 0x0080;
        private const ushort RI_MOUSE_BUTTON_5_DOWN = 0x0100;
        private const ushort RI_MOUSE_BUTTON_5_UP = 0x0200;
        private const ushort RI_MOUSE_WHEEL = 0x0400; // Uses usButtonData to determine distance
        private const ushort RI_MOUSE_HWHEEL = 0x0800; // Uses usButtonData to determine distance
        #endregion
        #region mouse event click types
        // mouse events and rawmouse have different constants for clicks
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        private const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
        private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        private const uint MOUSEEVENTF_XDOWN = 0x0080; // Set XBUTTON in dwFlags
        private const uint MOUSEEVENTF_XUP = 0x0100;
        private const uint MOUSEEVENTF_WHEEL = 0x0800;
        private const uint MOUSEEVENTF_HWHEEL = 0x01000;
        private const uint XBUTTON1 = 0x0001;
        private const uint XBUTTON2 = 0x0002;
        #endregion
        #region handle clicks
        private const uint INPUT_MOUSE = 0;
        private const uint INPUT_KEYBOARD = 1;
        private const uint KEYEVENTF_SCANCODE = 0x0008;
        private const uint KEYEVENTF_KEYUP = 0x0002;
        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint timeDelta;
            public IntPtr dwExtraInfo;
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public UIntPtr dwExtraInfo;
        }
        [StructLayout(LayoutKind.Explicit)]
        private struct INPUTUNION
        {
            [FieldOffset(0)]
            public MOUSEINPUT mi;
            [FieldOffset(0)]
            public KEYBDINPUT ki;
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint type;
            public INPUTUNION inputUnion;
        }
        [DllImport("user32.dll")]
        private static extern uint SendInput(uint cInputs, INPUT[] pInputs, int cbSize);
        #endregion
        private static int x = MouseEvent.GetX();
        private static int y = MouseEvent.GetY();
        public static void EstimateVelocity(MouseMovementEventArgs m)
        {
            x += m.VelocityX;
            y += m.VelocityY;
            var (width, height) = DisplayEvent.GetScreenDimensions();
            if (x > width)
                x = width;
            else if (x < 0)
                x = 0;
            if (y > height)
                y = height;
            else if (y < 0)
                y = 0;
        }
        public static void SetCursor()
        {
            Point newPos = new(x, y);
            Cursor.Position = newPos;
        }
        public static void SetInitialCursor(Point p)
        {
            Cursor.Position = p;
            x = p.X;
            y = p.Y;
        }
        public static void HandleClick(uint type, short speed)
        {
            INPUT[] input = new INPUT[1];
            input[0].type = INPUT_MOUSE;
            input[0].inputUnion.mi.dwFlags = type;
            input[0].inputUnion.mi.mouseData = 0;
            switch (type)
            {
                case RI_MOUSE_LEFT_BUTTON_DOWN:
                    input[0].inputUnion.mi.dwFlags = MOUSEEVENTF_LEFTDOWN;
                    break;
                case RI_MOUSE_LEFT_BUTTON_UP:
                    input[0].inputUnion.mi.dwFlags = MOUSEEVENTF_LEFTUP;
                    break;
                case RI_MOUSE_RIGHT_BUTTON_DOWN:
                    input[0].inputUnion.mi.dwFlags = MOUSEEVENTF_RIGHTDOWN;
                    break;
                case RI_MOUSE_RIGHT_BUTTON_UP:
                    input[0].inputUnion.mi.dwFlags = MOUSEEVENTF_RIGHTUP;
                    break;
                case RI_MOUSE_MIDDLE_BUTTON_DOWN:
                    input[0].inputUnion.mi.dwFlags = MOUSEEVENTF_MIDDLEDOWN;
                    break;
                case RI_MOUSE_MIDDLE_BUTTON_UP:
                    input[0].inputUnion.mi.dwFlags = MOUSEEVENTF_MIDDLEUP;
                    break;
                case RI_MOUSE_BUTTON_4_DOWN:
                    input[0].inputUnion.mi.dwFlags = MOUSEEVENTF_XDOWN;
                    input[0].inputUnion.mi.mouseData = XBUTTON1;
                    break;
                case RI_MOUSE_BUTTON_4_UP:
                    input[0].inputUnion.mi.dwFlags = MOUSEEVENTF_XUP;
                    input[0].inputUnion.mi.mouseData = XBUTTON1;
                    break;
                case RI_MOUSE_BUTTON_5_DOWN:
                    input[0].inputUnion.mi.dwFlags = MOUSEEVENTF_XDOWN;
                    input[0].inputUnion.mi.mouseData = XBUTTON2;
                    break;
                case RI_MOUSE_BUTTON_5_UP:
                    input[0].inputUnion.mi.dwFlags = MOUSEEVENTF_XUP;
                    input[0].inputUnion.mi.mouseData = XBUTTON2;
                    break;
                case RI_MOUSE_WHEEL:
                    input[0].inputUnion.mi.dwFlags = MOUSEEVENTF_WHEEL;
                    input[0].inputUnion.mi.mouseData = (uint)speed;
                    break;
                case RI_MOUSE_HWHEEL:
                    input[0].inputUnion.mi.dwFlags = MOUSEEVENTF_HWHEEL;
                    input[0].inputUnion.mi.mouseData = (uint)speed;
                    break;
                default:
                    Console.WriteLine("Error: invalid button type");
                    return;
            }
            _ = SendInput((uint)input.Length, input, Marshal.SizeOf<INPUT>());
        }
        public static void HandleKey(ushort make, ushort flag)
        {
            INPUT[] input = new INPUT[1];
            input[0].type = INPUT_KEYBOARD;
            input[0].inputUnion.ki.wScan = make;
            if (flag == 1)
                input[0].inputUnion.ki.dwFlags = KEYEVENTF_SCANCODE | KEYEVENTF_KEYUP;
            else
                input[0].inputUnion.ki.dwFlags = KEYEVENTF_SCANCODE;
            _ = SendInput((uint)input.Length, input, Marshal.SizeOf<INPUT>());
        }
    }
}