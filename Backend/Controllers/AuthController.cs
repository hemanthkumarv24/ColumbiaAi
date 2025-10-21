using Microsoft.AspNetCore.Mvc;
using ColumbiaAi.Backend.Models;
using ColumbiaAi.Backend.Services;

namespace ColumbiaAi.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ICosmosDbService _cosmosDb;
    private readonly IJwtService _jwtService;
    private readonly IPasswordService _passwordService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        ICosmosDbService cosmosDb,
        IJwtService jwtService,
        IPasswordService passwordService,
        ILogger<AuthController> logger)
    {
        _cosmosDb = cosmosDb;
        _jwtService = jwtService;
        _passwordService = passwordService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var existingUser = await _cosmosDb.GetUserByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "User already exists" });
            }

            var user = new User
            {
                Email = request.Email,
                Name = request.Name,
                PasswordHash = _passwordService.HashPassword(request.Password)
            };

            user = await _cosmosDb.CreateUserAsync(user);

            var token = _jwtService.GenerateToken(user);

            return Ok(new AuthResponse
            {
                Token = token,
                UserId = user.Id,
                Name = user.Name,
                Email = user.Email
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return StatusCode(500, new { message = "An error occurred during registration" });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var user = await _cosmosDb.GetUserByEmailAsync(request.Email);
            if (user == null || !_passwordService.VerifyPassword(request.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            var token = _jwtService.GenerateToken(user);

            return Ok(new AuthResponse
            {
                Token = token,
                UserId = user.Id,
                Name = user.Name,
                Email = user.Email
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, new { message = "An error occurred during login" });
        }
    }
}
