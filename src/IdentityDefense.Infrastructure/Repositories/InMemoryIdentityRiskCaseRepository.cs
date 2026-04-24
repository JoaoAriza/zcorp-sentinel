using IdentityDefense.Application.Interfaces;
using IdentityDefense.Domain.Entities;

namespace IdentityDefense.Infrastructure.Repositories;

public class InMemoryIdentityRiskCaseRepository : IIdentityRiskCaseRepository
{
    private static readonly List<IdentityRiskCase> Cases = [];

    public Task AddAsync(IdentityRiskCase riskCase)
    {
        Cases.Add(riskCase);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<IdentityRiskCase>> GetAllAsync()
    {
        return Task.FromResult((IReadOnlyList<IdentityRiskCase>)Cases
            .OrderByDescending(x => x.CreatedAt)
            .ToList());
    }

    public Task<IdentityRiskCase?> GetByIdAsync(Guid id)
    {
        var riskCase = Cases.FirstOrDefault(x => x.Id == id);
        return Task.FromResult(riskCase);
    }
}