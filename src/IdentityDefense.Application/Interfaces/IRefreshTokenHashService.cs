namespace IdentityDefense.Application.Interfaces;

public interface IRefreshTokenHashService
{
    string Hash(string token);
}