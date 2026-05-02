using System.Security.Cryptography;
using System.Text;
using IdentityDefense.Application.Interfaces;

namespace IdentityDefense.Infrastructure.Services;

public class RefreshTokenHashService : IRefreshTokenHashService
{
    public string Hash(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }
}