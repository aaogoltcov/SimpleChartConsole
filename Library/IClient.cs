namespace Library;

public interface IClient
{
    public void Run(string ip, int port, User user, User toUser);
}