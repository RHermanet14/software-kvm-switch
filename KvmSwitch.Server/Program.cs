using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Shared;
using System.Text.Json;
using System.Threading.Tasks;
using services;
using Microsoft.VisualBasic.Devices;

namespace Server
{
    public class Listening
    {
        private NetworkService? network;
        public Listening()
        {
            network = new NetworkService();
            network.StartConnection();
        }
        public void RunSocket()
        {
            Task.Run(async () =>
            {
                if (network != null)
                    await network.ReceiveCoords();
            });
        }
        public void KeyboardInterrupt()
        {
            Console.WriteLine("Terminating Server");
            if (network != null)
                network.Disconnect();
        }
    }
    class Program
    {
        private static Listening? l;
        static void Main(string[] args)
        {
            Console.CancelKeyPress += OnCancelKeyPress;
            var (width, height) = DisplayEvent.GetScreenDimensions();
            Console.WriteLine($"Screen dimensions: Width={width}, Height={height}");
            Console.WriteLine($"Initial value of edge: {DisplayEvent.edge}");
            l = new Listening();
            // Handle keyboard interrupt and properly close socket when needed
            while (DisplayEvent.OnScreen())
            {
                l.RunSocket();
                MouseService.SetCursor();
            }
        }
        private static void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
        {
            l?.KeyboardInterrupt();
            //e.Cancel = true; // prevent immediate termination
        }
        
    }
    
}