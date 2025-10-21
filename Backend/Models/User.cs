using Newtonsoft.Json;

namespace ColumbiaAi.Backend.Models;

public class User
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [JsonProperty("email")]
    public string Email { get; set; } = string.Empty;
    
    [JsonProperty("passwordHash")]
    public string PasswordHash { get; set; } = string.Empty;
    
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [JsonProperty("profile")]
    public UserProfile? Profile { get; set; }
}

public class UserProfile
{
    [JsonProperty("preferences")]
    public Dictionary<string, string> Preferences { get; set; } = new();
    
    [JsonProperty("interests")]
    public List<string> Interests { get; set; } = new();
    
    [JsonProperty("context")]
    public string Context { get; set; } = string.Empty;
}
