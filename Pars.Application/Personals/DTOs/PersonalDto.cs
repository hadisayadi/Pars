using System.ComponentModel.DataAnnotations;

namespace Pars.Application.Personals.DTOs;

public record PersonalDto(
    string Id,
    string? FirstName,
    string? LastName,
    string? FatherName,
    string? CodeMelli,
    string? TelMob,
    string? Email,
    string? Company,
    string? Pos,
    string? DateEstekhdam
);

public sealed class CreatePersonalDto
{
    [Required(ErrorMessage = "کد پرسنلی الزامی است."), StringLength(10, MinimumLength = 1)]
    public string Id { get; set; } = "";
    [StringLength(50)] public string? FirstName { get; set; }
    [StringLength(50)] public string? LastName { get; set; }
    [StringLength(50)] public string? FatherName { get; set; }
    [RegularExpression(@"^\d{10}$", ErrorMessage = "کد ملی باید ۱۰ رقم باشد.")] public string? CodeMelli { get; set; }
    [StringLength(50)] public string? TelMob { get; set; }
    [EmailAddress, StringLength(80)] public string? Email { get; set; }
    [StringLength(50)] public string? Company { get; set; }
    [StringLength(50)] public string? Pos { get; set; }
}
