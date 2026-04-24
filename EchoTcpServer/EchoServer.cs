namespace EchoTcpServer;

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
public class EchoServer
{
    private readonly int _initialPort;
    private TcpListener? _listener;
    private readonly CancellationTokenSource _cancellationTokenSource;

    public int Port => _listener?.LocalEndpoint is IPEndPoint ep ? ep.Port : _initialPort;

    public EchoServer(int port)
    {
        _initialPort = port;
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public async Task StartAsync()
    {
        _listener = new TcpListener(IPAddress.Any, _initialPort);
        _listener.Start();
        Console.WriteLine($"Server started on port {Port}.");

        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            try
            {
                TcpClient client = await _listener.AcceptTcpClientAsync(_cancellationTokenSource.Token);
                Console.WriteLine("Client connected.");

                _ = Task.Run(() => HandleClientAsync(client, _cancellationTokenSource.Token));
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.OperationAborted)
            {
                break; 
            }
        }

        Console.WriteLine("Server shutdown.");
    }

    private static async Task HandleClientAsync(TcpClient client, CancellationToken token)
    {
        using (client)
        using (NetworkStream stream = client.GetStream())
        {
            try
            {
                byte[] buffer = new byte[8192];
                int bytesRead;

                while (!token.IsCancellationRequested && (bytesRead = await stream.ReadAsync(buffer.AsMemory(), token)) > 0)
                {
                    await stream.WriteAsync(buffer.AsMemory(0, bytesRead), token);
                    Console.WriteLine($"Echoed {bytesRead} bytes to the client.");
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("Client disconnected.");
            }
        }
    }

    public void Stop()
    {
        _cancellationTokenSource.Cancel();
        _listener?.Stop();
        _cancellationTokenSource.Dispose();
        Console.WriteLine("Server stopped.");
    }
}