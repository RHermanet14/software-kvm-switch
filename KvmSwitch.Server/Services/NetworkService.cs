using System.Drawing;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using services;
using System.Runtime.InteropServices;
using Shared;
using System.Windows.Forms;
using MessagePack;

public class NetworkService
{
    private Socket? _listener;
    private Socket? _currentClient;
    private DisplayEvent? _displayArgs;
    private readonly int Port;
    private const int BufferSize = 1024;
    private bool _isConnected = false;
    public bool HasActiveCoordClient => _currentClient?.Connected == true;
    public bool HasInitialConnection => _isConnected;
    public NetworkService(int port) { Port = port; }
    public void StartConnection()
    {
        IPAddress ip = IPAddress.Any; // Listen on all available network interfaces
        IPEndPoint localEndPoint = new IPEndPoint(ip, Port);
        _listener = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            _listener.Bind(localEndPoint);
            _listener.Listen(); // Max pending connections
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    public bool AcceptRequest()
    {
        if (_listener == null || _currentClient != null) return false;
        try
        {
            if (_listener.Poll(0, SelectMode.SelectRead))
            {
                _currentClient = _listener.Accept();
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error accepting coordinate client: {ex.Message}");
        }
        return false;
    }
    public async Task<bool> ReceiveCoords()
    {
        if (_currentClient == null || !_currentClient.Connected) return false;

        byte[] buffer = new byte[BufferSize];
        StringBuilder sb = new StringBuilder();

        try
        {
            if (!_currentClient.Poll(0, SelectMode.SelectRead)) return true;
            if (!_isConnected) // Initial data needs to be handled differently because clipboard can be very large
            {
                byte[] lengthBuffer = new byte[4];
                int bytesRead = 0;
                while (bytesRead < 4)
                {
                    int read = await _currentClient.ReceiveAsync(
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
                    int read = await _currentClient.ReceiveAsync(
                        new ArraySegment<byte>(compressedBuffer, bytesRead, compressedLength - bytesRead),
                        SocketFlags.None);
                    if (read == 0) return false;
                    bytesRead += read;
                }
                byte[] decompressedData = ClipboardHelper.Decompress(compressedBuffer);
                var data = MessagePackSerializer.Deserialize<InitialMouseData>(decompressedData);

                ProcessInitialData(data);
            }
            else // If its just coordinates
            {
                int bytesRead = await _currentClient.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
                if (bytesRead == 0)
                {
                    Console.WriteLine("Coordinate client disconnected");
                    CloseCurrentClient();
                    return false;
                }
                sb.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));

                while (_currentClient.Available > 0) // Continue reading if more data is available
                {
                    bytesRead = await _currentClient.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
                    sb.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
                }
                string jsonString = sb.ToString();
                if (!string.IsNullOrEmpty(jsonString))
                {
                    ProcessReceivedData(jsonString);
                }
            }
            
            return true;
        }
        catch (SocketException ex)
        {
            Console.WriteLine($"Socket error receiving coordinates: {ex.Message}");
            CloseCurrentClient();
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error receiving coordinates: {ex.Message}");
            return true; // Try again
        }
    }

    private void ProcessInitialData(InitialMouseData? initial)
    {
        if (_isConnected) return;
        if (initial != null)
        {
            _displayArgs = new(initial.Value.Direction, initial.Value.Margin);
            MouseService.SetInitialCursor(initial.Value.Shared.InitialCoords);

            var staThread = new Thread(() =>
            {
                try
                {
                    initial.Value.Shared.CurrentClipboard.SetClipboardContent();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error setting clipboard in server: {ex.Message}");
                }
            });
            
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();
            _isConnected = true;
            return;
        }
        
    }

    private void ProcessReceivedData(string jsonString)
    {
        try
        {
            if (!_isConnected)
            {
                Console.WriteLine("Initial data is still looking in here"); // Moved to ProcessInitialData
                return;
            }
            string jsonArray = "[" + jsonString.Replace("}{", "},{") + "]";
            var jsonObjects = JsonSerializer.Deserialize<JsonElement[]>(jsonArray);
            if (jsonObjects == null)
                return;
            foreach (var jsonObj in jsonObjects)
            {
                try
                {
                    if (jsonObj.TryGetProperty("ClickType", out _))
                    {
                        MouseMovementEventArgs? m = JsonSerializer.Deserialize<MouseMovementEventArgs>(jsonObj);
                        if (m != null)
                        {
                            if (m.ClickType == 0)
                            {
                                MouseService.EstimateVelocity(m);
                                MouseService.SetCursor();
                                if (_displayArgs != null && !_displayArgs.OnScreen()) // maybe move null check
                                {
                                    SendTermination();
                                    CloseCurrentClient();
                                }
                            }
                            else
                            {
                                MouseService.HandleClick(m.ClickType, m.ScrollSpeed);
                            }
                        }
                    }
                    else if (jsonObj.TryGetProperty("KeyInputType", out _))
                    {
                        KeyboardInputEventArgs? k = JsonSerializer.Deserialize<KeyboardInputEventArgs>(jsonObj);
                        if (k != null)
                        {
                            MouseService.HandleKey(k.Key, k.KeyInputType);
                        }
                    }

                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"Error parsing individual JSON object: {ex.Message}");
                    Console.WriteLine($"JSON object: {jsonObj}");
                }
            }
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"JSON parsing error: {ex.Message}");
            Console.WriteLine($"Received data: {jsonString}");
        }
    }

    public void SendTermination()
    {
        if (_currentClient == null || _displayArgs == null)
            return;
        try
        {
            Point p = _displayArgs.StartingPoint();
            SharedInitialData sid = new() { InitialCoords = p };

            var staThread = new Thread(() =>
            {
                try
                {
                    sid.CurrentClipboard.GetClipboardContent();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error retrieving clipboard: {ex.Message}");
                }
            });
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();

            //ClipboardHelper.AnalyzeSharedInitialDataSize(sid);    // Causes out of memory exception
            byte[] messageSent = MessagePackSerializer.Serialize(sid);
            byte[] compressedData = ClipboardHelper.Compress(messageSent);
            byte[] dataLength = BitConverter.GetBytes(compressedData.Length);
            _currentClient.Send(dataLength);
            _currentClient.Send(compressedData);

            byte[] ackLengthBuffer = new byte[4];
            int bytesReceived = _currentClient.Receive(ackLengthBuffer, SocketFlags.None); // Wait for ack

            if (bytesReceived == 4)
            {
                int ackLength = BitConverter.ToInt32(ackLengthBuffer, 0);
                byte[] ackBuffer = new byte[ackLength];
            
                int totalReceived = 0;
                while (totalReceived < ackLength)
                {
                    int received = _currentClient.Receive(ackBuffer, totalReceived, ackLength - totalReceived, SocketFlags.None);
                    if (received == 0) break;
                    totalReceived += received;
                }
            }
            else
            {
                Console.WriteLine("Failed to receive acknowledgment length");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in SendTermination: {ex.Message}");
        }
    }

    private void CloseCurrentClient()
    {
        if (_currentClient != null)
        {
            try
            {
                if (_currentClient.Connected)
                {
                    _currentClient.Shutdown(SocketShutdown.Both);
                }
                _currentClient.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error closing current client: {ex.Message}");
            }
            finally
            {
                _currentClient = null;
                _isConnected = false;
            }
        }
    }

    public void Disconnect()
    {
        CloseCurrentClient();
        if (_listener != null)
        {
            try
            {
                _listener.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error closing listener: {ex.Message}");
            }
            finally
            {
                _listener = null;
            }
        }
        Console.WriteLine("Network service disconnected");
    }
}