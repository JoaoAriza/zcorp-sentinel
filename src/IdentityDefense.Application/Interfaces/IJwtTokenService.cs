using IdentityDefense.Domain.Entities;

namespace IdentityDefense.Application.Interfaces;

public interface IJwtTokenService
{
    string Generate(User user);
}