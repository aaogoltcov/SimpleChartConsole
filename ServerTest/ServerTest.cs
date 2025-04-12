using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Library;
using Microsoft.EntityFrameworkCore;
using ServerChat;

namespace ServerTest;

[TestFixture]
public class ServerTest
{
    private Context _ctx;
    private Server? _server;
    private UdpClient _udpClient;
    private IPEndPoint _localEndPoint;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<Context>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
        _ctx = new Context(options);
        _server = new Server(_ctx);
        _udpClient = new UdpClient(7777);
        _localEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0);
    }

    [TearDown]
    public void TeatDown()
    {
        _ctx.Dispose();
        _server = null;
        _udpClient.Close();
        _udpClient.Close();
    }

    [Test]
    public void TestRegisterUser()
    {
        var testUser = new User
        {
            Nick = "TestUser"
        };

        var registerUserMethod =
            _server?.GetType().GetMethod("RegisterUser", BindingFlags.NonPublic | BindingFlags.Instance);

        if (registerUserMethod != null)
        {
            registerUserMethod.Invoke(_server, new object[] { testUser, _localEndPoint });
        }

        var firstContextUser = _ctx.Users.FirstOrDefault();

        if (firstContextUser != null)
        {
            Assert.That(firstContextUser.Nick, Is.EqualTo(testUser.Nick));
        }

        Assert.That(firstContextUser, Is.Not.Null);
    }

    [Test]
    public void TestConfirmMessageReceived()
    {
        var testMessage = new Message
        {
            Id = 1,
            Content = "TestMessage",
            DateTime = DateTime.Now,
            IsReceived = false,
            FromUserId = 1,
            ToUserId = 2,
        };
        
        _ctx.Messages.Add(testMessage);
        _ctx.SaveChanges();
        
        var confirmMessageReceivedMethod =
            _server?.GetType().GetMethod("ConfirmMessageReceived", BindingFlags.NonPublic | BindingFlags.Instance);

        if (confirmMessageReceivedMethod != null)
        {
            confirmMessageReceivedMethod.Invoke(_server, new object[] { testMessage.Id });
        }
        
        var firstContextMessage = _ctx.Messages.FirstOrDefault();

        if (firstContextMessage != null)
        {
            Assert.That(firstContextMessage.Id, Is.EqualTo(testMessage.Id));
        }

        Assert.That(firstContextMessage, Is.Not.Null);
    }
}