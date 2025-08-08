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

    public async Task StartListening()
    {
        IPAddress ip = IPAddress.Any; // Listen on all available network interfaces
        IPEndPoint localEndPoint = new IPEndPoint(ip, Port);
        _listener = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            _listener.Bind(localEndPoint);
            _listener.Listen(100); // Max pending connections
            Socket handler = _listener.Accept();
            await HandleClient(handler);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private async Task HandleClient(Socket handler)
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

            Direction dir = JsonSerializer.Deserialize<Direction>(jsonString);
            DisplayEvent.edge = dir;
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
}