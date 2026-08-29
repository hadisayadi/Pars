using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Pars.Application.Common.Filtering;
using Pars.Application.Personals;
using Pars.Application.Personals.DTOs;
using Pars.Domain.Entities;
using Pars.Infrastructure.Persistence;

namespace Pars.Infrastructure.Services;

public class PersonalService : IPersonalService
{
    private readonly ParsDbContext _context;

    public PersonalService(ParsDbContext context) => _context = context;

    // ────────────────────────────────────────────────────
    // ⭐ جستجوی پیشرفته با فیلترهای اختصاصی
    // ────────────────────────────────────────────────────
    public async Task<PersonalSearchResult> SearchAdvancedAsync(
        PersonalFilterRequest request, CancellationToken ct = default)
    {
        var query = _context.Personals.AsQueryable();

        // 1. جستجوی عمومی
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(p =>
                (p.FirstName != null && p.FirstName.ToLower().Contains(search)) ||
                (p.LastName != null && p.LastName.ToLower().Contains(search)) ||
                (p.CodeMelli != null && p.CodeMelli.Contains(search)) ||
                (p.TelMob != null && p.TelMob.Contains(search)) ||
                p.Id.Contains(search)
            );
        }

        // 2. فیلترهای مشخص
        if (!string.IsNullOrWhiteSpace(request.FirstName))
            query = query.Where(p => p.FirstName != null &&
                p.FirstName.ToLower().Contains(request.FirstName.ToLower()));

        if (!string.IsNullOrWhiteSpace(request.LastName))
            query = query.Where(p => p.LastName != null &&
                p.LastName.ToLower().Contains(request.LastName.ToLower()));

        if (!string.IsNullOrWhiteSpace(request.CodeMelli))
            query = query.Where(p => p.CodeMelli != null &&
                p.CodeMelli.Contains(request.CodeMelli));

        if (!string.IsNullOrWhiteSpace(request.Company))
            query = query.Where(p => p.Company != null &&
                p.Company.ToLower().Contains(request.Company.ToLower()));

        if (!string.IsNullOrWhiteSpace(request.Pos))
            query = query.Where(p => p.Pos != null &&
                p.Pos.ToLower().Contains(request.Pos.ToLower()));

        if (!string.IsNullOrWhiteSpace(request.Jensiat))
            query = query.Where(p => p.Jensiat == request.Jensiat);

        if (!string.IsNullOrWhiteSpace(request.Estekhdam))
            query = query.Where(p => p.Estekhdam == request.Estekhdam);

        if (request.NobatKar.HasValue)
            query = query.Where(p => p.NobatKar == request.NobatKar.Value);

        // 3. فیلترهای بازه‌ای تاریخ
        if (!string.IsNullOrWhiteSpace(request.DateEstekhdamFrom))
            query = query.Where(p => p.DateEstekhdam != null &&
                string.Compare(p.DateEstekhdam, request.DateEstekhdamFrom) >= 0);

        if (!string.IsNullOrWhiteSpace(request.DateEstekhdamTo))
            query = query.Where(p => p.DateEstekhdam != null &&
                string.Compare(p.DateEstekhdam, request.DateEstekhdamTo) <= 0);

        // 4. فیلترهای چند انتخابی
        if (request.UnitCodes is { Count: > 0 })
            query = query.Where(p => p.UnitCode != null &&
                request.UnitCodes.Contains(p.UnitCode));

        if (request.MadrakLevels is { Count: > 0 })
            query = query.Where(p => p.Madrak != null &&
                request.MadrakLevels.Contains(p.Madrak));

        // 5. شمارش کل قبل از صفحه‌بندی
        var totalCount = await query.CountAsync(ct);

        // 6. آمار فیلتر شده
        var maleCount = await query.CountAsync(p => p.Jensiat == "مرد", ct);
        var femaleCount = await query.CountAsync(p => p.Jensiat == "زن", ct);

        // 7. مرتب‌سازی
        query = request.SortBy?.ToLower() switch
        {
            "firstname" => request.SortDescending
                ? query.OrderByDescending(p => p.FirstName)
                : query.OrderBy(p => p.FirstName),
            "lastname" => request.SortDescending
                ? query.OrderByDescending(p => p.LastName)
                : query.OrderBy(p => p.LastName),
            "codemelli" => request.SortDescending
                ? query.OrderByDescending(p => p.CodeMelli)
                : query.OrderBy(p => p.CodeMelli),
            "dateestekhdam" => request.SortDescending
                ? query.OrderByDescending(p => p.DateEstekhdam)
                : query.OrderBy(p => p.DateEstekhdam),
            _ => query.OrderBy(p => p.LastName)
        };

