using System.Net;
using System.Net.Sockets;
using System.Text;
using Library;

namespace ClientChat;

public class Client : IClient
{
    public void Run(string ip, int port, User user, User toUser)
    {
        IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
        UdpClient udpClient = new UdpClient();
        CancellationTokenSource cancelTokenSource = new CancellationTokenSource();

        string userJson = user.ToJson();
        byte[] userBytes = Encoding.UTF8.GetBytes(userJson);
        udpClient.Send(userBytes, localEndPoint);

        Console.WriteLine($"Здравсвуйте, {user}!");

        var inputTask = new Task(() => SendMessageHandler(udpClient, localEndPoint, cancelTokenSource, user, toUser));
        var listenerTask = new Task(() => ReceiveMessageHandler(udpClient, localEndPoint, cancelTokenSource));

        inputTask.Start();
        listenerTask.Start();

        inputTask.Wait(cancelTokenSource.Token);
        listenerTask.Wait(cancelTokenSource.Token);
    }

    public void SendMessageHandler(UdpClient udpClient, IPEndPoint localEndPoint,
        CancellationTokenSource cancelTokenSource, User user, User toUser)
    {
        while (cancelTokenSource.IsCancellationRequested == false)
        {
            Console.WriteLine($"Введите сообщение пользовтелю {toUser}, или 'exit' для выхода:");
            string? input = Console.ReadLine();

            if (String.IsNullOrEmpty(input))
            {
                continue;
            }

            if (input == "exit")
            {
                Console.WriteLine("Клиент: выход");

                udpClient.Send(Encoding.UTF8.GetBytes("exit"), localEndPoint);

                cancelTokenSource.Cancel();
                break;
            }

            Message message = new Message
            {
                FromUser = user,
                ToUser = toUser,
                Content = input,
                DateTime = DateTime.Now.ToUniversalTime(),
                Type = MessageType.Message
            };
            string messageJson = message.ToJson();
            byte[] bytes = Encoding.UTF8.GetBytes(messageJson);
            udpClient.Send(bytes, localEndPoint);
        }
    }

    public void ReceiveMessageHandler(UdpClient udpClient, IPEndPoint localEndPoint,
        CancellationTokenSource cancelTokenSource)
    {
        while (cancelTokenSource.IsCancellationRequested == false)
        {
            byte[] buffer = udpClient.Receive(ref localEndPoint);
            string encoded = Encoding.UTF8.GetString(buffer);

            try
            {
                Message? serverMessage = Message.GetMessage(encoded);

                if (serverMessage != null)
                {
                    var messageEntity = serverMessage;
                    messageEntity.Type = MessageType.Confirmation;

                    string messageJson = messageEntity.ToJson();
                    byte[] bytes = Encoding.UTF8.GetBytes(messageJson);
                    udpClient.Send(bytes, localEndPoint);

                    Console.WriteLine(
                        $"Сообщение от пользователя {messageEntity.ToUser.Nick}: {messageEntity.Content}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(encoded);
            }
        }
    }
}