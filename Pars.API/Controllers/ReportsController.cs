using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pars.Application.Reports;

namespace Pars.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Manager,HR")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reports;

    public ReportsController(IReportService reports) => _reports = reports;

    [HttpGet("attendance/monthly")]
    public async Task<IActionResult> MonthlyAttendance(
        [FromQuery] int year, [FromQuery] int month, CancellationToken ct)
    {
        var result = await _reports.GetMonthlyAttendanceAsync(year, month, ct);
        return Ok(result);
    }

    [HttpGet("personnel/summary")]
    public async Task<IActionResult> PersonnelSummary(CancellationToken ct)
    {
        var result = await _reports.GetPersonnelSummaryAsync(ct);
        return Ok(result);
    }

    [HttpGet("attendance/export")]
    public async Task<IActionResult> ExportAttendance(
        [FromQuery] int year, [FromQuery] int month, CancellationToken ct)
    {
        var bytes = await _reports.ExportMonthlyAttendanceToExcelAsync(year, month, ct);
        return File(bytes, "text/csv", $"attendance_{year}_{month}.csv");
    }
}