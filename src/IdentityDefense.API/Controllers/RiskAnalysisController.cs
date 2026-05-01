using System.Security.Claims;
using IdentityDefense.Application.DTOs.Risk;
using IdentityDefense.Application.Interfaces;
using IdentityDefense.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityDefense.API.Controllers;

[Authorize(Roles = "Admin,Analyst")]
[ApiController]
[Route("api/risk")]
public class RiskAnalysisController : ControllerBase
{
    private readonly IIdentityRiskScoringService _scoringService;
    private readonly IIdentityRiskCaseRepository _riskCases;
    private readonly IIdentityRiskPublisher _publisher;
    private readonly IIncidentRealtimeNotifier _notifier;
    private readonly IAuditService _auditService;

    public RiskAnalysisController(
        IIdentityRiskScoringService scoringService,
        IIdentityRiskCaseRepository riskCases,
        IIdentityRiskPublisher publisher,
        IIncidentRealtimeNotifier notifier,
        IAuditService auditService)
    {
        _scoringService = scoringService;
        _riskCases = riskCases;
        _publisher = publisher;
        _notifier = notifier;
        _auditService = auditService;
    }

    [HttpPost("analyze")]
    public async Task<IActionResult> Analyze([FromBody] AnalyzeRiskRequest request)
    {
        var assessment = _scoringService.Calculate(
            request.Source,
            request.Channel,
            request.Subject,
            request.DetectedSignals
        );

        var riskCase = new IdentityRiskCase(
            request.Source,
            request.Channel,
            request.Subject,
            request.DetectedSignals,
            assessment.Score,
            assessment.Classification,
            assessment.Reasons
        );

        await _riskCases.AddAsync(riskCase);
        await _publisher.PublishAsync(riskCase);
        await _notifier.NotifyIncidentCreatedAsync(riskCase);

        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Guid? userId = Guid.TryParse(userIdValue, out var parsedUserId)
            ? parsedUserId
            : null;

        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;

        await _auditService.LogAsync(new AuditLog(
            userId,
            userEmail,
            "RISK_ANALYZED",
            "IdentityRiskCase",
            riskCase.Id.ToString(),
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers.UserAgent.ToString()
        ));

        return Ok(new AnalyzeRiskResponse(
            riskCase.Id,
            riskCase.Source,
            riskCase.Channel,
            riskCase.Subject,
            riskCase.RiskScore,
            riskCase.Classification,
            riskCase.DetectedSignals,
            riskCase.RiskReasons,
            riskCase.CreatedAt
        ));
    }
}