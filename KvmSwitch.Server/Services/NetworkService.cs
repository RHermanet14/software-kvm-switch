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

    private void ProcessReceivedData(string jsonString)
    {
        try
        {
            if (!_isConnected)
            {
                InitialMouseData? initial = JsonSerializer.Deserialize<InitialMouseData>(jsonString);
                if (initial != null)
                {
                    _displayArgs = new(initial.Value.Direction, initial.Value.Margin);

                    MouseService.SetInitialCursor(initial.Value.InitialCoords);
                    _isConnected = true;
                    return;
                }
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
            byte[] messageSent = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(p));
            int byteSent = _currentClient.Send(messageSent);
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

    private SharedInitialData GetClipboardContent()
    {
        var clipboardData = new SharedInitialData();
        try
        {
            var ClipboardObject = Clipboard.GetDataObject();
            if (ClipboardObject == null) return clipboardData;
            string[] formats = ClipboardObject.GetFormats();
            foreach (string format in formats)
            {
                try
                {
                    var data = ClipboardObject.GetData(format);
                    if (data == null) continue;
                    if (data is string textData)
                    {
                        if (!string.IsNullOrEmpty(textData))
                        {
                            
                        }
                    }
                    else if (data is Image image)
                    {

                    }
                    else if (data is byte[] binaryData)
                    {

                    }
                    else if (data is MemoryStream stream)
                    {

                    }
                    else if (data is string[] stringArray)
                    {

                    }
                    else if (data is System.Collections.Specialized.StringCollection stringCollection)
                    {

                    }
                    else if (data is IEnumerable<string> stringEnumerable)
                    {

                    }
                    else
                    {
                        string? fallbackText = data.ToString();
                        if (!string.IsNullOrEmpty(fallbackText) && fallbackText != data.GetType().ToString())
                        {
                            
                        }
                    }
                }
                catch (Exception ex)
                {
                    clipboardData.DataType = "error";
                    clipboardData.TextData = ex.Message;
                }
            }

        }
        catch (Exception ex)
        {
            clipboardData.DataType = "error";
            clipboardData.TextData = ex.Message;
        }
        return clipboardData;
    }
}