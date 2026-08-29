using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pars.Application.Enterprise;

namespace Pars.API.Controllers;

[ApiController]
[Route("api/hr")]
[Authorize]
public sealed class HrDashboardController : ControllerBase
{
    private readonly IEnterpriseRequestService _service;
    public HrDashboardController(IEnterpriseRequestService service) => _service = service;

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard([FromQuery] DateTime? date, CancellationToken ct)
        => Ok(await _service.GetDashboardAsync(date ?? DateTime.Today, ct));

    [HttpGet("leave-balance/{personId}")]
    public async Task<IActionResult> Balance(string personId, [FromQuery] int? year, CancellationToken ct)
        => Ok(await _service.GetLeaveBalanceAsync(personId, year ?? DateTime.Today.Year, ct));
}
