namespace Shared
{
    public struct InitialMouseData(Direction d, int m, Point p)
    {
        public Direction Direction { get; set; } = d;
        public int Margin { get; set; } = m;
        public Point InitialCoords { get; set; } = p;
    }
}