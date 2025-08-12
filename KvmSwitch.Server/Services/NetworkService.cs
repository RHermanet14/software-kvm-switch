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
                Console.WriteLine(jsonString);
                InitialMouseData? initial = JsonSerializer.Deserialize<InitialMouseData>(jsonString);
                if (initial != null)
                {
                    DisplayEvent.edge = initial.Value.Direction;
                    DisplayEvent.margin = initial.Value.Margin;
                    Console.WriteLine($"Initial data received: {DisplayEvent.edge}, {DisplayEvent.margin}");
                    _isConnected = true;
                    return;
                }
            }
            MouseMovementEventArgs? m = JsonSerializer.Deserialize<MouseMovementEventArgs>(jsonString);
            if (m != null)
            {
                MouseService.EstimateVelocity(m);
                MouseService.SetCursor();
                return;
            }
            Console.WriteLine($"Could not parse received data: {jsonString}");
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"JSON parsing error: {ex.Message}");
            Console.WriteLine($"Received data: {jsonString}");
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