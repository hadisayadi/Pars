namespace Pars.Domain.Entities.Enterprise;

public enum EnterpriseRequestKind { Leave = 1, Mission = 2 }
public enum EnterpriseRequestStatus { Draft = 0, Pending = 1, Approved = 2, Rejected = 3, Cancelled = 4 }

public sealed class EnterpriseRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public EnterpriseRequestKind Kind { get; set; }
    public string PersonId { get; set; } = default!;
    public Guid RequestedByUserId { get; set; }
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public bool IsHourly { get; set; }
    public string? Destination { get; set; }
    public bool HasVehicle { get; set; }
    public string? SubstitutePersonId { get; set; }
    public EnterpriseRequestStatus Status { get; set; } = EnterpriseRequestStatus.Draft;
    public int CurrentStep { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public ICollection<EnterpriseApproval> Approvals { get; set; } = new List<EnterpriseApproval>();
}

public sealed class EnterpriseApproval
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RequestId { get; set; }
    public int Step { get; set; }
    public string StepName { get; set; } = default!;
    public Guid? ApproverUserId { get; set; }
    public string ApproverRole { get; set; } = "Manager";
    public string Status { get; set; } = "Pending";
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ActionAt { get; set; }
    public EnterpriseRequest Request { get; set; } = default!;
}

public sealed class LeaveBalance
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string PersonId { get; set; } = default!;
    public int Year { get; set; }
    public decimal AnnualEntitlementDays { get; set; } = 26;
    public decimal CarriedDays { get; set; }
    public decimal UsedDays { get; set; }
    public decimal RemainingDays => AnnualEntitlementDays + CarriedDays - UsedDays;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public sealed class AuditLog
{
    public long Id { get; set; }
    public Guid? UserId { get; set; }
    public string Action { get; set; } = default!;
    public string Entity { get; set; } = default!;
    public string? EntityId { get; set; }
    public string? Details { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
