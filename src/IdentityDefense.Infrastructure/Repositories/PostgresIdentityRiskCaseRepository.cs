using IdentityDefense.Application.Interfaces;
using IdentityDefense.Domain.Entities;
using IdentityDefense.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IdentityDefense.Infrastructure.Repositories;

public class PostgresIdentityRiskCaseRepository : IIdentityRiskCaseRepository
{
    private readonly IdentityDefenseDbContext _context;

    public PostgresIdentityRiskCaseRepository(IdentityDefenseDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(IdentityRiskCase riskCase)
    {
        await _context.IdentityRiskCases.AddAsync(riskCase);
        await _context.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<IdentityRiskCase>> GetAllAsync()
    {
        return await _context.IdentityRiskCases
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<IdentityRiskCase?> GetByIdAsync(Guid id)
    {
        return await _context.IdentityRiskCases
            .FirstOrDefaultAsync(x => x.Id == id);
    }
}