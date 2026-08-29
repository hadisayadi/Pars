namespace Pars.Domain.Entities;

public class PersonalFile
{
    public long Id { get; set; }
    public string? Pid { get; set; }
    public byte[]? FileContent { get; set; }
    public string? FileName { get; set; }
    public string? AddBy { get; set; }
    public string? Nesbat { get; set; }

    public Personal? Personal { get; set; }
}
