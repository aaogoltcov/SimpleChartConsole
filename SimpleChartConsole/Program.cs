using SimpleChartConsole;

Message message = new Message("Ivan", "Some text");

Console.WriteLine(message.ToJson());

Console.WriteLine(Message.GetMessage(message.ToJson()));

new Thread(() =>
{
    new Chart().Server("127.0.0.1", 7777);
}).Start();

new Thread(() =>
{
    new Chart().Client("127.0.0.1", 7777, "Ivan");
}).Start();