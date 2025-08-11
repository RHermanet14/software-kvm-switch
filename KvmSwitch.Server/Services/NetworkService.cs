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
            Socket handler = _listener.Accept();    // Does the server socket close when handler is closed?
            Task.Run(async () =>
            {
                await HandleInitialConnection(handler);
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private async Task HandleInitialConnection(Socket handler)
    {
        byte[] buffer = new byte[BufferSize];
        StringBuilder sb = new StringBuilder();
        int bytesRead;

        try
        {
            do
            {
                bytesRead = await handler.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
                sb.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
            } while (handler.Available > 0); // Continue reading if more data is available

            string jsonString = sb.ToString();

            InitialMouseData m = JsonSerializer.Deserialize<InitialMouseData>(jsonString);
            DisplayEvent.edge = m.direction;
            DisplayEvent.margin = m.margin;
            Console.WriteLine($"{DisplayEvent.edge}, {DisplayEvent.margin}");

            _isConnected = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling client: {ex.Message}");
        }
        finally
        {
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
        }
    }

    public bool IsListening()
    {
        if (_listener == null || !_isConnected) return false;
        try
        {
            return _listener.Poll(0, SelectMode.SelectRead);
        }
        catch
        {
            return false;
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
                Console.WriteLine($"Coordinate client connected from {_currentClient.RemoteEndPoint}");
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
        if (_currentClient == null || _currentClient.Connected) return false;

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
                MouseMovementEventArgs? m = JsonSerializer.Deserialize<MouseMovementEventArgs>(jsonString);
                if (m != null)
                {
                    MouseService.EstimateVelocity(m);
                    MouseService.SetCursor();
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