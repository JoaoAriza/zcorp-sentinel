using IdentityDefense.Application.DTOs;
using IdentityDefense.Application.Interfaces;

namespace IdentityDefense.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly IIdentityRiskCaseRepository _repository;

    public DashboardService(IIdentityRiskCaseRepository repository)
    {
        _repository = repository;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync()
    {
        var cases = await _repository.GetAllAsync();

        var summary = new DashboardSummaryDto
        {
            TotalCases = cases.Count,
            LowCases = cases.Count(x => x.Classification == "Low"),
            MediumCases = cases.Count(x => x.Classification == "Medium"),
            HighCases = cases.Count(x => x.Classification == "High"),
            CriticalCases = cases.Count(x => x.Classification == "Critical"),
            AverageRiskScore = cases.Count == 0 ? 0 : Math.Round(cases.Average(x => x.RiskScore), 2),
            CasesByChannel = cases
                .GroupBy(x => x.Channel)
                .ToDictionary(g => g.Key, g => g.Count()),
            RecentCases = cases
                .OrderByDescending(x => x.CreatedAt)
                .Take(5)
                .Select(x => new RecentRiskCaseDto
                {
                    Id = x.Id,
                    Source = x.Source,
                    Channel = x.Channel,
                    Subject = x.Subject,
                    RiskScore = x.RiskScore,
                    Classification = x.Classification,
                    DetectedSignals = x.DetectedSignals,
                    RiskReasons = x.RiskReasons,
                    CreatedAt = x.CreatedAt
                })
                .ToList()
        };

        return summary;
    }
}