using Microsoft.EntityFrameworkCore;
using Pars.Application.Enterprise;
using Pars.Domain.Entities.Enterprise;
using Pars.Infrastructure.Persistence;

namespace Pars.Infrastructure.Services.Enterprise;

public sealed class EnterpriseRequestService : IEnterpriseRequestService
{
    private readonly ParsDbContext _db;
    public EnterpriseRequestService(ParsDbContext db) => _db = db;

    public async Task<EnterpriseRequestDto> CreateAsync(Guid userId, CreateEnterpriseRequestDto dto, CancellationToken ct = default)
    {
        if (dto.EndDate.Date < dto.StartDate.Date) throw new InvalidOperationException("تاریخ پایان نمی‌تواند قبل از تاریخ شروع باشد.");
        if (dto.IsHourly && (!dto.StartTime.HasValue || !dto.EndTime.HasValue || dto.EndTime <= dto.StartTime))
            throw new InvalidOperationException("برای درخواست ساعتی، بازه زمانی معتبر وارد کنید.");
        if (dto.Kind == EnterpriseRequestKind.Mission && string.IsNullOrWhiteSpace(dto.Destination))
            throw new InvalidOperationException("مقصد مأموریت الزامی است.");

        var entity = new EnterpriseRequest
        {
            RequestedByUserId = userId,
            Kind = dto.Kind,
            PersonId = dto.PersonId.Trim(),
            Title = dto.Title.Trim(),
            Description = dto.Description?.Trim(),
            StartDate = dto.StartDate.Date,
            EndDate = dto.EndDate.Date,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            IsHourly = dto.IsHourly,
            Destination = dto.Destination?.Trim(),
            HasVehicle = dto.HasVehicle,
            SubstitutePersonId = dto.SubstitutePersonId?.Trim(),
            Status = EnterpriseRequestStatus.Draft
        };
        _db.EnterpriseRequests.Add(entity);
        await AuditAsync(userId, "Create", "EnterpriseRequest", entity.Id.ToString(), $"{entity.Kind}:{entity.Title}", ct);
        await _db.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task<EnterpriseRequestDto> SubmitAsync(Guid userId, Guid requestId, CancellationToken ct = default)
    {
        var entity = await _db.EnterpriseRequests.Include(x => x.Approvals).SingleOrDefaultAsync(x => x.Id == requestId, ct)
            ?? throw new KeyNotFoundException("درخواست یافت نشد.");
        if (entity.RequestedByUserId != userId) throw new UnauthorizedAccessException();
        if (entity.Status != EnterpriseRequestStatus.Draft) throw new InvalidOperationException("فقط درخواست پیش‌نویس قابل ارسال است.");

        entity.Status = EnterpriseRequestStatus.Pending;
        entity.SubmittedAt = DateTime.UtcNow;
        entity.CurrentStep = 1;
        entity.Approvals.Add(new EnterpriseApproval { Step = 1, StepName = "تأیید مدیر مستقیم", ApproverRole = "Manager" });
        entity.Approvals.Add(new EnterpriseApproval { Step = 2, StepName = "تأیید منابع انسانی", ApproverRole = "HR" });
        await AuditAsync(userId, "Submit", "EnterpriseRequest", entity.Id.ToString(), null, ct);
        await _db.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task<IReadOnlyList<EnterpriseRequestDto>> MyRequestsAsync(Guid userId, EnterpriseRequestKind? kind = null, CancellationToken ct = default)
    {
        var q = _db.EnterpriseRequests.AsNoTracking().Include(x => x.Approvals).Where(x => x.RequestedByUserId == userId);
        if (kind.HasValue) q = q.Where(x => x.Kind == kind.Value);
        return (await q.OrderByDescending(x => x.CreatedAt).Take(200).ToListAsync(ct)).Select(Map).ToList();
    }

    public async Task<IReadOnlyList<EnterpriseRequestDto>> InboxAsync(Guid userId, IEnumerable<string> roles, CancellationToken ct = default)
    {
        var roleSet = roles.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        if (roleSet.Contains("Admin", StringComparer.OrdinalIgnoreCase)) roleSet = roleSet.Append("Manager").Append("HR").Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var q = _db.EnterpriseRequests.AsNoTracking().Include(x => x.Approvals)
            .Where(x => x.Status == EnterpriseRequestStatus.Pending && x.Approvals.Any(a => a.Step == x.CurrentStep && a.Status == "Pending" && (a.ApproverUserId == userId || roleSet.Contains(a.ApproverRole))));
        return (await q.OrderBy(x => x.SubmittedAt).Take(200).ToListAsync(ct)).Select(Map).ToList();
    }

    public async Task<EnterpriseRequestDto> DecideAsync(Guid userId, IEnumerable<string> roles, ApprovalDecisionDto dto, CancellationToken ct = default)
    {
        var entity = await _db.EnterpriseRequests.Include(x => x.Approvals).SingleOrDefaultAsync(x => x.Id == dto.RequestId, ct)
            ?? throw new KeyNotFoundException("درخواست یافت نشد.");
        if (entity.Status != EnterpriseRequestStatus.Pending) throw new InvalidOperationException("این درخواست در وضعیت بررسی نیست.");
        var current = entity.Approvals.SingleOrDefault(x => x.Step == entity.CurrentStep && x.Status == "Pending")
            ?? throw new InvalidOperationException("مرحله جاری گردش کار یافت نشد.");
        var roleSet = roles.ToArray();
        if (current.ApproverUserId != userId && !roleSet.Contains(current.ApproverRole, StringComparer.OrdinalIgnoreCase) && !roleSet.Contains("Admin", StringComparer.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException();

        var action = dto.Action.Trim().ToLowerInvariant();
        if (action is not ("approve" or "reject")) throw new InvalidOperationException("عملیات فقط approve یا reject است.");
        current.ApproverUserId = userId;
        current.Comment = dto.Comment?.Trim();
        current.ActionAt = DateTime.UtcNow;
        current.Status = action == "approve" ? "Approved" : "Rejected";

        if (action == "reject")
        {
            entity.Status = EnterpriseRequestStatus.Rejected;
            entity.ClosedAt = DateTime.UtcNow;
        }
        else
        {
            var next = entity.Approvals.Where(x => x.Step > entity.CurrentStep).OrderBy(x => x.Step).FirstOrDefault();
            if (next is null)
            {
                entity.Status = EnterpriseRequestStatus.Approved;
                entity.ClosedAt = DateTime.UtcNow;
                if (entity.Kind == EnterpriseRequestKind.Leave) await ApplyLeaveBalanceAsync(entity, ct);
            }
            else entity.CurrentStep = next.Step;
        }

        await AuditAsync(userId, action == "approve" ? "Approve" : "Reject", "EnterpriseRequest", entity.Id.ToString(), dto.Comment, ct);
        await _db.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task<LeaveBalanceDto> GetLeaveBalanceAsync(string personId, int year, CancellationToken ct = default)
    {
        var b = await _db.LeaveBalances.AsNoTracking().SingleOrDefaultAsync(x => x.PersonId == personId && x.Year == year, ct);
        if (b is null) return new(personId, year, 26, 0, 0, 26);
        return new(b.PersonId, b.Year, b.AnnualEntitlementDays, b.CarriedDays, b.UsedDays, b.RemainingDays);
    }

    public async Task<EnterpriseDashboardDto> GetDashboardAsync(DateTime date, CancellationToken ct = default)
    {
        var day = date.Date;
        var monthStart = new DateTime(day.Year, day.Month, 1);
        var monthEnd = monthStart.AddMonths(1);
        var employees = await _db.Personals.AsNoTracking().CountAsync(ct);
        var present = await _db.AttendanceEntries.AsNoTracking().Where(x => x.DateTime.Date == day).Select(x => x.PersonId).Distinct().CountAsync(ct);
        var pending = await _db.EnterpriseRequests.AsNoTracking().CountAsync(x => x.Status == EnterpriseRequestStatus.Pending, ct);
        var approvedMonth = await _db.EnterpriseRequests.AsNoTracking().CountAsync(x => x.Status == EnterpriseRequestStatus.Approved && x.ClosedAt >= monthStart && x.ClosedAt < monthEnd, ct);
        var leaveToday = await _db.EnterpriseRequests.AsNoTracking().CountAsync(x => x.Kind == EnterpriseRequestKind.Leave && x.Status == EnterpriseRequestStatus.Approved && x.StartDate <= day && x.EndDate >= day, ct);
        var missionsToday = await _db.EnterpriseRequests.AsNoTracking().CountAsync(x => x.Kind == EnterpriseRequestKind.Mission && x.Status == EnterpriseRequestStatus.Approved && x.StartDate <= day && x.EndDate >= day, ct);
        var calc = _db.DailyAttendanceCalculations.AsNoTracking().Where(x => x.StartTime.HasValue && x.StartTime.Value.Date == day);
        var late = await calc.CountAsync(x => (x.ArrivalLateMinutes ?? 0) > 0, ct);
        var overtime = await calc.CountAsync(x => (x.OvertimeMinute ?? 0) > 0, ct);
        return new(employees, present, pending, approvedMonth, leaveToday, missionsToday, late, overtime);
    }

    private async Task ApplyLeaveBalanceAsync(EnterpriseRequest request, CancellationToken ct)
    {
        var year = request.StartDate.Year;
        var balance = await _db.LeaveBalances.SingleOrDefaultAsync(x => x.PersonId == request.PersonId && x.Year == year, ct);
        if (balance is null)
        {
            balance = new LeaveBalance { PersonId = request.PersonId, Year = year };
            _db.LeaveBalances.Add(balance);
        }
        decimal used = request.IsHourly && request.StartTime.HasValue && request.EndTime.HasValue
            ? (decimal)(request.EndTime.Value - request.StartTime.Value).TotalHours / 8m
            : (decimal)(request.EndDate.Date - request.StartDate.Date).TotalDays + 1m;
        balance.UsedDays += Math.Max(used, 0);
        balance.UpdatedAt = DateTime.UtcNow;
    }

    private Task AuditAsync(Guid userId, string action, string entity, string? entityId, string? details, CancellationToken ct)
    {
        _db.AuditLogs.Add(new AuditLog { UserId = userId, Action = action, Entity = entity, EntityId = entityId, Details = details });
        return Task.CompletedTask;
    }

    private static EnterpriseRequestDto Map(EnterpriseRequest x) => new(
        x.Id, x.Kind, x.PersonId, x.Title, x.Description, x.StartDate, x.EndDate, x.StartTime, x.EndTime,
        x.IsHourly, x.Destination, x.Status, x.CurrentStep, x.CreatedAt, x.SubmittedAt,
        x.Approvals.OrderBy(a => a.Step).Select(a => new ApprovalDto(a.Id, a.Step, a.StepName, a.ApproverRole, a.Status, a.Comment, a.ActionAt)).ToList());
}
