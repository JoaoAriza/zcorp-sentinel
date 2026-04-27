using IdentityDefense.Application.Interfaces;
using IdentityDefense.Domain.Entities;
using IdentityDefense.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IdentityDefense.Infrastructure.Repositories;

public class PostgresUserRepository : IUserRepository
{
    private readonly IdentityDefenseDbContext _context;

    public PostgresUserRepository(IdentityDefenseDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var normalizedEmail = email.ToLower().Trim();

        return await _context.Users
            .FirstOrDefaultAsync(x => x.Email == normalizedEmail);
    }
}