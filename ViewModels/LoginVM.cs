using System.ComponentModel.DataAnnotations;

namespace ChachaWilka.ViewModels;

public sealed class LoginVM
{
    [Required, EmailAddress, StringLength(100)]
    public string Correo { get; set; } = "";
    [Required, StringLength(128), DataType(DataType.Password)]
    public string Contraseña { get; set; } = "";
    [StringLength(500)] public string? ReturnUrl { get; set; }
}
