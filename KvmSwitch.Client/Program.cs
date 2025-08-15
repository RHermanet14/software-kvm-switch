using System;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Shared;
using services;
using System.Windows.Forms;

namespace Client {
    public class MouseTrackingContext : ApplicationContext
    {
        private MouseService? mouseTracker;
        private NetworkService? network;
        public bool Terminate { get; set; } = false;
        public MouseTrackingContext(string ip, Dir dir, int margin)
        {
            network = new NetworkService(ip);

            if (!network.Connect(!dir, margin))
            {
                ExitThread();
                Environment.Exit(0);
            }
            Console.CancelKeyPress += OnCancelKeyPress; // Append custom function to keyboard interrupt
            mouseTracker = new MouseService();
            mouseTracker.MouseMovement += OnMouseMovement;
            if (!mouseTracker.StartTracking())
                ExitThread();
        }
        private void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
        {
            //e.Cancel = true; // prevent immediate termination
            mouseTracker?.Dispose();
            if (network != null)
                network.Disconnect();
            ExitThread();
        }
        private void OnMouseMovement(object? sender, MouseMovementEventArgs e)
        {
            network?.SendCoords(e);
            Task.Run(async () =>
            {
                Terminate = await HaltSocket();
            });
            if (Terminate)
                StopService();
            // Check if received the signal to stop the service for now
        }
        private async Task<bool> HaltSocket()
        {
            if (network == null)
                return false;
            return await network.ReceiveTermination();
        }
        private void StopService()
        {
            mouseTracker?.Dispose();
            if (network != null)
                network.Disconnect();
            ExitThread();
        }
    }

    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Dir dir = Direction.Left;
            int margin = 15;
            var config = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();
            string ip = config["IP"] ?? throw new InvalidOperationException("Missing IP secret");
            while (true)
            {
                Application.Run(new MouseTrackingContext(ip, dir, margin));
                Console.WriteLine("This message is after the application runs"); 
            }
                 
        }
        
    }
}