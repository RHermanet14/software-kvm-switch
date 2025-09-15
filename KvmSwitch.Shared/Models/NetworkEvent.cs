namespace Shared
{
    public class SharedInitialData
    {
        // Used in Server -> Client
        public Point InitialCoords { get; set; }
        public ClipboardEvent CurrentClipboard { get; set; } = new();
    }
    public struct InitialMouseData
    {
        // Used in Client -> Server
        public Direction Direction { get; set; } = Direction.None;
        public int Margin { get; set; } = -1;
        public SharedInitialData Shared { get; set; } = new();
        public InitialMouseData() { }
        public InitialMouseData(Direction d, int m, Point p)
        {
            Direction = d;
            Margin = m;
            Shared = new(){
                InitialCoords = p
            };
        }
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
        public int Port { get; set; } = -1;
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