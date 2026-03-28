namespace IdentityDefense.Domain.Entities;

public class IdentityRiskCase
{
    public Guid Id { get; private set; }
    public string Source { get; private set; }
    public string Channel { get; private set; }
    public string Subject { get; private set; }
    public List<string> DetectedSignals { get; private set; }
    public int RiskScore { get; private set; }
    public string Classification { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public IdentityRiskCase(
        string source,
        string channel,
        string subject,
        List<string> detectedSignals)
    {
        Id = Guid.NewGuid();
        Source = source;
        Channel = channel;
        Subject = subject;
        DetectedSignals = detectedSignals ?? new List<string>();
        CreatedAt = DateTime.UtcNow;

        Validate();
        CalculateRisk();
    }

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(Source))
            throw new ArgumentException("Source is required.");

        if (string.IsNullOrWhiteSpace(Channel))
            throw new ArgumentException("Channel is required.");

        if (string.IsNullOrWhiteSpace(Subject))
            throw new ArgumentException("Subject is required.");
    }

    private void CalculateRisk()
    {
        var score = 0;

        foreach (var signal in DetectedSignals)
        {
            score += signal.ToLower() switch
            {
                "voice_clone" => 30,
                "face_mismatch" => 25,
                "executive_impersonation" => 35,
                "synthetic_identity" => 40,
                "behavior_anomaly" => 20,
                "urgent_language" => 10,
                _ => 5
            };
        }

        RiskScore = Math.Min(score, 100);

        Classification = RiskScore switch
        {
            <= 20 => "Low",
            <= 50 => "Medium",
            <= 80 => "High",
            _ => "Critical"
        };
    }
}