using System.Text.Json;

namespace Library;

public class User(string nick)
{
    public readonly string Nick = nick;

    public string ToJson()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions() { IncludeFields = true });
    }

    public static User? GetUser(string json)
    {
        return JsonSerializer.Deserialize<User>(json, new JsonSerializerOptions() { IncludeFields = true });
    }
    
    public override string ToString()
    {
        return this.Nick;
    }
}