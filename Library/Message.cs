using System.Text.Json;

namespace Library;

public class Message
{
    public User User;
    public DateTime DateTime;
    public string Content;
    public User FromUser;
    public User ToUser;

    public Message(User user, string content, User toUser)
    {
        User = user;
        Content = content;
        DateTime = DateTime.Now;
        FromUser = User;
        ToUser = toUser;
    }

    public string ToJson()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions() { IncludeFields = true });
    }

    public static Message? GetMessage(string json)
    {
        return JsonSerializer.Deserialize<Message>(json, new JsonSerializerOptions() { IncludeFields = true });
    }

    public override string ToString()
    {
        return $"от {this.FromUser} для {this.ToUser} в {this.DateTime}: {Content}";
    }
}