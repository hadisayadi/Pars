
namespace Pars.Domain.Entities;

public class PersonalChild
{
    public long Id { get; set; }

    public string Pid { get; set; } = default!;

    public string? Name { get; set; }

    public string? Nesbat { get; set; }

    public string? Date { get; set; }

    public string? Jensiat { get; set; }

    public string? CodeMelli { get; set; }

    public string? AddBy { get; set; }

    public virtual Personal? Personal { get; set; }
}
