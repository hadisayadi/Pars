using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pars.Application.Common.Filtering;
using Pars.Application.Personals;
using Pars.Application.Personals.DTOs;

namespace Pars.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PersonalsController : ControllerBase
{
    private readonly IPersonalService _service;

    public PersonalsController(IPersonalService service) => _service = service;

    // GET api/personals
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? keyword, CancellationToken ct)
    {
        var result = string.IsNullOrWhiteSpace(keyword)
            ? await _service.GetAllAsync(ct)
            : await _service.SearchAsync(keyword, ct);
        return Ok(result);
    }

    // ⭐ POST api/personals/search - جستجوی پیشرفته
    [HttpPost("search")]
    public async Task<IActionResult> SearchAdvanced(
        [FromBody] PersonalFilterRequest request, CancellationToken ct)
    {
        var result = await _service.SearchAdvancedAsync(request, ct);
        return Ok(result);
    }

    // ⭐ POST api/personals/query - جستجوی عمومی با فیلتر پویا
    [HttpPost("query")]
    public async Task<IActionResult> Query(
        [FromBody] QueryRequest request, CancellationToken ct)
    {
        var result = await _service.SearchGenericAsync(request, ct);
        return Ok(result);
    }

    // ⭐ GET api/personals/filters/units - برای پر کردن فیلترها
    [HttpGet("filters/units")]
    public async Task<IActionResult> GetUnitCounts(CancellationToken ct)
    {
        var result = await _service.GetUnitCountsAsync(ct);
        return Ok(result);
    }

    // ⭐ GET api/personals/filters/companies
    [HttpGet("filters/companies")]
    public async Task<IActionResult> GetCompanies(CancellationToken ct)
    {
        var result = await _service.GetDistinctCompaniesAsync(ct);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> Create(
        [FromBody] CreatePersonalDto dto, CancellationToken ct)
    {
        var id = await _service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> Update(
        string id, [FromBody] CreatePersonalDto dto, CancellationToken ct)
    {
        await _service.UpdateAsync(id, dto, ct);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }
}