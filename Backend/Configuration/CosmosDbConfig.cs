namespace ColumbiaAi.Backend.Configuration;

public class CosmosDbConfig
{
    /// <summary>
    /// MongoDB connection string for Cosmos DB
    /// Example: mongodb+srv://Hemanth:<password>@cosmosdbforcoumbiacopilot.global.mongocluster.cosmos.azure.com/?tls=true&authMechanism=SCRAM-SHA-256
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Database name
    /// </summary>
    public string DatabaseName { get; set; } = string.Empty;

    /// <summary>
    /// Collections (Containers) in the database
    /// </summary>
    public string UsersContainer { get; set; } = string.Empty;
    public string SessionsContainer { get; set; } = string.Empty;
    public string ChatHistoryContainer { get; set; } = string.Empty;
}
