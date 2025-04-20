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

        client.Object.SendMessageHandler(
            It.IsAny<CancellationTokenSource>(), It.IsAny<User>(), It.IsAny<User>());

        client.Verify(
            x =>
                x.SendMessageHandler(
                    It.IsAny<CancellationTokenSource>(), It.IsAny<User>(), It.IsAny<User>()), Times.Once);
    }

    [Test]
    public void TestReceiveMessageHandler()
    {
        var client = new Mock<IClient>();

        client.Object.ReceiveMessageHandler(
            It.IsAny<CancellationTokenSource>());

        client.Verify(
            x => x.ReceiveMessageHandler(
                It.IsAny<CancellationTokenSource>()), Times.Once);
    }
}