        // 8. صفحه‌بندی
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var items = await query
            .Skip((request.Page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PersonalDto(
                p.Id, p.FirstName, p.LastName, p.FatherName,
                p.CodeMelli, p.TelMob, p.Email, p.Company, p.Pos, p.DateEstekhdam))
            .ToListAsync(ct);

        return new PersonalSearchResult
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = pageSize,
            FilteredMaleCount = maleCount,
            FilteredFemaleCount = femaleCount
        };
    }

    // ────────────────────────────────────────────────────
    // ⭐ جستجوی عمومی با QueryBuilder (برای هر جدولی)
    // ────────────────────────────────────────────────────
    public async Task<PagedResult<PersonalDto>> SearchGenericAsync(
        QueryRequest request, CancellationToken ct = default)
    {
        // فیلتر جستجوی عمومی برای پرسنل
        Expression<Func<Personal, bool>>? globalFilter = null;

        if (!string.IsNullOrWhiteSpace(request.GlobalSearch))
        {
            var s = request.GlobalSearch.ToLower();
            globalFilter = p =>
                (p.FirstName != null && p.FirstName.ToLower().Contains(s)) ||
                (p.LastName != null && p.LastName.ToLower().Contains(s)) ||
                (p.CodeMelli != null && p.CodeMelli.Contains(s));
        }

        var query = QueryBuilder<Personal>.Apply(
            _context.Personals.AsQueryable(), request, globalFilter);

        var totalCount = await _context.Personals.CountAsync(ct);

        var items = await query
            .Select(p => new PersonalDto(
                p.Id, p.FirstName, p.LastName, p.FatherName,
                p.CodeMelli, p.TelMob, p.Email, p.Company, p.Pos, p.DateEstekhdam))
            .ToListAsync(ct);

        return new PagedResult<PersonalDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    // ────────────────────────────────────────────────────
    // ⭐ متدهای کمکی برای پر کردن فیلترها
    // ────────────────────────────────────────────────────
    public async Task<Dictionary<string, int>> GetUnitCountsAsync(CancellationToken ct = default)
    {
        return await _context.Personals
            .Where(p => p.UnitCode != null)
            .GroupBy(p => p.UnitCode!)
            .Select(g => new { Unit = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Unit, x => x.Count, ct);
    }

    public async Task<List<string>> GetDistinctCompaniesAsync(CancellationToken ct = default)
    {
        return await _context.Personals
            .Where(p => p.Company != null)
            .Select(p => p.Company!)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync(ct);
    }

    // ────────────────────────────────────────────────────
    // متدهای قبلی (بدون تغییر)
    // ────────────────────────────────────────────────────
    public async Task<List<PersonalDto>> GetAllAsync(CancellationToken ct = default)
        => await _context.Personals
            .Select(p => new PersonalDto(p.Id, p.FirstName, p.LastName, p.FatherName,
                p.CodeMelli, p.TelMob, p.Email, p.Company, p.Pos, p.DateEstekhdam))
            .ToListAsync(ct);

    public async Task<PersonalDto?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var p = await _context.Personals.FindAsync(new object[] { id }, ct);
        return p is null ? null : new PersonalDto(p.Id, p.FirstName, p.LastName, p.FatherName,
            p.CodeMelli, p.TelMob, p.Email, p.Company, p.Pos, p.DateEstekhdam);
    }

    public async Task<string> CreateAsync(CreatePersonalDto dto, CancellationToken ct = default)
    {
        var entity = new Personal
        {
            Id = dto.Id,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            FatherName = dto.FatherName,
            CodeMelli = dto.CodeMelli,
            TelMob = dto.TelMob,
            Email = dto.Email,
            Company = dto.Company,
            Pos = dto.Pos,
            AddBy = "System"
        };
        await _context.Personals.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);
        return entity.Id;
    }

    public async Task UpdateAsync(string id, CreatePersonalDto dto, CancellationToken ct = default)
    {
        var entity = await _context.Personals.FindAsync(new object[] { id }, ct)
            ?? throw new KeyNotFoundException($"Personal {id} not found");

        entity.FirstName = dto.FirstName;
        entity.LastName = dto.LastName;
        entity.FatherName = dto.FatherName;
        entity.CodeMelli = dto.CodeMelli;
        entity.TelMob = dto.TelMob;
        entity.Email = dto.Email;
        entity.Company = dto.Company;
        entity.Pos = dto.Pos;

        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        var entity = await _context.Personals.FindAsync(new object[] { id }, ct);
        if (entity is null) return;
        _context.Personals.Remove(entity);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<List<PersonalDto>> SearchAsync(string? keyword, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(keyword)) return await GetAllAsync(ct);
        var k = keyword.ToLower();
        return await _context.Personals
            .Where(p => (p.FirstName != null && p.FirstName.ToLower().Contains(k))
                     || (p.LastName != null && p.LastName.ToLower().Contains(k))
                     || (p.CodeMelli != null && p.CodeMelli.Contains(k)))
            .Select(p => new PersonalDto(p.Id, p.FirstName, p.LastName, p.FatherName,
                p.CodeMelli, p.TelMob, p.Email, p.Company, p.Pos, p.DateEstekhdam))
            .ToListAsync(ct);
    }
}