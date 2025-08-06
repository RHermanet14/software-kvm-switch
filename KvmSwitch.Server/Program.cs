using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Shared;
using System.Text.Json;
using System.Threading.Tasks;

namespace Server {

    class Program
    {
        static void Main(string[] args)
        {
            var (width, height) = DisplayEvent.GetScreenDimensions();
            Console.WriteLine($"Screen dimensions: Width={width}, Height={height}");
            NetworkService network = new NetworkService();
        }
    }

}