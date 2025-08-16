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
        private volatile bool isTerminating = false;
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
            MonitorTermination();
        }
        private void MonitorTermination()
        {
            Task.Run(async () =>
            {
                while (!isTerminating)
                {
                    Terminate = await HaltSocket();
                    if (Terminate)
                    {
                        StopService();
                        break;
                    }
                    await Task.Delay(100);
                }
                
            });
        }
        private void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
        {
            isTerminating = true;
            mouseTracker?.Dispose();
            if (network != null)
                network.Disconnect();
            ExitThread();
        }
        private void OnMouseMovement(object? sender, MouseMovementEventArgs e)
        {
            if (isTerminating)
                return;
            network?.SendCoords(e);
            
            
            // Check if received the signal to stop the service for now
        }
        private async Task<bool> HaltSocket()
        {
            if (network == null || isTerminating)
                return false;
            return await network.ReceiveTermination();
        }
        private void StopService()
        {
            if (isTerminating)
                return;
            isTerminating = true;
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
            int margin = 1;
            DisplayEvent.margin = margin;
            DisplayEvent.edge = dir;
            var config = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();
            string ip = config["IP"] ?? throw new InvalidOperationException("Missing IP secret");
            while (true)
            {
                Thread.Sleep(1000);
                if (!DisplayEvent.OnScreen())
                {
                    Application.Run(new MouseTrackingContext(ip, dir, margin));
                    Thread.Sleep(500);
                }
                    
            }
                 
        }
        
    }
}