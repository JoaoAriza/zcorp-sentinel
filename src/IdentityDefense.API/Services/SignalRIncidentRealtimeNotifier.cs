using IdentityDefense.API.Hubs;
using IdentityDefense.Application.Interfaces;
using IdentityDefense.Domain.Entities;
using Microsoft.AspNetCore.SignalR;

namespace IdentityDefense.API.Services;

public class SignalRIncidentRealtimeNotifier : IIncidentRealtimeNotifier
{
    private readonly IHubContext<IncidentHub> _hubContext;

    public SignalRIncidentRealtimeNotifier(IHubContext<IncidentHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyIncidentCreatedAsync(IdentityRiskCase riskCase)
    {
        await _hubContext.Clients.All.SendAsync("incident-created", new
        {
            riskCase.Id,
            riskCase.Source,
            riskCase.Channel,
            riskCase.Subject,
            riskCase.RiskScore,
            riskCase.Classification,
            riskCase.DetectedSignals,
            riskCase.RiskReasons,
            riskCase.CreatedAt
        });
    }
}