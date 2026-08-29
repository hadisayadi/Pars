namespace Pars.Application.Personals.DTOs;

/// <summary>
/// فیلترهای اختصاصی ماژول پرسنل
/// </summary>
public class PersonalFilterRequest
{
    // جستجوی عمومی
    public string? Search { get; set; }

    // فیلترهای مشخص
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? CodeMelli { get; set; }
    public string? Company { get; set; }
    public string? Pos { get; set; }
    public string? Jensiat { get; set; }

    // فیلترهای بازه‌ای
    public string? DateEstekhdamFrom { get; set; }
    public string? DateEstekhdamTo { get; set; }

    // فیلترهای چند انتخابی
    public List<string>? UnitCodes { get; set; }
    public List<string>? MadrakLevels { get; set; }

    // وضعیت
    public string? Estekhdam { get; set; }
    public bool? NobatKar { get; set; }

    // صفحه‌بندی
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; } = "LastName";
    public bool SortDescending { get; set; }
}

/// <summary>
/// نتیجه پیشرفته با آمار
/// </summary>
public class PersonalSearchResult
{
    public List<PersonalDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;

    // آمار فیلتر شده
    public int FilteredMaleCount { get; set; }
    public int FilteredFemaleCount { get; set; }
}