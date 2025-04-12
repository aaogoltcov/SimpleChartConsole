using Microsoft.EntityFrameworkCore;
using ServerChat;

var options = new DbContextOptionsBuilder<Context>()
    .LogTo(Console.WriteLine)
    .UseLazyLoadingProxies()
    .UseNpgsql("Host=localhost;Port=5432;Database=chatdb;Username=postgres;Password=postgres")
    .Options;
var ctx = new Context(options);
var server = new Task(() => new Server(ctx).Run("127.0.0.1", 7777));

server.Start();
server.Wait();