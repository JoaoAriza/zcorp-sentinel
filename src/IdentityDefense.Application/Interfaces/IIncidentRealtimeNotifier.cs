using IdentityDefense.Domain.Entities;

namespace IdentityDefense.Application.Interfaces;

public interface IIncidentRealtimeNotifier
{
    Task NotifyIncidentCreatedAsync(IdentityRiskCase riskCase);
}