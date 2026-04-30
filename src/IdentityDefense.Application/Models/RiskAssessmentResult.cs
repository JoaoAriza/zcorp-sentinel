namespace IdentityDefense.Application.Models;

public class RiskAssessmentResult
{
    public int Score { get; set; }
    public string Classification { get; set; } = string.Empty;
}