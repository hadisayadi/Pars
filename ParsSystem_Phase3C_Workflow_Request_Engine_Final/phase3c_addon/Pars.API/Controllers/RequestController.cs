using Microsoft.AspNetCore.Mvc;
namespace Pars.API.Controllers;
[ApiController]
[Route("api/requests")]
public class RequestController:ControllerBase { [HttpGet("my")] public IActionResult My()=>Ok(); }
