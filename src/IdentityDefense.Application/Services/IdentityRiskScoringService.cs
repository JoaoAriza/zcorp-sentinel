using IdentityDefense.Application.Interfaces;
using IdentityDefense.Application.Models;

namespace IdentityDefense.Application.Services;

public class IdentityRiskScoringService : IIdentityRiskScoringService
{
    public RiskAssessmentResult Calculate(
        string source,
        string channel,
        string subject,
        List<string> detectedSignals)
    {
        var score = 0;
        var reasons = new List<string>();

        var normalizedChannel = channel.ToLower().Trim();
        var normalizedSource = source.ToLower().Trim();
        var normalizedSubject = subject.ToLower().Trim();
        var normalizedSignals = detectedSignals
            .Select(signal => signal.ToLower().Trim())
            .ToList();

        if (normalizedChannel == "voice")
        {
            score += 25;
            reasons.Add("Voice channel increases impersonation risk (+25)");
        }

        if (normalizedChannel == "video")
        {
            score += 30;
            reasons.Add("Video channel increases deepfake risk (+30)");
        }

        if (normalizedChannel == "chat")
        {
            score += 15;
            reasons.Add("Chat channel has moderate social engineering exposure (+15)");
        }

        foreach (var signal in normalizedSignals)
        {
            switch (signal)
            {
                case "voice_clone":
                    score += 30;
                    reasons.Add("Voice clone indicator detected (+30)");
                    break;

                case "face_mismatch":
                    score += 25;
                    reasons.Add("Face mismatch indicator detected (+25)");
                    break;

                case "synthetic_identity":
                    score += 35;
                    reasons.Add("Synthetic identity indicator detected (+35)");
                    break;

                case "urgent_language":
                    score += 15;
                    reasons.Add("Urgent language pattern detected (+15)");
                    break;

                case "behavior_anomaly":
                    score += 20;
                    reasons.Add("Behavior anomaly detected (+20)");
                    break;

                case "executive_impersonation":
                    score += 35;
                    reasons.Add("Executive impersonation indicator detected (+35)");
                    break;

                default:
                    score += 5;
                    reasons.Add($"Unknown anomaly signal '{signal}' detected (+5)");
                    break;
            }
        }

        if (
            normalizedSignals.Contains("executive_impersonation") &&
            normalizedSignals.Contains("urgent_language"))
        {
            score += 20;
            reasons.Add("Executive impersonation combined with urgency pattern (+20)");
        }

        if (
            normalizedSignals.Contains("voice_clone") &&
            normalizedChannel == "voice")
        {
            score += 15;
            reasons.Add("Voice clone detected on voice channel (+15)");
        }

        if (
            normalizedSignals.Contains("face_mismatch") &&
            normalizedChannel == "video")
        {
            score += 15;
            reasons.Add("Face mismatch detected on video channel (+15)");
        }

        if (
            normalizedSubject.Contains("ceo") ||
            normalizedSubject.Contains("wire transfer") ||
            normalizedSubject.Contains("approval") ||
            normalizedSubject.Contains("payment"))
        {
            score += 15;
            reasons.Add("High-value business context detected (+15)");
        }

        if (
            normalizedSource.Contains("finance") ||
            normalizedSource.Contains("executive") ||
            normalizedSource.Contains("banking"))
        {
            score += 10;
            reasons.Add("Sensitive source context detected (+10)");
        }

        score = Math.Clamp(score, 0, 100);

        var classification =
            score >= 80 ? "Critical" :
            score >= 60 ? "High" :
            score >= 30 ? "Medium" :
            "Low";

        return new RiskAssessmentResult
        {
            Score = score,
            Classification = classification,
            Reasons = reasons
        };
    }
}