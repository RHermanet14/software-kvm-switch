using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Shared;
using MessagePack;
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

        public bool Connect(int port, DisplayEvent d)
        {
            try
            {
                IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddr = IPAddress.Parse(serverIP);
                IPEndPoint remoteEndPoint = new(ipAddr, port);
                clientSocket = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                clientSocket.Connect(remoteEndPoint);
                var m = new InitialMouseData(!(Dir)d.edge, d.margin, d.StartingPoint());
                m.Shared.CurrentClipboard.GetClipboardContent();    // Populate CurrentClipboard and optimize

                ClipboardHelper.AnalyzeMessagePackSize(m); // Debugging

                byte[] messageSent = MessagePackSerializer.Serialize(m);
                byte[] compressedData = ClipboardHelper.Compress(messageSent);
                byte[] dataLength = BitConverter.GetBytes(compressedData.Length);
                clientSocket.Send(dataLength);
                clientSocket.Send(compressedData);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
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
            if (!IsConnectionHealthy() || clientSocket == null)
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
                Disconnect();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected exception : {0}", ex.ToString());
                isConnected = false;
            }
        }
        private bool IsConnectionHealthy()
        {
            if (!isConnected || clientSocket == null)
                return false;
            try
            {
                if (clientSocket.Poll(0, SelectMode.SelectError))
                    return false;
            
                if (clientSocket.Poll(0, SelectMode.SelectRead))
                {
                    if (clientSocket.Available == 0)
                    {
                        return false;
                    }
                }
                return clientSocket.Connected;
            }
            catch
            {
                return false;
            }
        }
        public void SendKeys(KeyboardInputEventArgs k)
        {
            if (!isConnected || clientSocket == null || !clientSocket.Connected)
                return;
            try
            {
                byte[] messageSent = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(k));
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
            StringBuilder sb = new();
            try
            {
                byte[] lengthBuffer = new byte[4];
                int bytesRead = 0;
                while (bytesRead < 4)
                {
                    int read = await clientSocket.ReceiveAsync(
                        new ArraySegment<byte>(
                            lengthBuffer, bytesRead, 4 - bytesRead
                        ), SocketFlags.None
                    );
                    if (read == 0) return false;
                    bytesRead += read;
                }
                int compressedLength = BitConverter.ToInt32(lengthBuffer, 0);

                byte[] compressedBuffer = new byte[compressedLength];
                bytesRead = 0;
                while (bytesRead < compressedLength)
                {
                    int read = await clientSocket.ReceiveAsync(
                        new ArraySegment<byte>(compressedBuffer, bytesRead, compressedLength - bytesRead),
                        SocketFlags.None);
                    if (read == 0) return false;
                    bytesRead += read;
                }
                byte[] decompressedData = ClipboardHelper.Decompress(compressedBuffer);
                var data = MessagePackSerializer.Deserialize<SharedInitialData>(decompressedData);
                if (data != null)
                {
                    DisplayEvent.SetCursor(data.InitialCoords);
                    var staThread = new Thread(() =>
                    {
                        data.CurrentClipboard.SetClipboardContent();
                    });
                    staThread.SetApartmentState(ApartmentState.STA);
                    staThread.Start();
                    staThread.Join();
                }



                /*
                int bytesRead = await clientSocket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
                if (bytesRead == 0)
                    return false;
                sb.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
                string jsonString = sb.ToString();
                if (string.IsNullOrEmpty(jsonString))
                    return false;
                var p = JsonSerializer.Deserialize<SharedInitialData>(jsonString); // Changed to SharedInitialData
                if (p != null)
                {
                    DisplayEvent.SetCursor(p.InitialCoords);
                    var staThread = new Thread(() =>
                    {
                        p.CurrentClipboard.SetClipboardContent();
                    });
                    staThread.SetApartmentState(ApartmentState.STA);
                    staThread.Start();
                    staThread.Join();
                }
                */
                    
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