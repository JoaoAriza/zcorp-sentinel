using IdentityDefense.Application.Interfaces;
using IdentityDefense.Domain.Entities;
using IdentityDefense.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IdentityDefense.Infrastructure.Repositories;

public class PostgresRefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IdentityDefenseDbContext _context;

    public PostgresRefreshTokenRepository(IdentityDefenseDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(RefreshToken refreshToken)
    {
        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await _context.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == token);
    }

    public async Task UpdateAsync(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Update(refreshToken);
        await _context.SaveChangesAsync();
    }
}