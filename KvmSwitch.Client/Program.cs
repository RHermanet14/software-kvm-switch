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
        public MouseTrackingContext(string ip, Dir dir)
        {
            network = new NetworkService(ip);

            if (!network.Connect(!dir))
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
            e.Cancel = true; // prevent immediate termination
            mouseTracker?.Dispose();
            if (network != null)
                network.Disconnect();
            ExitThread();
        }
        private void OnMouseMovement(object? sender, MouseMovementEventArgs e)
        {
            //EstimateVelocity(e.VelocityX, e.VelocityY, e.TimeDelta);
            network?.SendCoords(e);
            //Console.WriteLine($"Estimated cursor position: X={x:F1}, Y={y:F1}");
            Console.WriteLine($"Actual cursor position: X={MouseEvent.GetX()}, Y={MouseEvent.GetY()}");
        }
    }

    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Dir dir = Direction.Left;
            var config = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();
            string ip = config["IP"] ?? throw new InvalidOperationException("Missing IP secret");
            var (width, height) = DisplayEvent.GetScreenDimensions();
            Console.WriteLine($"Screen dimensions: Width={width}, Height={height}");
            
            Console.WriteLine($"Socket connects the {dir} side of the client to the {!dir} side of the server");
            Application.Run(new MouseTrackingContext(ip, dir));      
            //ExecuteClient(ip);
        }
        
    }
}