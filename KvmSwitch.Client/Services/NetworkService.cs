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
        private volatile bool isConnected = false;
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
                isConnected = true;
                return true;
            }
            catch
            {
                Console.WriteLine("Could not establish connection with server.");
                clientSocket?.Close();
                isConnected = false;
                return false;
            }
        }
        public void Disconnect()
        {
            isConnected = false;
            try
            {
                clientSocket?.Shutdown(SocketShutdown.Both);
            }
            catch
            {
                //nothing for now
            }
            
            clientSocket?.Close();
        }
        public void SendCoords(MouseMovementEventArgs e)
        {
            if (!isConnected || clientSocket == null || !clientSocket.Connected)
                return;
            try
            {
                byte[] messageSent = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(e));
                int byteSent = clientSocket.Send(messageSent);
            }
            catch (ArgumentNullException ane)
            {
                Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                isConnected = false;
            }
            catch (SocketException se)
            {
                if (isConnected)
                    Console.WriteLine("SocketException : {0}", se.ToString());
                isConnected = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected exception : {0}", ex.ToString());
                isConnected = false;
            }
        }
        public async Task<bool> ReceiveTermination()
        {
            if (clientSocket == null || !clientSocket.Connected || !isConnected) return false;
            byte[] buffer = new byte[1024];
            try
            {
                int bytesRead = await clientSocket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
                if (bytesRead == 0)
                    return false;
                Console.WriteLine("Termination signal received. Stopping service...");
                return true;
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Socket error: {ex.Message}");
                Disconnect();
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }       
        }
    }
    
}