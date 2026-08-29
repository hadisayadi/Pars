namespace Pars.Web.Models;

public enum EnterpriseRequestKind { Leave = 1, Mission = 2 }
public enum EnterpriseRequestStatus { Draft = 0, Pending = 1, Approved = 2, Rejected = 3, Cancelled = 4 }

public sealed class EnterpriseRequestVm
{
    public Guid Id { get; set; }
    public EnterpriseRequestKind Kind { get; set; }
    public string PersonId { get; set; } = "";
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public bool IsHourly { get; set; }
    public string? Destination { get; set; }
    public EnterpriseRequestStatus Status { get; set; }
    public int CurrentStep { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public List<ApprovalVm> Approvals { get; set; } = new();
}

public sealed class ApprovalVm
{
    public Guid Id { get; set; }
    public int Step { get; set; }
    public string StepName { get; set; } = "";
    public string ApproverRole { get; set; } = "";
    public string Status { get; set; } = "";
    public string? Comment { get; set; }
    public DateTime? ActionAt { get; set; }
}

public sealed class EnterpriseDashboardVm
{
    public int TotalEmployees { get; set; }
    public int PresentToday { get; set; }
    public int PendingRequests { get; set; }
    public int ApprovedThisMonth { get; set; }
    public int LeaveToday { get; set; }
    public int MissionsToday { get; set; }
    public int LateToday { get; set; }
    public int OvertimeToday { get; set; }
}

public sealed class LeaveBalanceVm
{
    public string PersonId { get; set; } = "";
    public int Year { get; set; }
    public decimal EntitlementDays { get; set; }
    public decimal CarriedDays { get; set; }
    public decimal UsedDays { get; set; }
    public decimal RemainingDays { get; set; }
}
