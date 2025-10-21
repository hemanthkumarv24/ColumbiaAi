namespace ColumbiaAi.Backend.Configuration;

public class CosmosDbConfig
{
    public string Endpoint { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string UsersContainer { get; set; } = string.Empty;
    public string SessionsContainer { get; set; } = string.Empty;
    public string ChatHistoryContainer { get; set; } = string.Empty;
}
