using NUnit.Framework;
using NetSdrClientApp.Networking;
using System.Net;
using System.Net.Sockets;

namespace NetSdrClientAppTests;

[TestFixture]
public class TcpClientWrapperTests
{
    private TcpClientWrapper? _tcpWrapper;
    private const string LocalHost = "127.0.0.1";

    [Test]
    public void InitialStatusCheck()
    {
        // Act
        _tcpWrapper = new TcpClientWrapper(LocalHost, 8080);

        // Assert
        Assert.That(_tcpWrapper.Connected, Is.False);
    }

    [Test]
    public void ConnectionToClosedPort()
    {
        // Arrange
        _tcpWrapper = new TcpClientWrapper(LocalHost, 44444);

        // Act
        _tcpWrapper.Connect();

        // Assert
        Assert.That(_tcpWrapper.Connected, Is.False);
    }

    [Test]
    public async Task FullLifecycleCheck()
    {
        // Arrange
        var srv = new TcpListener(IPAddress.Loopback, 0);
        srv.Start();
        var port = ((IPEndPoint)srv.LocalEndpoint).Port;
        _tcpWrapper = new TcpClientWrapper(LocalHost, port);

        // Act
        _tcpWrapper.Connect();
        using var incoming = await srv.AcceptTcpClientAsync();

        // Assert
        Assert.That(_tcpWrapper.Connected, Is.True);

        // Act
        _tcpWrapper.Disconnect();

        // Assert
        Assert.That(_tcpWrapper.Connected, Is.False);
        srv.Stop();
    }

    [Test]
    public async Task MessageReceivedEventFires()
    {
        // Arrange
        var srv = new TcpListener(IPAddress.Loopback, 0);
        srv.Start();
        var port = ((IPEndPoint)srv.LocalEndpoint).Port;
        _tcpWrapper = new TcpClientWrapper(LocalHost, port);
        byte[]? receivedData = null;
        _tcpWrapper.MessageReceived += (s, data) => receivedData = data;

        // Act
        _tcpWrapper.Connect();
        using var serverSide = await srv.AcceptTcpClientAsync();
        var stream = serverSide.GetStream();

        byte[] sendData = { 0xDE, 0xAD, 0xBE, 0xEF };
        await stream.WriteAsync(sendData.AsMemory());

        await Task.Delay(200);

        // Assert
        Assert.That(receivedData, Is.Not.Null);
        Assert.That(receivedData, Is.EqualTo(sendData));

        _tcpWrapper.Disconnect();
        srv.Stop();
    }

    [Test]
    public void SendDataWithoutConnection()
    {
        // Arrange
        _tcpWrapper = new TcpClientWrapper(LocalHost, 1234);
        var payload = new byte[] { 0xAA, 0xBB, 0xCC };

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _tcpWrapper.SendMessageAsync(payload));
    }
}
