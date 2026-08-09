using System.Security.Claims;
using AuthService.Data;
using AuthService.DTOs;
using AuthService.Models;
using AuthService.Services;
using BITask.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthDbContext _db;
    private readonly ITokenService _tokenService;
    private readonly PasswordHasher<User> _passwordHasher = new();
    private readonly ILogger<AuthController> _logger;

    public AuthController(AuthDbContext db, ITokenService tokenService, ILogger<AuthController> logger)
    {
        _db = db;
        _tokenService = tokenService;
        _logger = logger;
    }

    /// <summary>Registers a new user with the "User" role.</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserResponse>> Register(RegisterRequest request)
    {
        var exists = await _db.Users.AnyAsync(u => u.Username == request.Username || u.Email == request.Email);
        if (exists)
        {
            return Conflict(new { message = "Username or email is already registered." });
        }

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            Role = Roles.User
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        _logger.LogInformation("New user registered: {Username}", user.Username);

        var response = new UserResponse { Id = user.Id, Username = user.Username, Email = user.Email, Role = user.Role };
        return CreatedAtAction(nameof(Me), new { }, response);
    }

    /// <summary>Authenticates a user and issues a JWT access token.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await _db.Users.SingleOrDefaultAsync(u => u.Username == request.Username);
        if (user is null)
        {
            return Unauthorized(new { message = "Invalid username or password." });
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new { message = "Invalid username or password." });
        }

        var (token, expiresAtUtc) = _tokenService.GenerateToken(user);

        return Ok(new AuthResponse
        {
            Token = token,
            ExpiresAtUtc = expiresAtUtc,
            Username = user.Username,
            Role = user.Role
        });
    }

    /// <summary>Returns the profile of the currently authenticated user.</summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserResponse>> Me()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _db.Users.FindAsync(userId);
        if (user is null)
        {
            return NotFound();
        }

        return Ok(new UserResponse { Id = user.Id, Username = user.Username, Email = user.Email, Role = user.Role });
    }
}
