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
        private static float x = MouseEvent.GetX();
        private static float y = MouseEvent.GetY();
        private MouseService? mouseTracker;
        public MouseTrackingContext()
        {
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
            ExitThread();
        }
        private void OnMouseMovement(object? sender, MouseMovementEventArgs e)
        {
            (x, y) = EstimateVelocity(x, y, e.VelocityX, e.VelocityY, e.TimeDelta);
            Console.WriteLine($"Estimated cursor position: X={x:F1}, Y={y:F1}");
            Console.WriteLine($"Actual cursor position: X={MouseEvent.GetX()}, Y={MouseEvent.GetY()}");
        }
        private (float, float) EstimateVelocity(float x, float y, float dx, float dy, double dt)
        {
            x += dx * (float)dt;
            y += dy * (float)dt;
            /*
            var (width, height) = DisplayEvent.GetScreenDimensions();
            if (x > width)
                x = width;
            if (x < 0)
                x = 0;
            if (y > height)
                y = height;
            if (y < 0)
                y = 0;
            */
            return (x, y);
        }
        static void ExecuteClient(string ip)
        {
            try
            {
                IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddr = IPAddress.Parse(ip);
                IPEndPoint remoteEndPoint = new IPEndPoint(ipAddr, 11111);
                Socket sender = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    sender.Connect(remoteEndPoint);
                    Console.WriteLine("Socket connected to -> {0} ", sender.RemoteEndPoint?.ToString() ?? "Unknown");
                    byte[] messageSent = Encoding.ASCII.GetBytes("Test Client<EOF>");
                    int byteSent = sender.Send(messageSent);
                    byte[] messageReceived = new byte[1024];
                    int byteRecv = sender.Receive(messageReceived);
                    Console.WriteLine("Message from Server -> {0}", Encoding.ASCII.GetString(messageReceived, 0, byteRecv));
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();
                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
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
            Application.Run(new MouseTrackingContext());      
            //ExecuteClient(ip);
        }
        
    }
}