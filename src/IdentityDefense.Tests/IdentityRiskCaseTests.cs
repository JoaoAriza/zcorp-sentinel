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

        var riskCase = new IdentityRiskCase(
            "mobile-banking",
            "voice",
            "CEO approval call",
            signals
        );

        Assert.Equal("High", riskCase.Classification);
        Assert.True(riskCase.RiskScore > 0);
    }
}