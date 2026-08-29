using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pars.Application.Security;
namespace Pars.API.Controllers;
[ApiController]
[Route("api/security/permissions")]
[Authorize]
public class PermissionsController:ControllerBase
{
 private readonly IPermissionService service;
 public PermissionsController(IPermissionService service)=>this.service=service;
 [HttpGet("{userId:guid}")]
 public async Task<IActionResult> Get(Guid userId,CancellationToken ct)=>Ok(await service.GetUserPermissionsAsync(userId,ct));
}
