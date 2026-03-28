using IdentityDefense.Application.Interfaces;
using IdentityDefense.Domain.Entities;

namespace IdentityDefense.Application.Commands;

public class CreateIdentityRiskCaseHandler
{
    private readonly IIdentityRiskPublisher _publisher;

    public CreateIdentityRiskCaseHandler(IIdentityRiskPublisher publisher)
    {
        _publisher = publisher;
    }

    public async Task<IdentityRiskCase> Handle(CreateIdentityRiskCaseCommand command)
    {
        var riskCase = new IdentityRiskCase(
            command.Source,
            command.Channel,
            command.Subject,
            command.DetectedSignals
        );

        await _publisher.PublishAsync(riskCase);

        return riskCase;
    }
}