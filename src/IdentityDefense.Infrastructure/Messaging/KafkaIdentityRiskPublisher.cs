using System.Text.Json;
using Confluent.Kafka;
using IdentityDefense.Application.Interfaces;
using IdentityDefense.Domain.Entities;

namespace IdentityDefense.Infrastructure.Messaging;

public class KafkaIdentityRiskPublisher : IIdentityRiskPublisher
{
    private readonly IProducer<Null, string> _producer;
    private const string Topic = "identity-risk-cases";

    public KafkaIdentityRiskPublisher(IProducer<Null, string> producer)
    {
        _producer = producer;
    }

    public async Task PublishAsync(IdentityRiskCase riskCase)
    {
        var payload = JsonSerializer.Serialize(riskCase);

        await _producer.ProduceAsync(Topic, new Message<Null, string>
        {
            Value = payload
        });
    }
}