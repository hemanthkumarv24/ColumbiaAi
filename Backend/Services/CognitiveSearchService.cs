using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using ColumbiaAi.Backend.Configuration;

namespace ColumbiaAi.Backend.Services;

public interface ICognitiveSearchService
{
    Task<List<string>> SearchDocumentsAsync(string query, int maxResults = 5);
}

public class CognitiveSearchService : ICognitiveSearchService
{
    private readonly SearchClient _searchClient;

    public CognitiveSearchService(CognitiveSearchConfig config)
    {
        _searchClient = new SearchClient(
            new Uri(config.Endpoint),
            config.IndexName,
            new AzureKeyCredential(config.ApiKey)
        );
    }

    public async Task<List<string>> SearchDocumentsAsync(string query, int maxResults = 5)
    {
        try
        {
            var searchOptions = new SearchOptions
            {
                Size = maxResults,
                IncludeTotalCount = false
            };

            var results = await _searchClient.SearchAsync<SearchDocument>(query, searchOptions);
            var documents = new List<string>();

            await foreach (var result in results.Value.GetResultsAsync())
            {
                if (result.Document.TryGetValue("content", out var content))
                {
                    documents.Add(content.ToString() ?? string.Empty);
                }
            }

            return documents;
        }
        catch
        {
            return new List<string>();
        }
    }
}
