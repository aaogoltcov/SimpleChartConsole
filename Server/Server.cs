using System.Net;
using System.Net.Sockets;
using System.Text;
using Library;

namespace ServerChat;

public class Server : IServer
{
    private readonly Dictionary<string, IPEndPoint> _clients = new();

    public void Run(string ip, int port)
    {
        IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse(ip), 0);
        UdpClient udpClient = new UdpClient(port);
        CancellationTokenSource cancelTokenSource = new CancellationTokenSource();

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
                    _clients?.TryAdd(user.Nick, localEndPoint);
                    Console.WriteLine($"Подключился польхователь {user}: {localEndPoint}.");
                }

                ;

                if (user?.Nick == null)
                {
                    Message? message = Message.GetMessage(encoded);

                    Console.WriteLine($"Сообщение {message}");

                    if (_clients != null && message != null)
                    {
                        if (_clients.TryGetValue(message.ToUser.Nick, out var endPoint))
                        {
                            string messageJson = message.ToJson();
                            byte[] bytes = Encoding.UTF8.GetBytes(messageJson);
                            udpClient.Send(bytes, endPoint);

                            udpClient.Send(Encoding.UTF8.GetBytes("Сообщение доставлено!"), localEndPoint);
                        }
                        else
                        {
                            udpClient.Send(Encoding.UTF8.GetBytes("Сообщение не доставлено!"), localEndPoint);
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