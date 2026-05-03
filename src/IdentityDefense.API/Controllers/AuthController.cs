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
    private readonly IRefreshTokenHashService _refreshTokenHashService;

    public AuthController(
        IUserRepository users,
        IRefreshTokenRepository refreshTokens,
        IJwtTokenService jwtTokenService,
        IRefreshTokenService refreshTokenService,
        IAuditService auditService,
        IRefreshTokenHashService refreshTokenHashService)
    {
        _users = users;
        _refreshTokens = refreshTokens;
        _jwtTokenService = jwtTokenService;
        _refreshTokenService = refreshTokenService;
        _auditService = auditService;
        _refreshTokenHashService = refreshTokenHashService;
    }

    private void SetRefreshTokenCookie(string refreshToken)
    {
        Response.Cookies.Append("zcorp_refresh_token", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = false, // em produção: true com HTTPS
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(14),
            Path = "/api/auth"
        });
    }

    private void ClearRefreshTokenCookie()
    {
        Response.Cookies.Delete("zcorp_refresh_token", new CookieOptions
        {
            Path = "/api/auth"
        });
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
            _refreshTokenHashService.Hash(refreshTokenValue),
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

        return Created("", new
        {
            user.Id,
            user.Name,
            user.Email,
            user.Role,
            user.CreatedAt
        });
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

        if (user.UnlockIfExpired())
        {
            await _users.UpdateAsync(user);
        }

        if (user.IsLockedOut())
        {
            await _auditService.LogAsync(new AuditLog(
                user.Id,
                user.Email,
                "LOGIN_LOCKED_OUT",
                "Auth",
                null,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                Request.Headers.UserAgent.ToString()
            ));

            var remainingMinutes = Math.Ceiling(
                (user.LockoutUntil!.Value - DateTime.UtcNow).TotalMinutes
            );

            return Unauthorized(new
            {
                message = $"Account temporarily locked. Try again in {remainingMinutes} minute(s)."
            });
        }

        var validPassword = BCrypt.Net.BCrypt.Verify(
            request.Password,
            user.PasswordHash
        );

        if (!validPassword)
        {
            user.RegisterFailedLogin();
            await _users.UpdateAsync(user);

            await _auditService.LogAsync(new AuditLog(
                user.Id,
                user.Email,
                user.IsLockedOut() ? "LOGIN_LOCKED_OUT" : "LOGIN_FAILED",
                "Auth",
                null,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                Request.Headers.UserAgent.ToString()
            ));

            if (user.IsLockedOut())
                return Unauthorized(new { message = "Account temporarily locked." });

            return Unauthorized(new { message = "Invalid credentials." });
        }

        var accessToken = _jwtTokenService.Generate(user);
        var refreshTokenValue = _refreshTokenService.Generate();

        var refreshToken = new RefreshToken(
            user.Id,
            _refreshTokenHashService.Hash(refreshTokenValue),
            DateTime.UtcNow.AddDays(14)
        );

        await _refreshTokens.AddAsync(refreshToken);
        SetRefreshTokenCookie(refreshTokenValue);

        await _auditService.LogAsync(new AuditLog(
            user.Id,
            user.Email,
            "LOGIN_SUCCESS",
            "Auth",
            null,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers.UserAgent.ToString()
        ));
        user.ResetLoginFailures();
        await _users.UpdateAsync(user);

        return Ok(new
        {
            userId = user.Id,
            user.Name,
            user.Email,
            user.Role,
            token = accessToken
        });
    }

    [EnableRateLimiting("auth")]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var refreshTokenFromCookie = Request.Cookies["zcorp_refresh_token"];

        if (string.IsNullOrWhiteSpace(refreshTokenFromCookie))
            return Unauthorized(new { message = "Missing refresh token." });

        var refreshTokenHash = _refreshTokenHashService.Hash(refreshTokenFromCookie);
        var existingRefreshToken = await _refreshTokens.GetByTokenAsync(refreshTokenHash);

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

        existingRefreshToken.Revoke(_refreshTokenHashService.Hash(newRefreshTokenValue));
        await _refreshTokens.UpdateAsync(existingRefreshToken);

        var newRefreshToken = new RefreshToken(
            user.Id,
            _refreshTokenHashService.Hash(newRefreshTokenValue),
            DateTime.UtcNow.AddDays(14)
        );

        await _refreshTokens.AddAsync(newRefreshToken);
        SetRefreshTokenCookie(newRefreshTokenValue);

        await _auditService.LogAsync(new AuditLog(
            user.Id,
            user.Email,
            "TOKEN_REFRESH",
            "Auth",
            null,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers.UserAgent.ToString()
        ));

        return Ok(new
        {
            userId = user.Id,
            user.Name,
            user.Email,
            user.Role,
            token = newAccessToken
        });
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

    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        ClearRefreshTokenCookie();
        return NoContent();
    }
}