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
        private MouseSuppressionService? mouseSuppressor;
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
            mouseSuppressor = new MouseSuppressionService();
            if (!mouseTracker.StartTracking())
            {
                ExitThread();
                return;
            }
            mouseSuppressor.StartSuppression();    
            
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
            StopService();
        }
        private void OnMouseMovement(object? sender, MouseMovementEventArgs e)
        {
            if (isTerminating)
                return;
            network?.SendCoords(e);
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
            mouseSuppressor?.StopSuppression();

            mouseTracker?.Dispose();
            mouseSuppressor?.Dispose();

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
            DisplayEvent.margin = 1;
            DisplayEvent.edge = Direction.Left;
            DisplayEvent.GetScreenDimensions();
            var config = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();
            string ip = config["IP"] ?? throw new InvalidOperationException("Missing IP secret");
            while (true)
            {

                if (!DisplayEvent.OnScreen())
                {
                    Application.Run(new MouseTrackingContext(ip, DisplayEvent.edge, DisplayEvent.margin));
                    Thread.Sleep(500);
                }
                else
                {
                    Thread.Sleep(50);
                }
                    
            }
                 
        }
        
    }
}