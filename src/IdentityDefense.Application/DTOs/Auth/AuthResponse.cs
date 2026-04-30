namespace IdentityDefense.Application.DTOs.Auth;

public record AuthResponse(
    Guid UserId,
    string Name,
    string Email,
    string Role,
    string Token,
    string RefreshToken
);