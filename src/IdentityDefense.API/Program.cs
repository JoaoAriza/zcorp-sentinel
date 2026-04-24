using IdentityDefense.Application.Commands;
using IdentityDefense.Application.Interfaces;
using IdentityDefense.Infrastructure.Messaging;
using IdentityDefense.Infrastructure.Repositories;
using IdentityDefense.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);
const string CorsPolicy = "FrontendPolicy";

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddScoped<CreateIdentityRiskCaseHandler>();
builder.Services.AddScoped<IIdentityRiskPublisher, FakeIdentityRiskPublisher>();
builder.Services.AddSingleton<IIdentityRiskCaseRepository, InMemoryIdentityRiskCaseRepository>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors(CorsPolicy);

app.MapGet("/", () => Results.Ok(new
{
    name = "Identity Defense API",
    status = "running"
}));

app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy"
}));

app.MapControllers();

app.Run();