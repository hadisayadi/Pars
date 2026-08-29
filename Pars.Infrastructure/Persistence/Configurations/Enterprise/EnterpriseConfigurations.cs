using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pars.Domain.Entities.Enterprise;

namespace Pars.Infrastructure.Persistence.Configurations.Enterprise;

public sealed class EnterpriseRequestConfiguration : IEntityTypeConfiguration<EnterpriseRequest>
{
    public void Configure(EntityTypeBuilder<EnterpriseRequest> b)
    {
        b.ToTable("Requests", "workflow");
        b.HasKey(x => x.Id);
        b.Property(x => x.PersonId).HasMaxLength(50).IsRequired();
        b.Property(x => x.Title).HasMaxLength(200).IsRequired();
        b.Property(x => x.Description).HasMaxLength(1000);
        b.Property(x => x.Destination).HasMaxLength(200);
        b.Property(x => x.SubstitutePersonId).HasMaxLength(50);
        b.Property(x => x.Kind).HasConversion<int>();
        b.Property(x => x.Status).HasConversion<int>();
        b.HasIndex(x => new { x.RequestedByUserId, x.CreatedAt });
        b.HasIndex(x => new { x.Status, x.Kind });
        b.HasMany(x => x.Approvals).WithOne(x => x.Request).HasForeignKey(x => x.RequestId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class EnterpriseApprovalConfiguration : IEntityTypeConfiguration<EnterpriseApproval>
{
    public void Configure(EntityTypeBuilder<EnterpriseApproval> b)
    {
        b.ToTable("Approvals", "workflow");
        b.HasKey(x => x.Id);
        b.Property(x => x.StepName).HasMaxLength(100).IsRequired();
        b.Property(x => x.ApproverRole).HasMaxLength(50).IsRequired();
        b.Property(x => x.Status).HasMaxLength(20).IsRequired();
        b.Property(x => x.Comment).HasMaxLength(1000);
        b.HasIndex(x => new { x.Status, x.ApproverRole });
    }
}

public sealed class LeaveBalanceConfiguration : IEntityTypeConfiguration<LeaveBalance>
{
    public void Configure(EntityTypeBuilder<LeaveBalance> b)
    {
        b.ToTable("LeaveBalances", "hr");
        b.HasKey(x => x.Id);
        b.Property(x => x.PersonId).HasMaxLength(50).IsRequired();
        b.Property(x => x.AnnualEntitlementDays).HasPrecision(8,2);
        b.Property(x => x.CarriedDays).HasPrecision(8,2);
        b.Property(x => x.UsedDays).HasPrecision(8,2);
        b.Ignore(x => x.RemainingDays);
        b.HasIndex(x => new { x.PersonId, x.Year }).IsUnique();
    }
}

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> b)
    {
        b.ToTable("AuditLogs", "audit");
        b.HasKey(x => x.Id);
        b.Property(x => x.Action).HasMaxLength(100).IsRequired();
        b.Property(x => x.Entity).HasMaxLength(100).IsRequired();
        b.Property(x => x.EntityId).HasMaxLength(100);
        b.Property(x => x.Details).HasMaxLength(2000);
        b.HasIndex(x => x.CreatedAt);
    }
}
