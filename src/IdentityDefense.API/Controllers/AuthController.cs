using System.Security.Claims;
using IdentityDefense.Application.DTOs.Auth;
using IdentityDefense.Application.Interfaces;
using IdentityDefense.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace IdentityDefense.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _users;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IAuditService _auditService;

    public AuthController(
        IUserRepository users,
        IRefreshTokenRepository refreshTokens,
        IJwtTokenService jwtTokenService,
        IRefreshTokenService refreshTokenService,
        IAuditService auditService)
    {
        _users = users;
        _refreshTokens = refreshTokens;
        _jwtTokenService = jwtTokenService;
        _refreshTokenService = refreshTokenService;
        _auditService = auditService;
    }

    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("auth")]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var existingUser = await _users.GetByEmailAsync(request.Email);

        if (existingUser is not null)
            return Conflict(new { message = "Email already registered." });

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User(
            request.Name,
            request.Email,
            passwordHash,
            request.Role
        );

        await _users.AddAsync(user);

        var accessToken = _jwtTokenService.Generate(user);
        var refreshTokenValue = _refreshTokenService.Generate();

        var refreshToken = new RefreshToken(
            user.Id,
            refreshTokenValue,
            DateTime.UtcNow.AddDays(14)
        );

        await _refreshTokens.AddAsync(refreshToken);

        await _auditService.LogAsync(new AuditLog(
            user.Id,
            user.Email,
            "REGISTER_SUCCESS",
            "Auth",
            user.Id.ToString(),
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers.UserAgent.ToString()
        ));

        return Created("", new AuthResponse(
            user.Id,
            user.Name,
            user.Email,
            user.Role,
            accessToken,
            refreshTokenValue
        ));
    }

    [EnableRateLimiting("auth")]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _users.GetByEmailAsync(request.Email);

        if (user is null)
        {
            await _auditService.LogAsync(new AuditLog(
                null,
                request.Email,
                "LOGIN_FAILED",
                "Auth",
                null,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                Request.Headers.UserAgent.ToString()
            ));

            return Unauthorized(new { message = "Invalid credentials." });
        }

        var validPassword = BCrypt.Net.BCrypt.Verify(
            request.Password,
            user.PasswordHash
        );

        if (!validPassword)
        {
            await _auditService.LogAsync(new AuditLog(
                user.Id,
                user.Email,
                "LOGIN_FAILED",
                "Auth",
                null,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                Request.Headers.UserAgent.ToString()
            ));

            return Unauthorized(new { message = "Invalid credentials." });
        }

        var accessToken = _jwtTokenService.Generate(user);
        var refreshTokenValue = _refreshTokenService.Generate();

        var refreshToken = new RefreshToken(
            user.Id,
            refreshTokenValue,
            DateTime.UtcNow.AddDays(14)
        );

        await _refreshTokens.AddAsync(refreshToken);

        await _auditService.LogAsync(new AuditLog(
            user.Id,
            user.Email,
            "LOGIN_SUCCESS",
            "Auth",
            null,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers.UserAgent.ToString()
        ));

        return Ok(new AuthResponse(
            user.Id,
            user.Name,
            user.Email,
            user.Role,
            accessToken,
            refreshTokenValue
        ));
    }

    [EnableRateLimiting("auth")]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        var existingRefreshToken = await _refreshTokens.GetByTokenAsync(request.RefreshToken);

        if (existingRefreshToken is null || !existingRefreshToken.IsActive)
        {
            await _auditService.LogAsync(new AuditLog(
                null,
                null,
                "TOKEN_REFRESH_FAILED",
                "Auth",
                null,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                Request.Headers.UserAgent.ToString()
            ));

            return Unauthorized(new { message = "Invalid refresh token." });
        }

        var user = await _users.GetByIdAsync(existingRefreshToken.UserId);

        if (user is null)
            return Unauthorized(new { message = "User not found." });

        var newAccessToken = _jwtTokenService.Generate(user);
        var newRefreshTokenValue = _refreshTokenService.Generate();

        existingRefreshToken.Revoke(newRefreshTokenValue);
        await _refreshTokens.UpdateAsync(existingRefreshToken);

        var newRefreshToken = new RefreshToken(
            user.Id,
            newRefreshTokenValue,
            DateTime.UtcNow.AddDays(14)
        );

        await _refreshTokens.AddAsync(newRefreshToken);

        await _auditService.LogAsync(new AuditLog(
            user.Id,
            user.Email,
            "TOKEN_REFRESH",
            "Auth",
            null,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers.UserAgent.ToString()
        ));

        return Ok(new AuthResponse(
            user.Id,
            user.Name,
            user.Email,
            user.Role,
            newAccessToken,
            newRefreshTokenValue
        ));
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var name = User.FindFirst(ClaimTypes.Name)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        return Ok(new
        {
            userId,
            name,
            email,
            role
        });
    }
}