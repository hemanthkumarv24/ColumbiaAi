using Microsoft.Azure.Cosmos;
using ColumbiaAi.Backend.Configuration;
using ColumbiaAi.Backend.Models;
using CosmosUser = Microsoft.Azure.Cosmos.User;
using AppUser = ColumbiaAi.Backend.Models.User;

namespace ColumbiaAi.Backend.Services;

public interface ICosmosDbService
{
    Task<AppUser?> GetUserByEmailAsync(string email);
    Task<AppUser?> GetUserByIdAsync(string userId);
    Task<AppUser> CreateUserAsync(AppUser user);
    Task<AppUser> UpdateUserAsync(AppUser user);
    Task<ChatSession> CreateSessionAsync(ChatSession session);
    Task<ChatSession?> GetSessionAsync(string sessionId);
    Task<List<ChatSession>> GetUserSessionsAsync(string userId);
    Task<ChatSession> UpdateSessionAsync(ChatSession session);
    Task<ChatMessage> AddMessageAsync(ChatMessage message);
    Task<List<ChatMessage>> GetSessionMessagesAsync(string sessionId);
}

public class CosmosDbService : ICosmosDbService
{
    private readonly Container _usersContainer;
    private readonly Container _sessionsContainer;
    private readonly Container _chatHistoryContainer;

    public CosmosDbService(CosmosClient cosmosClient, CosmosDbConfig config)
    {
        var database = cosmosClient.GetDatabase(config.DatabaseName);
        _usersContainer = database.GetContainer(config.UsersContainer);
        _sessionsContainer = database.GetContainer(config.SessionsContainer);
        _chatHistoryContainer = database.GetContainer(config.ChatHistoryContainer);
    }

    public async Task<AppUser?> GetUserByEmailAsync(string email)
    {
        try
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.email = @email")
                .WithParameter("@email", email);
            
            var iterator = _usersContainer.GetItemQueryIterator<AppUser>(query);
            var results = await iterator.ReadNextAsync();
            
            return results.FirstOrDefault();
        }
        catch (CosmosException)
        {
            return null;
        }
    }

    public async Task<AppUser?> GetUserByIdAsync(string userId)
    {
        try
        {
            var response = await _usersContainer.ReadItemAsync<AppUser>(userId, new PartitionKey(userId));
            return response.Resource;
        }
        catch (CosmosException)
        {
            return null;
        }
    }

    public async Task<AppUser> CreateUserAsync(AppUser user)
    {
        var response = await _usersContainer.CreateItemAsync(user, new PartitionKey(user.Id));
        return response.Resource;
    }

    public async Task<AppUser> UpdateUserAsync(AppUser user)
    {
        var response = await _usersContainer.ReplaceItemAsync(user, user.Id, new PartitionKey(user.Id));
        return response.Resource;
    }

    public async Task<ChatSession> CreateSessionAsync(ChatSession session)
    {
        var response = await _sessionsContainer.CreateItemAsync(session, new PartitionKey(session.UserId));
        return response.Resource;
    }

    public async Task<ChatSession?> GetSessionAsync(string sessionId)
    {
        try
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.id = @id")
                .WithParameter("@id", sessionId);
            
            var iterator = _sessionsContainer.GetItemQueryIterator<ChatSession>(query);
            var results = await iterator.ReadNextAsync();
            
            return results.FirstOrDefault();
        }
        catch (CosmosException)
        {
            return null;
        }
    }

    public async Task<List<ChatSession>> GetUserSessionsAsync(string userId)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.userId = @userId ORDER BY c.updatedAt DESC")
            .WithParameter("@userId", userId);
        
        var iterator = _sessionsContainer.GetItemQueryIterator<ChatSession>(query);
        var sessions = new List<ChatSession>();
        
        while (iterator.HasMoreResults)
        {
            var results = await iterator.ReadNextAsync();
            sessions.AddRange(results);
        }
        
        return sessions;
    }

    public async Task<ChatSession> UpdateSessionAsync(ChatSession session)
    {
        session.UpdatedAt = DateTime.UtcNow;
        var response = await _sessionsContainer.ReplaceItemAsync(session, session.Id, new PartitionKey(session.UserId));
        return response.Resource;
    }

    public async Task<ChatMessage> AddMessageAsync(ChatMessage message)
    {
        var response = await _chatHistoryContainer.CreateItemAsync(message, new PartitionKey(message.SessionId));
        return response.Resource;
    }

    public async Task<List<ChatMessage>> GetSessionMessagesAsync(string sessionId)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.sessionId = @sessionId ORDER BY c.timestamp ASC")
            .WithParameter("@sessionId", sessionId);
        
        var iterator = _chatHistoryContainer.GetItemQueryIterator<ChatMessage>(query);
        var messages = new List<ChatMessage>();
        
        while (iterator.HasMoreResults)
        {
            var results = await iterator.ReadNextAsync();
            messages.AddRange(results);
        }
        
        return messages;
    }
}
