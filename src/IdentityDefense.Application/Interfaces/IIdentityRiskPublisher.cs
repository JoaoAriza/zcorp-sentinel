using IdentityDefense.Domain.Entities;

namespace IdentityDefense.Application.Interfaces;

public interface IIdentityRiskPublisher
{
    Task PublishAsync(IdentityRiskCase riskCase);
}