using IdentityDefense.Application.Interfaces;
using IdentityDefense.Domain.Entities;

namespace IdentityDefense.Application.Commands;

public class CreateIdentityRiskCaseHandler
{
    private readonly IIdentityRiskPublisher _publisher;
    private readonly IIdentityRiskCaseRepository _repository;
    private readonly IIdentityRiskScoringService _scoringService;
    private readonly IIncidentRealtimeNotifier _notifier;

    public CreateIdentityRiskCaseHandler(
        IIdentityRiskPublisher publisher,
        IIdentityRiskCaseRepository repository,
        IIdentityRiskScoringService scoringService,
        IIncidentRealtimeNotifier notifier)
    {
        _publisher = publisher;
        _repository = repository;
        _scoringService = scoringService;
        _notifier = notifier;
    }

    public async Task<IdentityRiskCase> Handle(CreateIdentityRiskCaseCommand command)
    {
        var assessment = _scoringService.Calculate(
            command.Source,
            command.Channel,
            command.Subject,
            command.DetectedSignals
        );

        var riskCase = new IdentityRiskCase(
            command.Source,
            command.Channel,
            command.Subject,
            command.DetectedSignals,
            assessment.Score,
            assessment.Classification,
            assessment.Reasons
        );

        await _repository.AddAsync(riskCase);
        await _publisher.PublishAsync(riskCase);
        await _notifier.NotifyIncidentCreatedAsync(riskCase);

        return riskCase;
    }
}