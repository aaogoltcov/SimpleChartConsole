using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SimpleChartConsole;

public class Chat
{
    public void Server(string ip, int port)
    {
        IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse(ip), 0);
        UdpClient udpClient = new UdpClient(port);
        
        Console.WriteLine("Сервер: запущен и ожидает сообщения от клиента");

        while (true)
        {
            try
            {
                byte[] buffer = udpClient.Receive(ref localEndPoint);
                string encoded = Encoding.UTF8.GetString(buffer);
                
                if (encoded == "exit")
                {
                    
                    Console.WriteLine("Сервер: выход");
                    
                    break;
                }

                Message? message = Message.GetMessage(encoded);

                Console.WriteLine("Сервер: получено сообщение - " + message);
                udpClient.Send(Encoding.UTF8.GetBytes("Сообщение получено"), localEndPoint);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }
    }
    
    public void Client(string ip, int port, string nick)
    {
        IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
        UdpClient udpClient = new UdpClient();
        
        Console.WriteLine("Клиент: запущен");
        
        while (true)
        {
            Console.WriteLine("Клиент: введите сообщение");
            string? input = Console.ReadLine();

            if (String.IsNullOrEmpty(input))
            {
                continue;
            }

            if (input == "exit")
            {
                Console.WriteLine("Клиент: выход");
                
                udpClient.Send(Encoding.UTF8.GetBytes("exit"), localEndPoint);
                
                break;
            }

            Message message = new Message(nick, input);
            string messageJson = message.ToJson();
            byte[] bytes = Encoding.UTF8.GetBytes(messageJson);
            udpClient.Send(bytes, localEndPoint);
            
            byte[] buffer = udpClient.Receive(ref localEndPoint);
            string encoded = Encoding.UTF8.GetString(buffer);
            Console.WriteLine(encoded);
        }
    }
}