using IdentityDefense.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentityDefense.Infrastructure.Persistence;

public class IdentityDefenseDbContext : DbContext
{
	public IdentityDefenseDbContext(DbContextOptions<IdentityDefenseDbContext> options)
		: base(options)
	{
	}

    public DbSet<User> Users => Set<User>();

    public DbSet<IdentityRiskCase> IdentityRiskCases => Set<IdentityRiskCase>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public List<string> RiskReasons { get; private set; } = new();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(120);

            entity.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(180);

            entity.HasIndex(x => x.Email)
                .IsUnique();

            entity.Property(x => x.PasswordHash)
                .IsRequired();

            entity.Property(x => x.Role)
                .IsRequired()
                .HasMaxLength(40);

            entity.Property(x => x.CreatedAt)
                .IsRequired();

            entity.Property(x => x.FailedLoginAttempts)
                .IsRequired();

            entity.Property(x => x.LockoutUntil);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("refresh_tokens");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.UserId)
                .IsRequired();

            entity.Property(x => x.Token)
                .IsRequired();

            entity.HasIndex(x => x.Token)
                .IsUnique();

            entity.Property(x => x.ExpiresAt)
                .IsRequired();

            entity.Property(x => x.CreatedAt)
                .IsRequired();

            entity.Property(x => x.RevokedAt);

            entity.Property(x => x.ReplacedByToken);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("audit_logs");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Action)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.Resource)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.UserEmail)
                .HasMaxLength(180);

            entity.Property(x => x.ResourceId)
                .HasMaxLength(100);

            entity.Property(x => x.IpAddress)
                .HasMaxLength(50);

            entity.Property(x => x.UserAgent)
                .HasMaxLength(300);

            entity.Property(x => x.CreatedAt)
                .IsRequired();
        });

        modelBuilder.Entity<IdentityRiskCase>(entity =>
        {
            entity.ToTable("identity_risk_cases");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Source)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.Channel)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(x => x.Subject)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(x => x.DetectedSignals)
                .HasColumnType("text[]");

            entity.Property(x => x.RiskScore)
                .IsRequired();

            entity.Property(x => x.Classification)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(x => x.RiskReasons)
                .HasColumnType("text[]");

            entity.Property(x => x.CreatedAt)
                .IsRequired();
        });

    }
}