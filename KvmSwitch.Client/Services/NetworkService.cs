using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Shared;
namespace services
{
    public class NetworkService
    {
        private Socket? clientSocket;
        private readonly string serverIP;
        public NetworkService(string ip)
        {
            serverIP = ip;
        }

        public bool Connect(Direction direction, int margin)
        {
            try
            {
                IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddr = IPAddress.Parse(serverIP);
                IPEndPoint remoteEndPoint = new IPEndPoint(ipAddr, 11111);
                clientSocket = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                clientSocket.Connect(remoteEndPoint);
                var m = new InitialMouseData(direction, margin);
                byte[] messageSent = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(m));
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
        public void SendCoords(MouseMovementEventArgs e)
        {
            try
            {
                if (clientSocket != null)
                {
                    byte[] messageSent = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(e));
                    int byteSent = clientSocket.Send(messageSent);
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
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected exception : {0}", ex.ToString());
            }
        }
    }
    
}