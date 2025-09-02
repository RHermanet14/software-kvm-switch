namespace Shared
{
    public struct InitialMouseData(Direction d, int m, Point p)
    {
        public Direction Direction { get; set; } = d;
        public int Margin { get; set; } = m;
        public Point InitialCoords { get; set; } = p;
    }
    public class MouseMovementEventArgs : EventArgs
    {
        public uint ClickType { get; set; }
        public short ScrollSpeed { get; set; }
        public int VelocityX { get; set; }
        public int VelocityY { get; set; }
    }
    public class KeyboardInputEventArgs : EventArgs
    {
        public ushort Key { get; set; }
        public ushort KeyInputType { get; set; }
    }
    public class ConnectInfo
    {
        public string IP { get; set; } = "";
        public int Port { get; set; }
        public DisplayEvent Display { get; set; } = new(Direction.None, -1);
        public ConnectInfo() { }
        public ConnectInfo(string ip, int port, Direction dir, int border)
        {
            IP = ip;
            Port = port;
            Display = new(dir, border);
        }
    }
}