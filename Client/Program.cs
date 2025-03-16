using ClientChat;
using Library;

var fromName = Environment.GetEnvironmentVariable("FROM_NAME");
var toName = Environment.GetEnvironmentVariable("TO_NAME");
var client = new Task(() =>
{
    if (toName != null && fromName != null)
    {
        var toUser = new User
        {
            Nick = toName
        };

        var fromUser = new User
        {
            Nick = fromName
        };

        new Client().Run("127.0.0.1", 7777, fromUser, toUser);
    }


});

client.Start();
client.Wait();