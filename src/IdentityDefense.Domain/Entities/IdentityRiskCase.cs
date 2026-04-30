namespace IdentityDefense.Domain.Entities;

public class IdentityRiskCase
{
    public Guid Id { get; private set; }
    public string Source { get; private set; } = string.Empty;
    public string Channel { get; private set; } = string.Empty;
    public string Subject { get; private set; } = string.Empty;
    public List<string> DetectedSignals { get; private set; } = new();
    public int RiskScore { get; private set; }
    public string Classification { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    private IdentityRiskCase()
    {
    }

    public IdentityRiskCase(
        string source,
        string channel,
        string subject,
        List<string> detectedSignals,
        int riskScore,
        string classification)
    {
        Id = Guid.NewGuid();
        Source = source;
        Channel = channel;
        Subject = subject;
        DetectedSignals = detectedSignals ?? new List<string>();
        RiskScore = Math.Clamp(riskScore, 0, 100);
        Classification = classification;
        CreatedAt = DateTime.UtcNow;

        Validate();
    }

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(Source))
            throw new ArgumentException("Source is required.");

        if (string.IsNullOrWhiteSpace(Channel))
            throw new ArgumentException("Channel is required.");

        if (string.IsNullOrWhiteSpace(Subject))
            throw new ArgumentException("Subject is required.");

        if (string.IsNullOrWhiteSpace(Classification))
            throw new ArgumentException("Classification is required.");

        if (Classification is not "Low" and not "Medium" and not "High" and not "Critical")
            throw new ArgumentException("Invalid classification.");
    }
}