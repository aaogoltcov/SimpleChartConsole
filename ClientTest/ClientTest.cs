using System.Net;
using System.Net.Sockets;
using System.Reflection;
using ClientChat;
using Library;
using Moq;

namespace ClientTest;

[TestFixture]
public class ClientTest
{
    [Test]
    public void TestSendMessageHandler()
    {
        var client = new Mock<IClient>();

        client.Object.SendMessageHandler(It.IsAny<UdpClient>(), It.IsAny<IPEndPoint>(),
            It.IsAny<CancellationTokenSource>(), It.IsAny<User>(), It.IsAny<User>());

        client.Verify(
            x =>
                x.SendMessageHandler(It.IsAny<UdpClient>(), It.IsAny<IPEndPoint>(),
                    It.IsAny<CancellationTokenSource>(), It.IsAny<User>(), It.IsAny<User>()), Times.Once);
    }

    [Test]
    public void TestReceiveMessageHandler()
    {
        var client = new Mock<IClient>();

        client.Object.ReceiveMessageHandler(It.IsAny<UdpClient>(), It.IsAny<IPEndPoint>(),
            It.IsAny<CancellationTokenSource>());

        client.Verify(
            x => x.ReceiveMessageHandler(It.IsAny<UdpClient>(), It.IsAny<IPEndPoint>(),
                It.IsAny<CancellationTokenSource>()), Times.Once);
    }
}