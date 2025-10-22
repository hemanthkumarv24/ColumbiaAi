using MongoDB.Driver;
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
    private readonly IMongoCollection<AppUser> _usersCollection;
    private readonly IMongoCollection<ChatSession> _sessionsCollection;
    private readonly IMongoCollection<ChatMessage> _chatHistoryCollection;

    public CosmosDbService(IMongoDatabase database, IConfiguration config)
    {
        _usersCollection = database.GetCollection<AppUser>(config["CosmosDb:UsersContainer"]);
        _sessionsCollection = database.GetCollection<ChatSession>(config["CosmosDb:SessionsContainer"]);
        _chatHistoryCollection = database.GetCollection<ChatMessage>(config["CosmosDb:ChatHistoryContainer"]);
    }

    public async Task<AppUser?> GetUserByEmailAsync(string email)
        => await _usersCollection.Find(u => u.Email == email).FirstOrDefaultAsync();

    public async Task<AppUser?> GetUserByIdAsync(string userId)
        => await _usersCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();

    public async Task<AppUser> CreateUserAsync(AppUser user)
    {
        await _usersCollection.InsertOneAsync(user);
        return user;
    }

    public async Task<AppUser> UpdateUserAsync(AppUser user)
    {
        await _usersCollection.ReplaceOneAsync(u => u.Id == user.Id, user);
        return user;
    }

    public async Task<ChatSession> CreateSessionAsync(ChatSession session)
    {
        await _sessionsCollection.InsertOneAsync(session);
        return session;
    }

    public async Task<ChatSession?> GetSessionAsync(string sessionId)
        => await _sessionsCollection.Find(s => s.Id == sessionId).FirstOrDefaultAsync();

    public async Task<List<ChatSession>> GetUserSessionsAsync(string userId)
        => await _sessionsCollection
            .Find(s => s.UserId == userId)
            .SortByDescending(s => s.UpdatedAt)
            .ToListAsync();

    public async Task<ChatSession> UpdateSessionAsync(ChatSession session)
    {
        session.UpdatedAt = DateTime.UtcNow;
        await _sessionsCollection.ReplaceOneAsync(s => s.Id == session.Id, session);
        return session;
    }

    public async Task<ChatMessage> AddMessageAsync(ChatMessage message)
    {
        await _chatHistoryCollection.InsertOneAsync(message);
        return message;
    }

    public async Task<List<ChatMessage>> GetSessionMessagesAsync(string sessionId)
        => await _chatHistoryCollection
            .Find(m => m.SessionId == sessionId)
            .SortBy(m => m.Timestamp)
            .ToListAsync();
}
