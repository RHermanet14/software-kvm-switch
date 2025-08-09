namespace Shared
{
    public struct InitialMouseData
    {
        public Direction direction { get; set; }
        public int margin { get; set; }
        public InitialMouseData(Direction d, int m)
        {
            direction = d;
            margin = m;
        }
    }
}