using System.Windows.Forms;
using System.Drawing;
using Shared;
namespace services
{
    public static class MouseService
    {
        #region mouse click types
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
            Point screenPos = Cursor.Position;
            Point newPos = new(x, y);
            Cursor.Position = newPos;
        }
        public static void HandleClick(uint type, short speed)
        {
            switch (type)
            {
                case RI_MOUSE_LEFT_BUTTON_DOWN:
                    Console.WriteLine("Left mouse button clicked");
                    break;
                case RI_MOUSE_LEFT_BUTTON_UP:
                    Console.WriteLine("Left mouse button released");
                    break;
                case RI_MOUSE_RIGHT_BUTTON_DOWN:
                    Console.WriteLine("right mouse button clicked");
                    break;
                case RI_MOUSE_RIGHT_BUTTON_UP:
                    Console.WriteLine("right mouse button released");
                    break;
                case RI_MOUSE_MIDDLE_BUTTON_DOWN:
                    Console.WriteLine("middle mouse button clicked");
                    break;
                case RI_MOUSE_MIDDLE_BUTTON_UP:
                    Console.WriteLine("middle mouse button released");
                    break;
                case RI_MOUSE_BUTTON_4_DOWN:
                    Console.WriteLine("mouse4 button clicked");
                    break;
                case RI_MOUSE_BUTTON_4_UP:
                    Console.WriteLine("mouse4 button released");
                    break;
                case RI_MOUSE_BUTTON_5_DOWN:
                    Console.WriteLine("mouse5 button clicked");
                    break;
                case RI_MOUSE_BUTTON_5_UP:
                    Console.WriteLine("mouse5 button released");
                    break;
                case RI_MOUSE_WHEEL:
                    if (speed < 0)
                        Console.WriteLine("Scroll down");
                    else
                        Console.WriteLine("Scroll up");
                    break;
                case RI_MOUSE_HWHEEL:
                    if (speed < 0)
                        Console.WriteLine("Scroll left");
                    else
                        Console.WriteLine("Scroll right");
                    break;   
            }
        }
    }
}