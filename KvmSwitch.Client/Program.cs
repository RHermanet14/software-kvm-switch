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
        private SuppressionService? suppressor;
        private NetworkService? network;
        private volatile bool isTerminating = false;
        public bool Terminate { get; set; } = false;
        public MouseTrackingContext(string ip, int port, DisplayEvent display)
        {
            network = new NetworkService(ip);

            if (!network.Connect(port, display))
            {
                ExitThread();
                Environment.Exit(0);
            }
            Console.CancelKeyPress += OnCancelKeyPress; // Append custom function to keyboard interrupt
            mouseTracker = new MouseService();
            mouseTracker.MouseMovement += OnMouseMovement;
            suppressor = new SuppressionService();
            SuppressionService.KeyboardInput += OnKeyboardInput;
            if (!mouseTracker.StartTracking())
            {
                ExitThread();
                return;
            }
            suppressor.StartSuppression();
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

        private void OnKeyboardInput(object? sender, KeyboardInputEventArgs k)
        {
            if (isTerminating)
                return;
            network?.SendKeys(k);
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
            suppressor?.StopSuppression();

            mouseTracker?.Dispose();
            suppressor?.Dispose();

            network?.Disconnect();
            ExitThread();
        }
    }

    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            DisplayEvent display;
            string ip;
            int port;
            if (args.Length > 0 && args.Length % 4 == 0)
            {
                ip = args[0];
                _ = int.TryParse(args[1], out port);
                _ = int.TryParse(args[2], out int edge);
                _ = int.TryParse(args[3], out int margin);
                display = new((Direction)edge, margin);
            }
            else
            {
                var config = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .Build();
                ip = config["IP"] ?? throw new InvalidOperationException("Missing IP secret");
                port = 11111;
                display = new(Direction.Left, 1);
            }
            
            DisplayEvent.GetScreenDimensions();

            Console.WriteLine($"{ip}\n{port}\n{display.edge}\n{display.margin}\n");

            while (true)
            {
                if (!display.OnScreen())
                {
                    Application.Run(new MouseTrackingContext(ip, port, display));
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