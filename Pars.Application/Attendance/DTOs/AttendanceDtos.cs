using System.ComponentModel.DataAnnotations;

namespace Pars.Application.Attendance.DTOs;

public sealed class AttendanceSearchRequest
{
    [Required, StringLength(10, MinimumLength = 1)] public string PersonId { get; set; } = "";
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    [Range(1, 500)] public int Take { get; set; } = 100;
}

public sealed record AttendanceEntryDto(long Id, string PersonId, DateTime DateTime, string? AddBy, DateTime? Updated);

public sealed class DailyAttendanceRequest
{
    [Required, StringLength(10)] public string PersonId { get; set; } = "";
    [StringLength(10)] public string? FromPersianDate { get; set; }
    [StringLength(10)] public string? ToPersianDate { get; set; }
}

public sealed record DailyAttendanceDto(
    string? PersianDate, string? PersonId, int AttendanceMinutes, int DutyMinutes,
    DateTime? Arrival, DateTime? Departure, int LateMinutes, int EarlyDepartureMinutes,
    int OvertimeMinutes, bool IsOffDay, bool IsHoliday, int HourlyLeaveMinutes,
    int HourlyMissionMinutes, string? Status, string? Entries);

public sealed class ManualAttendanceCreateDto
{
    [Required, StringLength(10)] public string PersonId { get; set; } = "";
    [Required, StringLength(10)] public string Kind { get; set; } = "";
    [Required, StringLength(10)] public string Date { get; set; } = "";
    [Required] public TimeSpan? Time { get; set; }
    [StringLength(250)] public string? Description { get; set; }
    [StringLength(50)] public string? SendTo { get; set; }
}

public sealed record ManualAttendanceDto(long Id, string PersonId, string? Kind, string? Date, TimeSpan? Time, string? Description, string? Status);

public sealed class MissionCreateDto
{
    [Required, StringLength(10)] public string PersonId { get; set; } = "";
    public long? RequestTypeId { get; set; }
    [Required, StringLength(10)] public string DateFrom { get; set; } = "";
    [Required, StringLength(10)] public string DateTo { get; set; } = "";
    public TimeSpan? TimeFrom { get; set; }
    public TimeSpan? TimeTo { get; set; }
    [StringLength(150)] public string? Destination { get; set; }
    [StringLength(100)] public string? Subject { get; set; }
    public bool? HasVehicle { get; set; }
}

public sealed record MissionDto(long Id, string PersonId, string? Code, string? DateFrom, string? DateTo, TimeSpan? TimeFrom, TimeSpan? TimeTo, string? Destination, string? Subject, bool? HasVehicle, string? Status);
public sealed record AttendanceDashboardDto(int PersonnelWithEntries, int RawEntries, int ManualEntries, int Missions, int LatePersons, int OvertimePersons);
public sealed record ShiftLookupDto(long Id, string? Name, string? ShiftCode, string? Description);
public sealed record EmployeeGroupDto(long Id, string? Name, string? Description, int PersonsCount);
