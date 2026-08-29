using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pars.Application.Attendance;
using Pars.Application.Attendance.DTOs;

namespace Pars.API.Controllers;

[ApiController, Route("api/attendance"), Authorize(Roles="Admin,HR,Manager")]
public sealed class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _service;
    public AttendanceController(IAttendanceService service)=>_service=service;

    [HttpGet("entries")]
    public async Task<IActionResult> Entries([FromQuery] AttendanceSearchRequest request,CancellationToken ct)=>Ok(await _service.GetEntriesAsync(request,ct));

    [HttpGet("daily")]
    public async Task<IActionResult> Daily([FromQuery] DailyAttendanceRequest request,CancellationToken ct)=>Ok(await _service.GetDailyAsync(request,ct));

    [HttpGet("manual/{personId}")]
    public async Task<IActionResult> Manual(string personId,[FromQuery]int take,CancellationToken ct)=>Ok(await _service.GetManualEntriesAsync(personId,take<=0?100:take,ct));

    [HttpPost("manual"), Authorize(Roles="Admin,HR")]
    public async Task<IActionResult> CreateManual([FromBody]ManualAttendanceCreateDto dto,CancellationToken ct){var id=await _service.CreateManualEntryAsync(dto,User.Identity?.Name,ct);return Created($"api/attendance/manual/{id}",new{id});}

    [HttpGet("missions")]
    public async Task<IActionResult> Missions([FromQuery]string? personId,[FromQuery]string? status,[FromQuery]int take,CancellationToken ct)=>Ok(await _service.GetMissionsAsync(personId,status,take<=0?100:take,ct));

    [HttpPost("missions"), Authorize(Roles="Admin,HR")]
    public async Task<IActionResult> CreateMission([FromBody]MissionCreateDto dto,CancellationToken ct){var id=await _service.CreateMissionAsync(dto,User.Identity?.Name,ct);return Created($"api/attendance/missions/{id}",new{id});}

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard([FromQuery]DateTime? date,[FromQuery]string? persianDate,CancellationToken ct)=>Ok(await _service.GetDashboardAsync(date??DateTime.Today,persianDate,ct));
    [HttpGet("shifts")] public async Task<IActionResult> Shifts(CancellationToken ct)=>Ok(await _service.GetShiftsAsync(ct));
    [HttpGet("groups")] public async Task<IActionResult> Groups(CancellationToken ct)=>Ok(await _service.GetGroupsAsync(ct));
}
