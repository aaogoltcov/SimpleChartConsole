using ServerChat;

var server = new Task(() => new Server().Run("127.0.0.1", 7777));

server.Start();
server.Wait();