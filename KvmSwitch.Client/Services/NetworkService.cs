using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
namespace Shared
{
    public class NetworkService
    {
        private Socket? clientSocket;
        private readonly string serverIP;
        public NetworkService(string ip)
        {
            serverIP = ip;
        }

        private static byte[] Serialize(MouseData s)
        {
            return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(s));
        }

        public bool Connect(Dir direction)
        {
            try
            {
                IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddr = IPAddress.Parse(serverIP);
                IPEndPoint remoteEndPoint = new IPEndPoint(ipAddr, 11111);
                clientSocket = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                clientSocket.Connect(remoteEndPoint);
                Console.WriteLine("Socket connected to -> {0} ", clientSocket.RemoteEndPoint?.ToString() ?? "Unknown");
                Console.WriteLine($"{JsonSerializer.Serialize(direction)}");
                byte[] messageSent = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(direction));
                int byteSent = clientSocket.Send(messageSent);
                return true;
            }
            catch
            {
                Console.WriteLine("Could not establish connection with server.");
                clientSocket?.Close();
                return false;
            }
        }
        public void Disconnect()
        {
            clientSocket?.Shutdown(SocketShutdown.Both);
            clientSocket?.Close();
        }
        private void SendCoords(float x, float y)
        {
            try
            {
                byte[] messageReceived = new byte[1024];
                int byteRecv;
                if (clientSocket != null)
                {
                    byteRecv = clientSocket.Receive(messageReceived);
                    Console.WriteLine("Message from Server -> {0}", Encoding.ASCII.GetString(messageReceived, 0, byteRecv));
                }
                else
                {
                    throw new SocketException();
                }
                    
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
    }
    
}