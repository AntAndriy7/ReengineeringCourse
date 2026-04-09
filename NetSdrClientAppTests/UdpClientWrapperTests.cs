using NUnit.Framework;
using System.Net;

namespace NetSdrClientAppTests;

[TestFixture]
public class UdpClientWrapperTests
{
    [Test]
    public void HashCodeUniqueness()
    {
        // Arrange
        var udp1 = new UdpClientWrapper(9000);
        var udp2 = new UdpClientWrapper(9001);

        // Act
        var h1 = udp1.GetHashCode();
        var h2 = udp2.GetHashCode();

        // Assert
        Assert.That(h1, Is.Not.EqualTo(h2));
    }

    [Test]
    public void ShutdownMethodsSafeCall()
    {
        // Arrange
        var udp = new UdpClientWrapper(0);

        // Act & Assert
        Assert.Multiple(() =>
        {
            Assert.That(() => udp.StopListening(), Throws.Nothing);
            Assert.That(() => udp.Exit(), Throws.Nothing);
        });
    }

    [Test]
    public async Task AsyncListeningLifecycle()
    {
        // Arrange
        var udp = new UdpClientWrapper(0);

        // Act
        var backgroundWorker = udp.StartListeningAsync();
        await Task.Delay(100);
        udp.StopListening();

        // Assert
        bool completed = backgroundWorker.Wait(1000);
        Assert.That(completed, Is.True);
    }
}
