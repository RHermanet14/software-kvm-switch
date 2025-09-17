using MessagePack;
namespace Shared
{
    [MessagePackObject]
    public struct SerializablePoint
    {
        [Key(0)]
        public int X { get; set; }
        [Key(1)]
        public int Y { get; set; }
        public SerializablePoint() { X = -1;  Y = -1; }
        public SerializablePoint(int x, int y)
        {
            X = x;
            Y = y;
        }
        public static implicit operator Point(SerializablePoint sp) => new(sp.X, sp.Y);
        public static implicit operator SerializablePoint(Point p) => new(p.X, p.Y);
    }

    [MessagePackObject]
    public class SharedInitialData
    {
        // Used in Server -> Client
        [MessagePack.Key(0)]
        public SerializablePoint InitialCoords { get; set; }
        [MessagePack.Key(1)]
        public ClipboardEvent CurrentClipboard { get; set; } = new();
    }

    [MessagePackObject]
    public struct InitialMouseData
    {
        // Used in Client -> Server
        [MessagePack.Key(0)]
        public Direction Direction { get; set; } = Direction.None;
        [MessagePack.Key(1)]
        public int Margin { get; set; } = -1;
        [MessagePack.Key(2)]
        public SharedInitialData Shared { get; set; } = new();
        public InitialMouseData() { }
        public InitialMouseData(Direction d, int m, SerializablePoint p)
        {
            Direction = d;
            Margin = m;
            Shared = new()
            {
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