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
        public static bool OnScreen(int cursorX, int cursorY)
        {
            switch (edge)
            {
                case Direction.Up:
                    if (cursorY < 0)
                        return false;
                    return true;
                case Direction.Down:
                    if (cursorY > height)
                        return false;
                    return true;
                case Direction.Left:
                    if (cursorX < 0)
                        return false;
                    return true;
                case Direction.Right:
                    if (cursorX > width)
                        return false;
                    return true;
                default:
                    return false;
            }
        }
    }
}