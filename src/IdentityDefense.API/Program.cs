using Confluent.Kafka;
using IdentityDefense.Application.Commands;
using IdentityDefense.Application.Interfaces;
using IdentityDefense.Infrastructure.Messaging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddScoped<CreateIdentityRiskCaseHandler>();

builder.Services.AddSingleton<IProducer<Null, string>>(_ =>
{
    var config = new ProducerConfig
    {
        BootstrapServers = "localhost:9092"
    };

    return new ProducerBuilder<Null, string>(config).Build();
});

builder.Services.AddScoped<IIdentityRiskPublisher, KafkaIdentityRiskPublisher>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();