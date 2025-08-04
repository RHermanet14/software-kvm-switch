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
        public MouseTrackingContext()
        {
            Console.CancelKeyPress += OnCancelKeyPress;
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
            Console.WriteLine($"Velocity: X={e.VelocityX:F1} px/s, Y={e.VelocityY:F1} px/s");
            Console.WriteLine($"Time = {e.TimeDelta}");
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
            var (x, y) = MouseEvent.GetCursorPosition();
            Console.WriteLine($"Cursor is at: X={x}, Y={y}");
            Console.WriteLine($"Socket connects the {dir} side of the client to the {!dir} side of the server");
            Application.Run(new MouseTrackingContext());      
            //ExecuteClient(ip);
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
}