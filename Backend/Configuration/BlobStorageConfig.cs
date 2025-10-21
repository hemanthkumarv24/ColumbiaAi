namespace ColumbiaAi.Backend.Configuration;

public class BlobStorageConfig
{
    public string ConnectionString { get; set; } = string.Empty;
    public string ContainerName { get; set; } = string.Empty;
}
