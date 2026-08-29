using Microsoft.AspNetCore.Mvc;
namespace Pars.API.Controllers;
[ApiController]
[Route("api/approval")]
public class ApprovalController:ControllerBase { [HttpGet("inbox")] public IActionResult Inbox()=>Ok(); }
