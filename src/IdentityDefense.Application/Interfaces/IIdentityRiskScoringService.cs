using IdentityDefense.Application.Models;

namespace IdentityDefense.Application.Interfaces;

public interface IIdentityRiskScoringService
{
    RiskAssessmentResult Calculate(
        string source,
        string channel,
        string subject,
        List<string> detectedSignals
    );
}