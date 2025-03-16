using System.Net;
using System.Net.Sockets;
using System.Text;
using Library;

namespace ServerChat;

public class Server : IServer
{
    private readonly Dictionary<string, IPEndPoint> _clients = new();
    private UdpClient udpClient;

    private void RegisterUser(User fromUser, IPEndPoint fromUserEndPoint)
    {
        using var ctx = new Context();
        var fromUserNick = fromUser?.Nick;

        if (fromUserNick == null) return;

        var connectedUser = ctx.Users.FirstOrDefault(user => user.Nick == fromUserNick);
        
        _clients[fromUserNick] = fromUserEndPoint;
        Console.WriteLine($"Подключился пользователь {fromUser}: {fromUserEndPoint}.");

        if (connectedUser != null)
        {
            var messagesToReceived = ctx.Messages
                .Where(message => message.ToUserId == connectedUser.Id)
                .Where(message => message.IsReceived == false).ToList();
            
            foreach (var message in messagesToReceived)
            {
                string messageJson = message.ToJson();
                byte[] bytes = Encoding.UTF8.GetBytes(messageJson);
                udpClient.Send(bytes, fromUserEndPoint);
            }

            return;
        }

        var newUser = new User
        {
            Nick = fromUserNick
        };

        ctx.Add(newUser);
        ctx.SaveChanges();
    }

    private void ConfirmMessageReceived(int? id)
    {
        using var ctx = new Context();
        var message = ctx.Messages.FirstOrDefault(x => x.Id == id);

        if (message != null)
        {
            message.IsReceived = true;
            ctx.SaveChanges();
        }
    }

    private void RelyMessage(Message message, IPEndPoint localEndPoint)
    {
        using var ctx = new Context();
        var messageEntity = new Message
        {
            FromUserId = ctx.Users.FirstOrDefault(x => x.Nick == message.FromUser.Nick).Id,
            ToUserId = ctx.Users.FirstOrDefault(x => x.Nick == message.ToUser.Nick).Id,
            Content = message.Content,
            DateTime = message.DateTime,
        };
        
        ctx.Messages.Add(messageEntity);
        ctx.SaveChanges();
        
        if (_clients.TryGetValue(messageEntity.ToUser.Nick, out var endPoint))
        {
            string messageJson = messageEntity.ToJson();
            byte[] bytes = Encoding.UTF8.GetBytes(messageJson);
            udpClient.Send(bytes, endPoint);
            udpClient.Send(Encoding.UTF8.GetBytes("Сообщение отправлено!"), localEndPoint);
        }
        else
        {
            udpClient.Send(Encoding.UTF8.GetBytes("Сообщение не отправлено!"), localEndPoint);
        }
    }

    public void Run(string ip, int port)
    {
        IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse(ip), 0);
        CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
        
        udpClient = new UdpClient(port);

        Console.WriteLine("Сервер запущен и ожидает сообщения от клиента.");

        while (cancelTokenSource.IsCancellationRequested == false)
        {
            try
            {
                byte[] buffer = udpClient.Receive(ref localEndPoint);
                string encoded = Encoding.UTF8.GetString(buffer);

                if (encoded == "exit")
                {
                    Console.WriteLine("Сервер: выход");

                    cancelTokenSource.Cancel();
                    
                    break;
                }

                User? user = User.GetUser(encoded);

                if (user?.Nick != null)
                {
                    RegisterUser(user, localEndPoint);
                }

                if (user?.Nick == null)
                {
                    Message? message = Message.GetMessage(encoded);

                    Console.WriteLine($"Сообщение {message}");
                    
                    if (_clients != null && message != null)
                    {
                        switch (message.Type)
                        {
                            case MessageType.Confirmation:
                                ConfirmMessageReceived(message.Id);

                                break;
                            case MessageType.Message:
                                RelyMessage(message, localEndPoint);
                            
                                break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}