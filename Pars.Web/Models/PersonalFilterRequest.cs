namespace Pars.Web.Models;

public class PersonalFilterRequest
{
    public string? Search { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? CodeMelli { get; set; }
    public string? Company { get; set; }
    public string? Pos { get; set; }
    public string? Jensiat { get; set; }
    public string? DateEstekhdamFrom { get; set; }
    public string? DateEstekhdamTo { get; set; }
    public List<string>? UnitCodes { get; set; }
    public string? Estekhdam { get; set; }
    public bool? NobatKar { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; } = "LastName";
    public bool SortDescending { get; set; }
}

public class PersonalDto
{
    public string Id { get; set; } = "";
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? FatherName { get; set; }
    public string? CodeMelli { get; set; }
    public string? TelMob { get; set; }
    public string? Email { get; set; }
    public string? Company { get; set; }
    public string? Pos { get; set; }
    public string? DateEstekhdam { get; set; }
}

public class PersonalSearchResult
{
    public List<PersonalDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasPrevious { get; set; }
    public bool HasNext { get; set; }
    public int FilteredMaleCount { get; set; }
    public int FilteredFemaleCount { get; set; }
}