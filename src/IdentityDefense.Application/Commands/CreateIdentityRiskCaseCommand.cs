namespace IdentityDefense.Application.Commands;

public record CreateIdentityRiskCaseCommand(
    string Source,
    string Channel,
    string Subject,
    List<string> DetectedSignals
);