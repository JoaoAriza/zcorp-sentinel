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
        });
    }
}