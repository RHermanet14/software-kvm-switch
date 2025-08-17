using System;
using System.Runtime.InteropServices;
namespace Shared
{
    public enum Direction {None = -1, Up, Down, Left, Right}
    public struct Dir
    {
        public Direction Side { get; }
        public Dir(Direction side) { Side = side; }
        public static Dir operator !(Dir d)
        {
            return d.Side switch
            {
                Direction.Up => new Dir(Direction.Down),
                Direction.Down => new Dir(Direction.Up),
                Direction.Left => new Dir(Direction.Right),
                Direction.Right => new Dir(Direction.Left),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        public override string ToString() => Side.ToString();
        public static implicit operator Dir(Direction d) => new Dir(d);
        public static implicit operator Direction(Dir d) => d.Side;
    }
    public static class DisplayEvent
    {
        public static int margin { get; set; } = 10;
        private static int width { get; set; } = -1;
        private static int height { get; set; } = -1;
        public static Direction edge { get; set; } = Direction.None;
        public static (int width, int height) GetScreenDimensions()
        {
            if (width != -1 && height != -1)
                return (width, height);
            Screen? primaryScreen = Screen.PrimaryScreen;
            if (primaryScreen == null)
                return (0, 0);
            Rectangle screenBounds = primaryScreen.Bounds;
            width = screenBounds.Width; height = screenBounds.Height;
            return (screenBounds.Width, screenBounds.Height);
        }
        public static bool OnScreen()
        {
            switch (edge)
            {
                case Direction.Up:
                    if (MouseEvent.GetY() <= margin)
                        return false;
                    return true;
                case Direction.Down:
                    if (MouseEvent.GetY() >= height - margin)
                        return false;
                    return true;
                case Direction.Left:
                    if (MouseEvent.GetX() <= margin)
                        return false;
                    return true;
                case Direction.Right:
                    if (MouseEvent.GetX() >= width - margin)
                        return false;
                    return true;
                default:
                    return false;
            }
        }
        public static bool OnScreen(int cursorX, int cursorY)
        {
            switch (edge)
            {
                case Direction.Up:
                    if (cursorY <= margin)
                        return false;
                    return true;
                case Direction.Down:
                    if (cursorY >= height - margin)
                        return false;
                    return true;
                case Direction.Left:
                    if (cursorX <= margin)
                        return false;
                    return true;
                case Direction.Right:
                    if (cursorX >= width - margin)
                        return false;
                    return true;
                default:
                    return false;
            }
        }
        public static Point StartingPoint() // Gets the starting point of the other screen, assuming its edge is opposite of the current screen
        {
            switch (edge)
            {
                case Direction.Up:
                    return new(MouseEvent.GetX(), height - margin);
                case Direction.Down:
                    return new(MouseEvent.GetX(), margin);
                case Direction.Left:
                    return new(width - margin, MouseEvent.GetY()); // opposite x, same y
                case Direction.Right:
                    return new(margin, MouseEvent.GetY());
                default:
                    return new(0, 0);
            }
        }
    }
}