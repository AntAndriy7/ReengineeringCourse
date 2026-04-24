namespace EchoTcpServer;

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Security.Cryptography;

public class UdpTimedSender : IDisposable
{
    private readonly string _host;
    private readonly int _port;
    private readonly UdpClient _udpClient;
    private Timer? _timer;
    private ushort _messageIndex = 0;
    private bool _disposed;

    public UdpTimedSender(string host, int port)
    {
        _host = host;
        _port = port;
        _udpClient = new UdpClient();
    }

    public void StartSending(int intervalMilliseconds)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_timer != null)
            throw new InvalidOperationException("Sender is already running.");

        _timer = new Timer(SendMessageCallback, null, 0, intervalMilliseconds);
    }

    private void SendMessageCallback(object? state)
    {
        try
        {
            byte[] samples = new byte[1024];
            RandomNumberGenerator.Fill(samples);
            _messageIndex++;

            byte[] msg = [0x04, 0x84, .. BitConverter.GetBytes(_messageIndex), .. samples];
            var endpoint = new IPEndPoint(IPAddress.Parse(_host), _port);

            _udpClient.Send(msg, msg.Length, endpoint);
            Console.WriteLine($"Message sent to {_host}:{_port}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending message: {ex.Message}");
        }
    }

    public void StopSending()
    {
        _timer?.Dispose();
        _timer = null;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                StopSending();
                _udpClient.Dispose();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}