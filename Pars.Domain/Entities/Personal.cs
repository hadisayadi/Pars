
namespace Pars.Domain.Entities;

public class Personal
{
    public string Id { get; set; } = default!;

    public string? UnitCode { get; set; }

    public string? Taradod { get; set; }

    public string? Estekhdam { get; set; }

    public string? DateEstekhdam { get; set; }

    public string? DateTavalod { get; set; }

    public string? Madrak { get; set; }

    public string? Reshte { get; set; }

    public string? Gerayesh { get; set; }

    public string? University { get; set; }

    public string? Jensiat { get; set; }

    public string? TavalodCity { get; set; }

    public string? SokonatCity { get; set; }

    public string? TelKar { get; set; }

    public string? TelMob { get; set; }

    public string? Email { get; set; }

    public string? UnitCodeTemp { get; set; }

    public string? Company { get; set; }

    public string? Pos { get; set; }

    public int? PosIndex { get; set; }

    public string? AddBy { get; set; }

    public int? Level1 { get; set; }

    public string? Shift { get; set; }

    public bool? NobatKar { get; set; }

    public string? SizeShalvar { get; set; }

    public string? SizeKafsh { get; set; }

    public string? SizeLebas { get; set; }

    public string? SizeBlarsoot { get; set; }

    public string? SizeKapshan { get; set; }

    public string? Khedmat { get; set; }

    public string? EllatMoafiyat { get; set; }

    public string? CodeMelli { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? FatherName { get; set; }

    public int? CardNo { get; set; }

    // Navigation Properties (بر اساس روابط FK)
    public ICollection<AttendanceEntry> AttendanceEntries { get; set; } = new List<AttendanceEntry>();
    public ICollection<PersonalChild> Children { get; set; } = new List<PersonalChild>();
    public ICollection<PersonalFile> Files { get; set; } = new List<PersonalFile>();
    public ICollection<Pars.Domain.Entities.Attendance.EmployeeGroupPerson> EmployeeGroups { get; set; } = new List<Pars.Domain.Entities.Attendance.EmployeeGroupPerson>();
    public ICollection<Pars.Domain.Entities.Attendance.ManualAttendanceEntry> ManualAttendanceEntries { get; set; } = new List<Pars.Domain.Entities.Attendance.ManualAttendanceEntry>();
    public ICollection<Pars.Domain.Entities.Attendance.MissionEntry> MissionEntries { get; set; } = new List<Pars.Domain.Entities.Attendance.MissionEntry>();
}
