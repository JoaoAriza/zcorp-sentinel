using IdentityDefense.Domain.Entities;

namespace IdentityDefense.Application.Interfaces;

public interface IIdentityRiskCaseRepository
{
    Task AddAsync(IdentityRiskCase riskCase);
    Task<IReadOnlyList<IdentityRiskCase>> GetAllAsync();
    Task<IdentityRiskCase?> GetByIdAsync(Guid id);
}