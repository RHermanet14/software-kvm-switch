using System.Windows.Forms;
using System.Drawing;
using Shared;
namespace services
{
    public static class MouseService
    {
        private static int x = MouseEvent.GetX();
        private static int y = MouseEvent.GetY();
        public static void EstimateVelocity(MouseMovementEventArgs m)
        {
            x += (int)(m.VelocityX * m.TimeDelta);
            y += (int)(m.VelocityY * m.TimeDelta);
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
            Console.WriteLine(screenPos);
        }
    }
}