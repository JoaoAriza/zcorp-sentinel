using System.Security.Cryptography;
using IdentityDefense.Application.Interfaces;

namespace IdentityDefense.Infrastructure.Security;

public class RefreshTokenService : IRefreshTokenService
{
    public string Generate()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }
}