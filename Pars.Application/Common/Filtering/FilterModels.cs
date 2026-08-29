namespace Pars.Application.Common.Filtering;

/// <summary>
/// عملگرهای فیلتر
/// </summary>
public enum FilterOperator
{
    Equals,              // =
    NotEquals,           // !=
    Contains,            // LIKE %value%
    StartsWith,          // LIKE value%
    EndsWith,            // LIKE %value
    GreaterThan,         // >
    GreaterThanOrEqual,  // >=
    LessThan,            // <
    LessThanOrEqual,     // <=
    IsNull,              // IS NULL
    IsNotNull,           // IS NOT NULL
    In                   // IN (values)
}

/// <summary>
/// منطق ترکیب فیلترها
/// </summary>
public enum FilterLogic
{
    And,
    Or
}

/// <summary>
/// توصیف یک فیلتر منفرد
/// </summary>
public class FilterDescriptor
{
    public string Field { get; set; } = default!;
    public FilterOperator Operator { get; set; } = FilterOperator.Equals;
    public object? Value { get; set; }
    public FilterLogic Logic { get; set; } = FilterLogic.And;
}

/// <summary>
/// توصیف مرتب‌سازی
/// </summary>
public class SortDescriptor
{
    public string Field { get; set; } = default!;
    public bool Descending { get; set; }
}

/// <summary>
/// درخواست فیلتر + مرتب‌سازی + صفحه‌بندی
/// </summary>
public class QueryRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public List<FilterDescriptor> Filters { get; set; } = new();
    public List<SortDescriptor> Sorts { get; set; } = new();
    public string? GlobalSearch { get; set; }
}

/// <summary>
/// نتیجه صفحه‌بندی شده
/// </summary>
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
}