namespace Shared
{
    public struct InitialMouseData(Direction d, int m)
    {
        public Direction Direction { get; set; } = d;
        public int Margin { get; set; } = m;
    }
}