using Newtonsoft.Json;

namespace ColumbiaAi.Backend.Models;

public class ChatSession
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [JsonProperty("userId")]
    public string UserId { get; set; } = string.Empty;
    
    [JsonProperty("title")]
    public string Title { get; set; } = "New Conversation";
    
    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [JsonProperty("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    [JsonProperty("context")]
    public string Context { get; set; } = string.Empty;
    
    [JsonProperty("isActive")]
    public bool IsActive { get; set; } = true;
}
