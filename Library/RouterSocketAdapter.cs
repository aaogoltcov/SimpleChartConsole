using NetMQ;
using NetMQ.Sockets;

namespace Library;

public class RouterSocketAdapter : RouterSocket
{
    public void SendClientMessage(NetMQFrame client, byte[] bytes)
    {
        var responseMessage = new NetMQMessage();
        
        responseMessage.Append(client);
        responseMessage.Append(bytes);
        this.SendMultipartMessage(responseMessage);
    }

}