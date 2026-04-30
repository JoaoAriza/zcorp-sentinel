using IdentityDefense.Application.Commands;
using IdentityDefense.Application.Interfaces;
using IdentityDefense.Infrastructure.Messaging;
using IdentityDefense.Infrastructure.Repositories;
using IdentityDefense.Infrastructure.Services;
using IdentityDefense.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Text;
using IdentityDefense.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using IdentityDefense.API.Hubs;
using IdentityDefense.API.Services;
using IdentityDefense.Application.Services;

var builder = WebApplication.CreateBuilder(args);
const string CorsPolicy = "FrontendPolicy";

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var issuer = builder.Configuration["Jwt:Issuer"];
        var audience = builder.Configuration["Jwt:Audience"];
        var secret = builder.Configuration["Jwt:Secret"];

        if (string.IsNullOrWhiteSpace(secret))
            throw new InvalidOperationException("JWT secret is not configured.");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddDbContext<IdentityDefenseDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres"));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddScoped<CreateIdentityRiskCaseHandler>();
builder.Services.AddScoped<IIdentityRiskPublisher, FakeIdentityRiskPublisher>();
builder.Services.AddScoped<IIdentityRiskCaseRepository, PostgresIdentityRiskCaseRepository>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IUserRepository, PostgresUserRepository>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IRefreshTokenRepository, PostgresRefreshTokenRepository>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<IIncidentRealtimeNotifier, SignalRIncidentRealtimeNotifier>();
builder.Services.AddScoped<IIdentityRiskScoringService, IdentityRiskScoringService>();
builder.Services.AddScoped<IAuditService, AuditService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors(CorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

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
app.MapHub<IncidentHub>("/hubs/incidents");

app.Run();