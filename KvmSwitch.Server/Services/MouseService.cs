using Shared;
namespace services
{
    public static class MouseService
    {
        private static int x = MouseEvent.GetX();
        private static int y = MouseEvent.GetY();
        public static void EstimateVelocity(float dx, float dy, double dt)
        {
            x += (int)(dx * dt);
            y += (int)(dy * dt);
            if (DisplayEvent.OnScreen(x, y))
            {
                var (width, height) = DisplayEvent.GetScreenDimensions();
                if (x > width)
                    x = width;
                if (x < 0)
                    x = 0;
                if (y > height)
                    y = height;
                if (y < 0)
                    y = 0;
            }
        }
    }
}