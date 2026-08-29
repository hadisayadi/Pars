namespace Pars.Application.Reports;

public record MonthlyAttendanceRecord(
    string PersonId, string FullName, string Unit,
    int WorkDays, int PresentDays, int AbsentDays,
    int LateMinutes, int EarlyDepartureMinutes,
    int OvertimeMinutes, decimal TotalWorkHours
);

public record PersonnelSummaryReport(
    int TotalPersonnel, int ActivePersonnel, int MaleCount, int FemaleCount,
    Dictionary<string, int> ByUnit, Dictionary<string, int> ByEducation
);

public interface IReportService
{
    Task<List<MonthlyAttendanceRecord>> GetMonthlyAttendanceAsync(int year, int month, CancellationToken ct = default);
    Task<PersonnelSummaryReport> GetPersonnelSummaryAsync(CancellationToken ct = default);
    Task<byte[]> ExportMonthlyAttendanceToExcelAsync(int year, int month, CancellationToken ct = default);
}