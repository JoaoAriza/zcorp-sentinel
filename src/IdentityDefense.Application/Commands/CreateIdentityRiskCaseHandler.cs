using IdentityDefense.Application.Interfaces;
using IdentityDefense.Domain.Entities;

namespace IdentityDefense.Application.Commands;

public class CreateIdentityRiskCaseHandler
{
    private readonly IIdentityRiskPublisher _publisher;
    private readonly IIdentityRiskCaseRepository _repository;

    public CreateIdentityRiskCaseHandler(
        IIdentityRiskPublisher publisher,
        IIdentityRiskCaseRepository repository)
    {
        _publisher = publisher;
        _repository = repository;
    }

    public async Task<IdentityRiskCase> Handle(CreateIdentityRiskCaseCommand command)
    {
        var riskCase = new IdentityRiskCase(
            command.Source,
            command.Channel,
            command.Subject,
            command.DetectedSignals
        );

        await _repository.AddAsync(riskCase);
        await _publisher.PublishAsync(riskCase);

        return riskCase;
    }
}