using IdentityDefense.Application.DTOs;

namespace IdentityDefense.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync();
}