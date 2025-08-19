using System.Drawing;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using services;
using Shared;

public class NetworkService
{
    private Socket? _listener;
    private Socket? _currentClient;
    private const int Port = 11111;
    private const int BufferSize = 1024;
    private bool _isConnected = false;
    public bool HasActiveCoordClient => _currentClient?.Connected == true;
    public bool HasInitialConnection => _isConnected;

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
                    DisplayEvent.edge = initial.Value.Direction;
                    DisplayEvent.margin = initial.Value.Margin;
                    MouseService.SetInitialCursor(initial.Value.InitialCoords);
                    //Console.WriteLine($"Initial data received: {DisplayEvent.edge}, {DisplayEvent.margin}");
                    _isConnected = true;
                    return;
                }
            }

            var jsonObjects = SplitJsonObjects(jsonString);
            foreach (string jsonObj in jsonObjects)
            {
                if (string.IsNullOrWhiteSpace(jsonObj)) continue;
                try
                {
                    MouseMovementEventArgs? m = JsonSerializer.Deserialize<MouseMovementEventArgs>(jsonObj);
                    if (m != null)
                    {
                        if (m.ClickType == 0)
                        {
                            MouseService.EstimateVelocity(m);
                            MouseService.SetCursor();
                            if (!DisplayEvent.OnScreen())
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

    private List<string> SplitJsonObjects(string concatenatedJson)
    {
        var result = new List<string>();
        int startPos = 0;
        
        while (startPos < concatenatedJson.Length)
        {
            // Find the start of next JSON object
            int openBrace = concatenatedJson.IndexOf('{', startPos);
            if (openBrace == -1) break;
            
            // Find the matching closing brace
            int braceCount = 0;
            int closeBrace = -1;
            
            for (int i = openBrace; i < concatenatedJson.Length; i++)
            {
                if (concatenatedJson[i] == '{')
                    braceCount++;
                else if (concatenatedJson[i] == '}')
                {
                    braceCount--;
                    if (braceCount == 0)
                    {
                        closeBrace = i;
                        break;
                    }
                }
            }
            
            if (closeBrace != -1)
            {
                string jsonObj = concatenatedJson.Substring(openBrace, closeBrace - openBrace + 1);
                result.Add(jsonObj);
                startPos = closeBrace + 1;
            }
            else
                break;
        }
        
        return result;
    }

    public void SendTermination()
    {
        if (_currentClient == null)
            return;
        try
        {
            Point p = DisplayEvent.StartingPoint();
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
}