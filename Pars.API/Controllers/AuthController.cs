using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pars.Application.Auth;
using Pars.Application.Auth.DTOs;

namespace Pars.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await _auth.LoginAsync(request, ct);
        return result is null ? Unauthorized(new { message = "Invalid credentials" }) : Ok(result);
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var user = await _auth.RegisterAsync(request, ct);
        return CreatedAtAction(nameof(Register), user);
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        return Ok(new
        {
            Username = User.Identity?.Name,
            Roles = User.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
                              .Select(c => c.Value).ToList()
        });
    }
}