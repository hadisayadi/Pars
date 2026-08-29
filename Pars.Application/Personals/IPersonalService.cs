using Pars.Application.Common.Filtering;
using Pars.Application.Personals.DTOs;

namespace Pars.Application.Personals;

public interface IPersonalService
{
    Task<List<PersonalDto>> GetAllAsync(CancellationToken ct = default);
    Task<PersonalDto?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<string> CreateAsync(CreatePersonalDto dto, CancellationToken ct = default);
    Task UpdateAsync(string id, CreatePersonalDto dto, CancellationToken ct = default);
    Task DeleteAsync(string id, CancellationToken ct = default);
    Task<List<PersonalDto>> SearchAsync(string? keyword, CancellationToken ct = default);

    // ⭐ روش‌های جدید فیلتر پیشرفته
    Task<PersonalSearchResult> SearchAdvancedAsync(PersonalFilterRequest request, CancellationToken ct = default);
    Task<PagedResult<PersonalDto>> SearchGenericAsync(QueryRequest request, CancellationToken ct = default);
    Task<Dictionary<string, int>> GetUnitCountsAsync(CancellationToken ct = default);
    Task<List<string>> GetDistinctCompaniesAsync(CancellationToken ct = default);
}