using System.Text.Json;

namespace SimpleChartConsole;

public class Message
{
    public string Nick;
    public DateTime DateTime;
    public string Content;

    public Message(string nick, string content)
    {
        Nick = nick;
        Content = content;
        DateTime = DateTime.Now;
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
        return $"{this.Nick} ({DateTime}): {Content}";
    }
}