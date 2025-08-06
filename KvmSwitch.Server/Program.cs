using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Shared;
using System.Text.Json;

namespace Server {

    class Program {
        static void Main(string[] args)
        {
            ExecuteServer();
        }

        public static void ExecuteServer()
        {
            var (width, height) = DisplayEvent.GetScreenDimensions();
            Console.WriteLine($"Screen dimensions: Width={width}, Height={height}");
            IPAddress ipAddr = IPAddress.Any;
            IPEndPoint localEndPoint = new IPEndPoint(ipAddr, 11111);
            Socket listener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try {
                listener.Bind(localEndPoint);
                listener.Listen(10);
                    Console.WriteLine("Waiting connection ... ");
                    Socket clientSocket = listener.Accept();
                    byte[] bytes = new byte[1024];
                    Direction dir;
                    dir = JsonSerializer.Deserialize<Direction>(clientSocket.Receive(bytes));

                    Console.WriteLine("Text received -> {0} ", dir);
                    //byte[] message = Encoding.ASCII.GetBytes("Test Server");
                    //clientSocket.Send(message);
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }
    }
}