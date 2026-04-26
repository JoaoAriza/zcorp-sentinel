using IdentityDefense.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentityDefense.Infrastructure.Persistence;

public class IdentityDefenseDbContext : DbContext
{
	public IdentityDefenseDbContext(DbContextOptions<IdentityDefenseDbContext> options)
		: base(options)
	{
	}

	public DbSet<IdentityRiskCase> IdentityRiskCases => Set<IdentityRiskCase>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<IdentityRiskCase>(entity =>
		{
			entity.ToTable("identity_risk_cases");

			entity.HasKey(x => x.Id);

			entity.Property(x => x.Source)
				.IsRequired()
				.HasMaxLength(120);

			entity.Property(x => x.Channel)
				.IsRequired()
				.HasMaxLength(40);

			entity.Property(x => x.Subject)
				.IsRequired()
				.HasMaxLength(240);

			entity.Property(x => x.RiskScore)
				.IsRequired();

			entity.Property(x => x.Classification)
				.IsRequired()
				.HasMaxLength(40);

			entity.Property(x => x.CreatedAt)
				.IsRequired();

			entity.Property(x => x.DetectedSignals)
				.HasColumnType("text[]");
		});
	}
}