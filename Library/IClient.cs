using System.Net;
using System.Net.Sockets;

namespace Library;

public interface IClient
{
    public void Run(string ip, int port, User user, User toUser);

    public void SendMessageHandler(UdpClient udpClient, IPEndPoint localEndPoint,
        CancellationTokenSource cancelTokenSource, User user, User toUser);

    public void ReceiveMessageHandler(UdpClient udpClient, IPEndPoint localEndPoint,
        CancellationTokenSource cancelTokenSource);
}