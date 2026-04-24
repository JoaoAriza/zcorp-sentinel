using IdentityDefense.Application.Interfaces;
using IdentityDefense.Domain.Entities;

namespace IdentityDefense.Infrastructure.Messaging;

public class FakeIdentityRiskPublisher : IIdentityRiskPublisher
{
    public Task PublishAsync(IdentityRiskCase riskCase)
    {
        return Task.CompletedTask;
    }
}