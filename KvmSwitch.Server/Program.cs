using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Shared;
using System.Text.Json;
using System.Threading.Tasks;
using services;

namespace Server {

    class Program
    {
        static async Task Main(string[] args)
        {
            var (width, height) = DisplayEvent.GetScreenDimensions();
            Console.WriteLine($"Screen dimensions: Width={width}, Height={height}");
            //MouseService m = new MouseService();
            Console.WriteLine($"Initial value of edge: {DisplayEvent.edge}");
            NetworkService network = new NetworkService();
            await network.StartListening();
            Console.WriteLine($"The server direction is: {DisplayEvent.edge}");
        }
    }

}