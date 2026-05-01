namespace IdentityDefense.Application.DTOs.Risk;

public record AnalyzeRiskResponse(
    Guid Id,
    string Source,
    string Channel,
    string Subject,
    int RiskScore,
    string Classification,
    List<string> DetectedSignals,
    List<string> RiskReasons,
    DateTime CreatedAt
);