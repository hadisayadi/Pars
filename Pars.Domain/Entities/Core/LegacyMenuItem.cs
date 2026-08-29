namespace Pars.Domain.Entities.Core;

public class LegacyMenuItem
{
    public int Id { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public decimal? Calories { get; set; }
    public byte? Kind { get; set; }
    public string? AddBy { get; set; }
    public string? Days { get; set; }
}
