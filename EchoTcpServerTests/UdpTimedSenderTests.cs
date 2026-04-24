using NUnit.Framework;
using EchoTcpServer;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace EchoTcpServerTests;

public class UdpTimedSenderTests
{
    private int GetFreeUdpPort()
    {
        using var udp = new UdpClient(0);
        return ((System.Net.IPEndPoint)udp.Client.LocalEndPoint!).Port;
    }

    [Test]
    public async Task StartSending_SendsUdpPackets_WithCorrectHeader()
    {
        // Arrange
        int port = GetFreeUdpPort();
        using var receiver = new UdpClient(port);
        using var sender = new UdpTimedSender("127.0.0.1", port);

        // Act
        sender.StartSending(50);

        var receiveTask = receiver.ReceiveAsync();
        var completedTask = await Task.WhenAny(receiveTask, Task.Delay(1000));

        // Assert
        Assert.That(completedTask, Is.EqualTo(receiveTask), "Пакети UDP не були отримані вчасно.");

        var udpReceiveResult = receiveTask.Result;
        var bytes = udpReceiveResult.Buffer;

        Assert.That(bytes.Length, Is.GreaterThan(0));
        Assert.That(bytes[0], Is.EqualTo(0x04));
        Assert.That(bytes[1], Is.EqualTo(0x84));
    }

    [Test]
    public void StartSending_Twice_ThrowsInvalidOperationException()
    {
        // Arrange
        using var sender = new UdpTimedSender("127.0.0.1", GetFreeUdpPort());
        sender.StartSending(1000);

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => sender.StartSending(1000));
        Assert.That(ex.Message, Is.EqualTo("Sender is already running."));
    }

    [Test]
    public void StartSending_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var sender = new UdpTimedSender("127.0.0.1", GetFreeUdpPort());
        sender.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => sender.StartSending(1000));
    }

    [Test]
    public void StopSending_DoesNotThrowException()
    {
        // Arrange
        using var sender = new UdpTimedSender("127.0.0.1", GetFreeUdpPort());
        
        // Act & Assert
        Assert.DoesNotThrow(() => sender.StopSending());
    }
}