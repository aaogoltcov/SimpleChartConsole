using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Library;

public class User
{
    public int Id { get; set; }
    public string Nick  { get; set; }
    
    [InverseProperty(nameof(Message.FromUser))]
    public virtual ICollection<Message> MessagesFromUser { get; set; } = new List<Message>();
    
    [InverseProperty(nameof(Message.ToUser))]
    public virtual ICollection<Message> MessagesToUser { get; set; } = new List<Message>();
    
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