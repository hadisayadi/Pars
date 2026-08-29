using Pars.Domain.Entities.Enterprise;

namespace Pars.Application.Enterprise;

public record CreateEnterpriseRequestDto(
    EnterpriseRequestKind Kind,
    string PersonId,
    string Title,
    string? Description,
    DateTime StartDate,
    DateTime EndDate,
    TimeSpan? StartTime,
    TimeSpan? EndTime,
    bool IsHourly,
    string? Destination,
    bool HasVehicle,
    string? SubstitutePersonId);

public record EnterpriseRequestDto(
    Guid Id,
    EnterpriseRequestKind Kind,
    string PersonId,
    string Title,
    string? Description,
    DateTime StartDate,
    DateTime EndDate,
    TimeSpan? StartTime,
    TimeSpan? EndTime,
    bool IsHourly,
    string? Destination,
    EnterpriseRequestStatus Status,
    int CurrentStep,
    DateTime CreatedAt,
    DateTime? SubmittedAt,
    IReadOnlyList<ApprovalDto> Approvals);

public record ApprovalDto(Guid Id, int Step, string StepName, string ApproverRole, string Status, string? Comment, DateTime? ActionAt);
public record ApprovalDecisionDto(Guid RequestId, string Action, string? Comment);
public record LeaveBalanceDto(string PersonId, int Year, decimal EntitlementDays, decimal CarriedDays, decimal UsedDays, decimal RemainingDays);
public record EnterpriseDashboardDto(int TotalEmployees, int PresentToday, int PendingRequests, int ApprovedThisMonth, int LeaveToday, int MissionsToday, int LateToday, int OvertimeToday);

public interface IEnterpriseRequestService
{
    Task<EnterpriseRequestDto> CreateAsync(Guid userId, CreateEnterpriseRequestDto dto, CancellationToken ct = default);
    Task<EnterpriseRequestDto> SubmitAsync(Guid userId, Guid requestId, CancellationToken ct = default);
    Task<IReadOnlyList<EnterpriseRequestDto>> MyRequestsAsync(Guid userId, EnterpriseRequestKind? kind = null, CancellationToken ct = default);
    Task<IReadOnlyList<EnterpriseRequestDto>> InboxAsync(Guid userId, IEnumerable<string> roles, CancellationToken ct = default);
    Task<EnterpriseRequestDto> DecideAsync(Guid userId, IEnumerable<string> roles, ApprovalDecisionDto dto, CancellationToken ct = default);
    Task<LeaveBalanceDto> GetLeaveBalanceAsync(string personId, int year, CancellationToken ct = default);
    Task<EnterpriseDashboardDto> GetDashboardAsync(DateTime date, CancellationToken ct = default);
}
