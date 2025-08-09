using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using services;
using Shared;

public class NetworkService
{
    private Socket? _listener;
    private const int Port = 11111;
    private const int BufferSize = 1024;

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

    public async Task ReceiveCoords()
    {
        byte[] buffer = new byte[BufferSize];
        StringBuilder sb = new StringBuilder();
        int bytesRead;
        if (_listener != null)
        {
            try
            {
                do
                {
                    bytesRead = await _listener.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
                    sb.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
                } while (_listener.Available > 0); // Continue reading if more data is available

                string jsonString = sb.ToString();

                InitialMouseData m = JsonSerializer.Deserialize<InitialMouseData>(jsonString);
                DisplayEvent.edge = m.direction;
                DisplayEvent.margin = m.margin;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling client: {ex.Message}");
            }
        }
        
    }

    public void Disconnect()
    {
        _listener?.Shutdown(SocketShutdown.Both);
        _listener?.Close();
    }
}