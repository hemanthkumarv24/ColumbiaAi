using Newtonsoft.Json;

namespace ColumbiaAi.Backend.Models;

public class ChatMessage
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [JsonProperty("sessionId")]
    public string SessionId { get; set; } = string.Empty;
    
    [JsonProperty("userId")]
    public string UserId { get; set; } = string.Empty;
    
    [JsonProperty("role")]
    public string Role { get; set; } = string.Empty; // "user" or "assistant"
    
    [JsonProperty("content")]
    public string Content { get; set; } = string.Empty;
    
    [JsonProperty("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    [JsonProperty("attachments")]
    public List<string> Attachments { get; set; } = new();
}
