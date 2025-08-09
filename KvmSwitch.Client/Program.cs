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
        public MouseTrackingContext(string ip, Dir dir, int margin)
        {
            /*
            network = new NetworkService(ip);

            if (!network.Connect(!dir, margin))
            {
                ExitThread();
                Environment.Exit(0);
            }
            */
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
            //EstimateVelocity(e.VelocityX, e.VelocityY, e.TimeDelta);
            //network?.SendCoords(e);
            //Console.WriteLine($"Estimated cursor position: X={x:F1}, Y={y:F1}");

            if (MouseEvent.GetX() > 1900)
            {
                StopService();
            }
            else
            {
                Console.WriteLine($"Actual cursor position: X={MouseEvent.GetX()}, Y={MouseEvent.GetY()}");
            }
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