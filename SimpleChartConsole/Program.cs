using SimpleChartConsole;

Message message = new Message("Ivan", "Some text");

Console.WriteLine(message.ToJson());

Console.WriteLine(Message.GetMessage(message.ToJson()));

var server = new Task(() => new Chat().Server("127.0.0.1", 7777));
var client = new Task(() => new Chat().Client("127.0.0.1", 7777, "Ivan"));

server.Start();
client.Start();

server.Wait();
client.Wait();