namespace IdentityDefense.Application.DTOs.Risk;

public record AnalyzeRiskRequest(
    string Source,
    string Channel,
    string Subject,
    List<string> DetectedSignals
);