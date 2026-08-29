using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pars.Application.Enterprise;
using Pars.Domain.Entities.Enterprise;

namespace Pars.API.Controllers;

[ApiController]
[Route("api/enterprise/requests")]
[Authorize]
public sealed class EnterpriseRequestsController : ControllerBase
{
    private readonly IEnterpriseRequestService _service;
    public EnterpriseRequestsController(IEnterpriseRequestService service) => _service = service;

    [HttpPost]
    public async Task<IActionResult> Create(CreateEnterpriseRequestDto dto, CancellationToken ct)
        => Ok(await _service.CreateAsync(UserId(), dto, ct));

    [HttpPost("{id:guid}/submit")]
    public async Task<IActionResult> Submit(Guid id, CancellationToken ct)
        => Ok(await _service.SubmitAsync(UserId(), id, ct));

    [HttpGet("my")]
    public async Task<IActionResult> My([FromQuery] EnterpriseRequestKind? kind, CancellationToken ct)
        => Ok(await _service.MyRequestsAsync(UserId(), kind, ct));

    [HttpGet("inbox")]
    public async Task<IActionResult> Inbox(CancellationToken ct)
        => Ok(await _service.InboxAsync(UserId(), Roles(), ct));

    [HttpPost("decision")]
    public async Task<IActionResult> Decision(ApprovalDecisionDto dto, CancellationToken ct)
        => Ok(await _service.DecideAsync(UserId(), Roles(), dto, ct));

    private Guid UserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException());
    private string[] Roles() => User.FindAll(ClaimTypes.Role).Select(x => x.Value).ToArray();
}
