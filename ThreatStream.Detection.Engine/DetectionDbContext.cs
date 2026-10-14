using Microsoft.EntityFrameworkCore;
using ThreatStream.Detection.Engine.Models;

namespace ThreatStream.Detection.Engine;

public sealed class DetectionDbContext : DbContext
{
    public DetectionDbContext(DbContextOptions<DetectionDbContext> options) : base(options)
    {
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();

    public DbSet<DetectionRule> DetectionRules => Set<DetectionRule>();

    public DbSet<SecurityAlert> SecurityAlerts => Set<SecurityAlert>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.ToTable("Tenants");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();
        });

        modelBuilder.Entity<DetectionRule>(entity =>
        {
            entity.ToTable("DetectionRules");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .IsRequired();

            entity.Property(x => x.TenantId)
                .IsRequired();

            entity.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.Description)
                .HasMaxLength(2000);

            entity.Property(x => x.ElasticQueryDsl)
                .IsRequired();

            entity.Property(x => x.IsActive)
                .IsRequired();

            entity.Property(x => x.CreatedAt)
                .IsRequired();

            entity.HasOne<Tenant>()
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SecurityAlert>(entity =>
        {
            entity.ToTable("SecurityAlerts");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .IsRequired();

            entity.Property(x => x.TenantId)
                .IsRequired();

            entity.Property(x => x.RuleId)
                .IsRequired();

            entity.Property(x => x.MatchedEventId)
                .HasMaxLength(256)
                .IsRequired();

            entity.Property(x => x.Severity)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.Status)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.CreatedAt)
                .IsRequired();

            entity.HasOne<Tenant>()
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<DetectionRule>()
                .WithMany()
                .HasForeignKey(x => x.RuleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new
            {
                x.TenantId,
                x.RuleId,
                x.MatchedEventId
            }).IsUnique();
        });
    }
}