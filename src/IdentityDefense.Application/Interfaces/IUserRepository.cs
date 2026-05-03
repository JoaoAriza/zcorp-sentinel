using IdentityDefense.Domain.Entities;

namespace IdentityDefense.Application.Interfaces;

public interface IUserRepository
{
    Task AddAsync(User user);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<User>> GetAllAsync();
    Task DeleteAsync(User user);
    Task UpdateAsync(User user);
}