
namespace Pars.Domain.Entities;

public class AttendanceEntry
{
    public long Id { get; set; }

    public string PersonId { get; set; } = default!;

    public DateTime DateTime { get; set; }

    public string? AddBy { get; set; }

    public DateTime? Updated { get; set; }

    public virtual Personal? Personal { get; set; }
}
