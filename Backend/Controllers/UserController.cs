using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ColumbiaAi.Backend.Models;
using ColumbiaAi.Backend.Services;

namespace ColumbiaAi.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly ICosmosDbService _cosmosDb;
    private readonly ILogger<UserController> _logger;

    public UserController(ICosmosDbService cosmosDb, ILogger<UserController> logger)
    {
        _cosmosDb = cosmosDb;
        _logger = logger;
    }

    private string GetUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
    }

    [HttpGet("profile")]
    public async Task<ActionResult<User>> GetProfile()
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _cosmosDb.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            user.PasswordHash = string.Empty;
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving profile");
            return StatusCode(500, new { message = "An error occurred while retrieving profile" });
        }
    }

    [HttpPut("profile")]
    public async Task<ActionResult<User>> UpdateProfile([FromBody] UserProfile profile)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _cosmosDb.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            user.Profile = profile;
            user = await _cosmosDb.UpdateUserAsync(user);
            user.PasswordHash = string.Empty;

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile");
            return StatusCode(500, new { message = "An error occurred while updating profile" });
        }
    }
}
