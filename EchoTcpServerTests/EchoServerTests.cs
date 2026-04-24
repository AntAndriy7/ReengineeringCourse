using EchoTcpServer;
using System.Net.Sockets;
using System.Text;

namespace EchoTcpServerTests;

public class EchoServerTests
{
    private EchoServer _server;
    private int _dynamicPort;

    [SetUp]
    public async Task Setup()
    {
        _server = new EchoServer(0); 
        
        _ = Task.Run(() => _server.StartAsync());
        
        await Task.Delay(100); 
        
        _dynamicPort = _server.Port;
    }

    [TearDown]
    public void TearDown()
    {
        _server.Stop();
    }

    [Test]
    public async Task Echo_ReturnsSameMsg()
    {
        // Arrange
        using var client = new TcpClient();
        await client.ConnectAsync("127.0.0.1", _dynamicPort);
        using var stream = client.GetStream();

        var messageToSend = "Hello from Unit Test!";
        var bytesToSend = Encoding.UTF8.GetBytes(messageToSend);
        var buffer = new byte[1024];

        // Act
        await stream.WriteAsync(bytesToSend);

        var bytesRead = await stream.ReadAsync(buffer);
        var receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);

        // Assert
        Assert.That(receivedMessage, Is.EqualTo(messageToSend));
    }

    [Test]
    public async Task Echo_MultiMsg_Works()
    {
        // Arrange
        using var client = new TcpClient();
        await client.ConnectAsync("127.0.0.1", _dynamicPort);
        using var stream = client.GetStream();
        var buffer = new byte[1024];

        // Act & Assert 1
        await stream.WriteAsync(Encoding.UTF8.GetBytes("Message 1"));
        var bytesRead = await stream.ReadAsync(buffer);
        Assert.That(Encoding.UTF8.GetString(buffer, 0, bytesRead), Is.EqualTo("Message 1"));

        // Act & Assert 2
        await stream.WriteAsync(Encoding.UTF8.GetBytes("Message 2"));
        bytesRead = await stream.ReadAsync(buffer);
        Assert.That(Encoding.UTF8.GetString(buffer, 0, bytesRead), Is.EqualTo("Message 2"));
    }
    
    [Test]
    public async Task Echo_ClientDisconnects()
    {
        // Arrange
        using var client = new TcpClient();
        await client.ConnectAsync("127.0.0.1", _dynamicPort);
        
        // Act
        client.Close(); 

        await Task.Delay(100); 

        // Assert
        Assert.Pass();
    }
}