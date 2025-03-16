using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Library;

public class Message
{
    public int Id  { get; set; }
    public DateTime DateTime  { get; set; }
    public string Content  { get; set; }
    public bool IsReceived  { get; set; }
    public int? FromUserId  { get; set; }
    public int? ToUserId  { get; set; }
    public MessageType Type { get; set; }
    
    [InverseProperty(nameof(User.MessagesFromUser))]
    public virtual User? FromUser { get; set; }
    
    [InverseProperty(nameof(User.MessagesToUser))]
    public virtual User? ToUser { get; set; }

    public string ToJson()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions() { IncludeFields = true, ReferenceHandler = ReferenceHandler.Preserve });
    }

    public static Message? GetMessage(string json)
    {
        return JsonSerializer.Deserialize<Message>(json, new JsonSerializerOptions() { IncludeFields = true, ReferenceHandler = ReferenceHandler.Preserve });
    }

    public override string ToString()
    {
        return $"от {this.FromUser} для {this.ToUser} в {this.DateTime}: {Content}, тип: {this.Type}";
    }
}