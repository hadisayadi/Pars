using Microsoft.EntityFrameworkCore;
using Pars.Application.Reports;
using Pars.Infrastructure.Persistence;

namespace Pars.Infrastructure.Services;

public class ReportService : IReportService
{
    private readonly ParsDbContext _context;

    public ReportService(ParsDbContext context) => _context = context;

    public async Task<List<MonthlyAttendanceRecord>> GetMonthlyAttendanceAsync(
        int year, int month, CancellationToken ct = default)
    {
        // Query joining personal + attendance entries
        var result = await (
            from p in _context.Personals
            join ae in _context.AttendanceEntries on p.Id equals ae.PersonId
            where ae.DateTime.Year == year && ae.DateTime.Month == month
            group ae by new { p.Id, p.FirstName, p.LastName, p.UnitCode } into g
            select new MonthlyAttendanceRecord(
                PersonId: g.Key.Id,
                FullName: $"{g.Key.FirstName} {g.Key.LastName}",
                Unit: g.Key.UnitCode ?? "-",
                WorkDays: g.Select(x => x.DateTime.Date).Distinct().Count(),
                PresentDays: g.Select(x => x.DateTime.Date).Distinct().Count(),
                AbsentDays: 0, // calculated separately
                LateMinutes: 0,
                EarlyDepartureMinutes: 0,
                OvertimeMinutes: 0,
                TotalWorkHours: g.Count() * 8 // simplified
            )
        ).ToListAsync(ct);

        return result;
    }

    public async Task<PersonnelSummaryReport> GetPersonnelSummaryAsync(CancellationToken ct = default)
    {
        var all = await _context.Personals.ToListAsync(ct);

        var byUnit = all.Where(p => p.UnitCode != null)
                        .GroupBy(p => p.UnitCode!)
                        .ToDictionary(g => g.Key, g => g.Count());

        var byEducation = all.Where(p => p.Madrak != null)
                             .GroupBy(p => p.Madrak!)
                             .ToDictionary(g => g.Key, g => g.Count());

        return new PersonnelSummaryReport(
            TotalPersonnel: all.Count,
            ActivePersonnel: all.Count(p => p.Estekhdam == "1"),
            MaleCount: all.Count(p => p.Jensiat == "مرد"),
            FemaleCount: all.Count(p => p.Jensiat == "زن"),
            ByUnit: byUnit,
            ByEducation: byEducation
        );
    }

    public async Task<byte[]> ExportMonthlyAttendanceToExcelAsync(
        int year, int month, CancellationToken ct = default)
    {
        var data = await GetMonthlyAttendanceAsync(year, month, ct);
        // In production, use ClosedXML or EPPlus
        // For now, return CSV as bytes
        var csv = "PersonId,FullName,Unit,WorkDays,PresentDays\n";
        foreach (var r in data)
            csv += $"{r.PersonId},{r.FullName},{r.Unit},{r.WorkDays},{r.PresentDays}\n";

        return System.Text.Encoding.UTF8.GetBytes(csv);
    }
}