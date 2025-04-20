using System.Net;
using System.Net.Sockets;
using System.Text;
using Library;
using NetMQ;
using NetMQ.Sockets;

namespace ClientChat;

public class Client : IClient
{
    private readonly DealerSocket _dealerSocket = new DealerSocket();

    public void Run(string ip, int port, User user, User toUser)
    {
        _dealerSocket.Connect("tcp://" + ip + ":" + port);

        CancellationTokenSource cancelTokenSource = new CancellationTokenSource();

        string userJson = user.ToJson();
        byte[] userBytes = Encoding.UTF8.GetBytes(userJson);
        _dealerSocket.SendFrame(userBytes);

        Console.WriteLine($"Здравсвуйте, {user}!");

        var inputTask = new Task(() => SendMessageHandler(cancelTokenSource, user, toUser));
        var listenerTask = new Task(() => ReceiveMessageHandler(cancelTokenSource));

        inputTask.Start();
        listenerTask.Start();

        inputTask.Wait(cancelTokenSource.Token);
        listenerTask.Wait(cancelTokenSource.Token);
    }

    public void SendMessageHandler(
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

                _dealerSocket.SendFrame(Encoding.UTF8.GetBytes("exit"));

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
            _dealerSocket.SendFrame(bytes);
        }
    }

    public void ReceiveMessageHandler(
        CancellationTokenSource cancelTokenSource)
    {
        while (cancelTokenSource.IsCancellationRequested == false)
        {
            string encoded = _dealerSocket.ReceiveFrameString();

            try
            {
                Message? serverMessage = Message.GetMessage(encoded);

                if (serverMessage != null)
                {
                    var messageEntity = serverMessage;
                    messageEntity.Type = MessageType.Confirmation;

                    string messageJson = messageEntity.ToJson();
                    byte[] bytes = Encoding.UTF8.GetBytes(messageJson);
                    _dealerSocket.SendFrame(bytes);

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