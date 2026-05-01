using IdentityDefense.Domain.Entities;

namespace IdentityDefense.Tests;

public class IdentityRiskCaseTests
{
    [Fact]
    public void Should_Create_Risk_Case_With_Correct_Classification()
    {
        var signals = new List<string>
        {
            "voice_clone",
            "executive_impersonation"
        };

        var reasons = new List<string>
        {
            "Voice clone indicator detected (+30)",
            "Executive impersonation indicator detected (+35)"
        };

        var riskCase = new IdentityRiskCase(
            "mobile-banking",
            "voice",
            "CEO approval call",
            signals,
            75,
            "High",
            reasons
        );

        Assert.Equal("High", riskCase.Classification);
        Assert.Equal(75, riskCase.RiskScore);
        Assert.NotEmpty(riskCase.RiskReasons);
    }
}