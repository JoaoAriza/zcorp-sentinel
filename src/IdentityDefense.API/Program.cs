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
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Serilog;
using IdentityDefense.Domain.Entities;
using IdentityDefense.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File(
            "logs/zcorp-sentinel-api.log",
            rollingInterval: RollingInterval.Day
        );
});

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

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("auth", limiter =>
    {
        limiter.PermitLimit = 5;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
    });

    options.AddFixedWindowLimiter("risk-analysis", limiter =>
    {
        limiter.PermitLimit = 20;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 2;
        limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
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
builder.Services.AddHealthChecks();
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var users = scope.ServiceProvider.GetRequiredService<IUserRepository>();

    var adminEmail = "joao@zcorp.dev";

    var existingAdmin = await users.GetByEmailAsync(adminEmail);

    if (existingAdmin is null)
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Sentinel@123");

        var adminUser = new User(
            "Joao Admin",
            adminEmail,
            passwordHash,
            "Admin"
        );

        await users.AddAsync(adminUser);

        Console.WriteLine("🔥 Admin user created: joao@zcorp.dev");
    }
    else
    {
        Console.WriteLine("✅ Admin user already exists");
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors(CorsPolicy);

app.UseRateLimiter();

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
app.MapHealthChecks("/health");

app.Run();