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
        public bool HasInitialConnection => network?.HasInitialConnection == true;
        public bool HasActiveClient => network?.HasActiveCoordClient == true;
        public Listening(int port)
        {
            network = new NetworkService(port);
            network.StartConnection();
        }
        public async Task<bool> RunSocket()
        {
            if (network == null) return false;
            if (!network.HasActiveCoordClient) network.AcceptRequest();
            if (network.HasActiveCoordClient) return await network.ReceiveCoords();
            return true;
        }
        public void KeyboardInterrupt()
        {
            Console.WriteLine("Terminating Server");
            network?.SendTermination();
            network?.Disconnect();
        }
    }
    public class Program
    {
        private static Listening? l;
        private static volatile bool _isRunning = true;
        static async Task Main(string[] args)
        {
            int port = 11111;
            if (args.Length > 0)
                _ = int.TryParse(args[0], out port);
            bool keepRunning;
            Console.CancelKeyPress += OnCancelKeyPress;
            var (width, height) = DisplayEvent.GetScreenDimensions();
            l = new Listening(port);
            var inputThread = new Thread(() => MonitorTermination().Wait())
            {
                IsBackground = false,
                Name = "ConsoleMonitor"
            };
            inputThread.Start();

            while (_isRunning)
            {
                try
                {
                    keepRunning = await l.RunSocket();
                    if (!keepRunning)
                    {
                        Console.WriteLine("Network service stopped");
                        break;
                    }
                    await Task.Delay(1);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in main loop: {ex.Message}");
                    await Task.Delay(100);
                }
            }
            Console.WriteLine("Program exiting...");
        }
        private static void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            _isRunning = false;
            try
            {
                l?.KeyboardInterrupt();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during cleanup: {ex.Message}");
            }

            Task.Delay(2000).ContinueWith(_ =>
            {
                Console.WriteLine("Forcing program to exit...");
                Environment.Exit(0);
            });
        }

        private static async Task MonitorTermination()
        {
            while (_isRunning)
            {
                try
                {
                    string? input = Console.ReadLine();
                    if (input?.Trim().ToLower() == "exit")
                    {
                        Console.WriteLine("typed exit");
                        _isRunning = false;
                        StopServer();
                        break;
                    }
                    await Task.Delay(100);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error while trying to get input from stdin: {ex.Message}");
                }
            }
        }
        public static void StopServer()
        {
            try
            {               
                l?.KeyboardInterrupt();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during cleanup: {ex.Message}");
            }
        }
        
    }
    
}