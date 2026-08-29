using Pars.Application.Attendance.DTOs;
namespace Pars.Application.Attendance;

public interface IAttendanceService
{
    Task<IReadOnlyList<AttendanceEntryDto>> GetEntriesAsync(AttendanceSearchRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<DailyAttendanceDto>> GetDailyAsync(DailyAttendanceRequest request, CancellationToken ct = default);
    Task<long> CreateManualEntryAsync(ManualAttendanceCreateDto dto, string? actor, CancellationToken ct = default);
    Task<IReadOnlyList<ManualAttendanceDto>> GetManualEntriesAsync(string personId, int take = 100, CancellationToken ct = default);
    Task<long> CreateMissionAsync(MissionCreateDto dto, string? actor, CancellationToken ct = default);
    Task<IReadOnlyList<MissionDto>> GetMissionsAsync(string? personId, string? status, int take = 100, CancellationToken ct = default);
    Task<AttendanceDashboardDto> GetDashboardAsync(DateTime date, string? persianDate, CancellationToken ct = default);
    Task<IReadOnlyList<ShiftLookupDto>> GetShiftsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<EmployeeGroupDto>> GetGroupsAsync(CancellationToken ct = default);
}
