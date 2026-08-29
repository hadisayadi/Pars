using Microsoft.AspNetCore.Mvc;
namespace Pars.API.Controllers;
[ApiController]
[Route("api/workflow")]
public class WorkflowController:ControllerBase
{
 [HttpGet("health")]
 public object Health()=>new { Module="Workflow Engine", Status="Ready"};
}
