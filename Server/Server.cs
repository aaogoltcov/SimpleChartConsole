using System.Net;
using System.Text;
using Library;
using NetMQ;

namespace ServerChat;

public class Server(Context ctx) : IServer
{
    private readonly Dictionary<string, NetMQFrame> _clients = new();
    private readonly RouterSocketAdapter _routerSocket = new RouterSocketAdapter();

    private void RegisterUser(User fromUser,  NetMQFrame client)
    {
        var fromUserNick = fromUser?.Nick;

        if (fromUserNick == null) return;

        var connectedUser = ctx.Users.FirstOrDefault(user => user.Nick == fromUserNick);
        
        _clients[fromUserNick] = client;
        Console.WriteLine($"Подключился пользователь {fromUser}: {client}.");

        if (connectedUser != null)
        {
            var messagesToReceived = ctx.Messages
                .Where(message => message.ToUserId == connectedUser.Id)
                .Where(message => message.IsReceived == false).ToList();
            
            foreach (var message in messagesToReceived)
            {
                string messageJson = message.ToJson();
                byte[] bytes = Encoding.UTF8.GetBytes(messageJson);

                _routerSocket.SendClientMessage(client, bytes);
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
        var message = ctx.Messages.FirstOrDefault(x => x.Id == id);

        if (message != null)
        {
            message.IsReceived = true;
            ctx.SaveChanges();
        }
    }

    private void RelyMessage(Message message, NetMQFrame client)
    {
        var messageEntity = new Message
        {
            FromUserId = ctx.Users.FirstOrDefault(x => x.Nick == message.FromUser.Nick).Id,
            ToUserId = ctx.Users.FirstOrDefault(x => x.Nick == message.ToUser.Nick).Id,
            Content = message.Content,
            DateTime = message.DateTime,
        };
        
        ctx.Messages.Add(messageEntity);
        ctx.SaveChanges();
        
        
        
        if (_clients.TryGetValue(messageEntity.ToUser.Nick, out var endClient))
        {
            string messageJson = messageEntity.ToJson();
            byte[] bytes = Encoding.UTF8.GetBytes(messageJson);
            
            _routerSocket.SendClientMessage(endClient, bytes);
            _routerSocket.SendClientMessage(client, Encoding.UTF8.GetBytes("Сообщение отправлено!"));
        }
        else
        {
            _routerSocket.SendClientMessage(client, Encoding.UTF8.GetBytes("Сообщение не отправлено!"));
        }
    }

    public void Run(string ip, int port)
    {
        IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse(ip), 0);
        CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
        
        _routerSocket.Bind("tcp://*:" + port);

        Console.WriteLine("Сервер запущен и ожидает сообщения от клиента.");

        while (cancelTokenSource.IsCancellationRequested == false)
        {
            try
            {
                var msg = _routerSocket.ReceiveMultipartMessage();
                var client = msg.First;
                var clientMessage = msg.Last.ConvertToString();

                if (clientMessage == "exit")
                {
                    Console.WriteLine("Сервер: выход");

                    cancelTokenSource.Cancel();
                    
                    break;
                }

                User? user = User.GetUser(clientMessage);

                if (user?.Nick != null)
                {
                    RegisterUser(user, client);
                }

                if (user?.Nick == null)
                {
                    Message? message = Message.GetMessage(clientMessage);

                    Console.WriteLine($"Сообщение {message}");
                    
                    if (_clients != null && message != null)
                    {
                        switch (message.Type)
                        {
                            case MessageType.Confirmation:
                                ConfirmMessageReceived(message.Id);

                                break;
                            case MessageType.Message:
                                RelyMessage(message, client);
                            
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