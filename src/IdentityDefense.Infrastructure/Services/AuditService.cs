using IdentityDefense.Application.Interfaces;
using IdentityDefense.Domain.Entities;
using IdentityDefense.Infrastructure.Persistence;

namespace IdentityDefense.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly IdentityDefenseDbContext _context;

    public AuditService(IdentityDefenseDbContext context)
    {
        _context = context;
    }

    public async Task LogAsync(AuditLog log)
    {
        await _context.AuditLogs.AddAsync(log);
        await _context.SaveChangesAsync();
    }
}