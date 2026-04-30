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
        int score = 0;

        if (channel == "voice") score += 25;
        if (channel == "video") score += 30;
        if (channel == "chat") score += 15;

        foreach (var signal in detectedSignals)
        {
            switch (signal)
            {
                case "voice_clone":
                    score += 30;
                    break;

                case "face_mismatch":
                    score += 25;
                    break;

                case "synthetic_identity":
                    score += 35;
                    break;

                case "urgent_language":
                    score += 15;
                    break;

                case "behavior_anomaly":
                    score += 20;
                    break;
            }
        }

        score = Math.Min(score, 100);

        var classification =
            score >= 80 ? "Critical" :
            score >= 60 ? "High" :
            score >= 30 ? "Medium" :
            "Low";

        return new RiskAssessmentResult
        {
            Score = score,
            Classification = classification
        };
    }
}