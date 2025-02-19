using ClientChat;
using Library;

var fromName = Environment.GetEnvironmentVariable("FROM_NAME");
var toName = Environment.GetEnvironmentVariable("TO_NAME");
var client = new Task(() =>
{
    if (toName != null && fromName != null)
        new Client().Run("127.0.0.1", 7777, new User(fromName), new User(toName));
});

client.Start();
client.Wait();