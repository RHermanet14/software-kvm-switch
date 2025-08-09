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
            //while(true) {
            while (DisplayEvent.OnScreen(1, 1))
            {
                // receive coordinates
                // Estimate Velocity
                // Set cursor position

            }
            // Send signal back to client
            //}
        }
        private static void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
        {
            l?.KeyboardInterrupt();
            //e.Cancel = true; // prevent immediate termination
        }
        
    }
    
}