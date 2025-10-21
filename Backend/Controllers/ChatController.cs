using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ColumbiaAi.Backend.Models;
using ColumbiaAi.Backend.Services;
using ColumbiaAi.Backend.Configuration;

namespace ColumbiaAi.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly ICosmosDbService _cosmosDb;
    private readonly IAzureOpenAIService _openAIService;
    private readonly ICognitiveSearchService _searchService;
    private readonly FeatureFlags _features;
    private readonly ILogger<ChatController> _logger;

    public ChatController(
        ICosmosDbService cosmosDb,
        IAzureOpenAIService openAIService,
        ICognitiveSearchService searchService,
        FeatureFlags features,
        ILogger<ChatController> logger)
    {
        _cosmosDb = cosmosDb;
        _openAIService = openAIService;
        _searchService = searchService;
        _features = features;
        _logger = logger;
    }

    private string GetUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
    }

    [HttpPost("message")]
    public async Task<ActionResult<ChatResponse>> SendMessage([FromBody] ChatRequest request)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            ChatSession? session;
            if (string.IsNullOrEmpty(request.SessionId))
            {
                session = new ChatSession
                {
                    UserId = userId,
                    Title = "New Conversation"
                };
                session = await _cosmosDb.CreateSessionAsync(session);
            }
            else
            {
                session = await _cosmosDb.GetSessionAsync(request.SessionId);
                if (session == null || session.UserId != userId)
                {
                    return NotFound(new { message = "Session not found" });
                }
            }

            var userMessage = new ChatMessage
            {
                SessionId = session.Id,
                UserId = userId,
                Role = "user",
                Content = request.Message,
                Attachments = request.Attachments
            };
            await _cosmosDb.AddMessageAsync(userMessage);

            var history = await _cosmosDb.GetSessionMessagesAsync(session.Id);
            
            var contextMessages = history.TakeLast(10).ToList();

            string? userContext = null;
            if (_features.EnableUserProfiling)
            {
                var user = await _cosmosDb.GetUserByIdAsync(userId);
                if (user?.Profile != null && !string.IsNullOrEmpty(user.Profile.Context))
                {
                    userContext = user.Profile.Context;
                }
            }

            var aiResponse = await _openAIService.GetChatResponseAsync(contextMessages, userContext);

            var assistantMessage = new ChatMessage
            {
                SessionId = session.Id,
                UserId = userId,
                Role = "assistant",
                Content = aiResponse
            };
            await _cosmosDb.AddMessageAsync(assistantMessage);

            session.UpdatedAt = DateTime.UtcNow;
            if (session.Title == "New Conversation" && history.Count == 0)
            {
                session.Title = request.Message.Length > 50 
                    ? request.Message.Substring(0, 50) + "..." 
                    : request.Message;
            }
            await _cosmosDb.UpdateSessionAsync(session);

            return Ok(new ChatResponse
            {
                Message = aiResponse,
                SessionId = session.Id,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message");
            return StatusCode(500, new { message = "An error occurred while processing your message" });
        }
    }

    [HttpGet("sessions")]
    public async Task<ActionResult<List<ChatSession>>> GetSessions()
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var sessions = await _cosmosDb.GetUserSessionsAsync(userId);
            return Ok(sessions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sessions");
            return StatusCode(500, new { message = "An error occurred while retrieving sessions" });
        }
    }

    [HttpGet("sessions/{sessionId}/messages")]
    public async Task<ActionResult<List<ChatMessage>>> GetSessionMessages(string sessionId)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var session = await _cosmosDb.GetSessionAsync(sessionId);
            if (session == null || session.UserId != userId)
            {
                return NotFound(new { message = "Session not found" });
            }

            var messages = await _cosmosDb.GetSessionMessagesAsync(sessionId);
            return Ok(messages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving messages");
            return StatusCode(500, new { message = "An error occurred while retrieving messages" });
        }
    }

    [HttpPost("sessions/{sessionId}/continue")]
    public async Task<ActionResult<ChatSession>> ContinueSession(string sessionId)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var session = await _cosmosDb.GetSessionAsync(sessionId);
            if (session == null || session.UserId != userId)
            {
                return NotFound(new { message = "Session not found" });
            }

            session.IsActive = true;
            session.UpdatedAt = DateTime.UtcNow;
            session = await _cosmosDb.UpdateSessionAsync(session);

            return Ok(session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error continuing session");
            return StatusCode(500, new { message = "An error occurred while continuing session" });
        }
    }

    [HttpPost("search")]
    public async Task<ActionResult<List<string>>> SearchDocuments([FromBody] string query)
    {
        try
        {
            var results = await _searchService.SearchDocumentsAsync(query);
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching documents");
            return StatusCode(500, new { message = "An error occurred while searching documents" });
        }
    }
}
