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
        public MouseTrackingContext(ConnectInfo c)
        {
            network = new NetworkService(c.IP);

            if (!network.Connect(c.Port, c.Display))
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
            string ip;
            DisplayEvent.GetScreenDimensions();
            if (args.Length > 0 && args.Length % 4 == 0)
            {
                ConnectInfo[] c = new ConnectInfo[4];
                int count = 0;
                for (int i = 0; i < args.Length / 4; i++)
                {
                    ip = args[i * 4];
                    _ = int.TryParse(args[(i * 4) + 1], out int port);
                    _ = int.TryParse(args[(i * 4) + 2], out int edge);
                    _ = int.TryParse(args[(i * 4) + 3], out int margin);
                    c[i] = new ConnectInfo(ip, port, (Direction)edge, margin);
                    count++;
                }

                while (true)
                {
                    for (int i = 0; i < count; i++)
                    {
                        if (!c[i].Display.OnScreen())
                        {
                            Application.Run(new MouseTrackingContext(c[i]));
                            Thread.Sleep(450);
                        }
                    }
                    Thread.Sleep(50);
                }   
            }
            else
            {
                var config = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .Build();
                ip = config["IP"] ?? throw new InvalidOperationException("Missing IP secret");
                ConnectInfo c = new(ip, 11111, Direction.Left, 1);
                while (true)
                {
                    if (!c.Display.OnScreen())
                    {
                        Application.Run(new MouseTrackingContext(c));
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
}