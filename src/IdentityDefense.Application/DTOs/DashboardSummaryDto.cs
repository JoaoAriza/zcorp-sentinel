namespace IdentityDefense.Application.DTOs;

public class DashboardSummaryDto
{
    public int TotalCases { get; set; }
    public int LowCases { get; set; }
    public int MediumCases { get; set; }
    public int HighCases { get; set; }
    public int CriticalCases { get; set; }
    public double AverageRiskScore { get; set; }
    public Dictionary<string, int> CasesByChannel { get; set; } = [];
    public List<RecentRiskCaseDto> RecentCases { get; set; } = [];
}

public class RecentRiskCaseDto
{
    public Guid Id { get; set; }
    public string Source { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public int RiskScore { get; set; }
    public string Classification { get; set; } = string.Empty;
    public List<string> DetectedSignals { get; set; } = new();
    public List<string> RiskReasons { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}