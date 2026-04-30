using IdentityDefense.Domain.Entities;

namespace IdentityDefense.Application.Interfaces;

public interface IAuditService
{
    Task LogAsync(AuditLog log);
}