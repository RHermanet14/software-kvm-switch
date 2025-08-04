using System;
using System.Runtime.InteropServices;
namespace Shared
{
    public enum Direction {Up, Down, Left, Right}
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
    public class DisplayEvent
    {


        private static int margin { get; set; } = 10;
        public static (int width, int height) GetScreenDimensions()
        {
            Screen? primaryScreen = Screen.PrimaryScreen;
            if (primaryScreen == null)
                return (0, 0);
            Rectangle screenBounds = primaryScreen.Bounds;
            return (screenBounds.Width, screenBounds.Height);
        }

    }
}