using IdentityDefense.Application.DTOs.Auth;
using IdentityDefense.Application.Interfaces;
using IdentityDefense.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace IdentityDefense.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _users;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthController(
        IUserRepository users,
        IJwtTokenService jwtTokenService)
    {
        _users = users;
        _jwtTokenService = jwtTokenService;
    }

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

        var token = _jwtTokenService.Generate(user);

        return Created("", new AuthResponse(
            user.Id,
            user.Name,
            user.Email,
            user.Role,
            token
        ));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _users.GetByEmailAsync(request.Email);

        if (user is null)
            return Unauthorized(new { message = "Invalid credentials." });

        var validPassword = BCrypt.Net.BCrypt.Verify(
            request.Password,
            user.PasswordHash
        );

        if (!validPassword)
            return Unauthorized(new { message = "Invalid credentials." });

        var token = _jwtTokenService.Generate(user);

        return Ok(new AuthResponse(
            user.Id,
            user.Name,
            user.Email,
            user.Role,
            token
        ));
    }
}