namespace Pars.Domain.Entities.Core;

public class LegacyAccess
{
    public string FormName { get; set; } = default!;
    public string? FormDescription { get; set; }
    public string? EditUsers { get; set; }
    public string? ViewUsers { get; set; }
    public string? ApproveUsers { get; set; }
    public int? ApproveLevel { get; set; }
    public string? AddBy { get; set; }
    public int? AdminLevel { get; set; }
    public string? Approve2 { get; set; }
    public string? Approve3 { get; set; }
    public string? Approve4 { get; set; }
    public string? Approve5 { get; set; }
    public string? DeleteUsers { get; set; }
    public string? DeleteComment { get; set; }
    public string? EditComment { get; set; }
    public string? A1Comment { get; set; }
    public string? A2Comment { get; set; }
    public string? A3Comment { get; set; }
    public string? A4Comment { get; set; }
    public string? A5Comment { get; set; }
    public string? Tag { get; set; }
    public ICollection<LegacyAccessScope> Scopes { get; set; } = new List<LegacyAccessScope>();
}

public class LegacyAccessScope
{
    public long Id { get; set; }
    public string? FormName { get; set; }
    public string? Kind { get; set; }
    public string? UnitCode { get; set; }
    public string? UnitName { get; set; }
    public string? Position { get; set; }
    public string? AddBy { get; set; }
    public string? Status { get; set; }
    public LegacyAccess? Access { get; set; }
}
