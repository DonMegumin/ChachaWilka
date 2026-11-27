using System.ComponentModel.DataAnnotations;

namespace ChachaWilka.ViewModels;

public sealed class UsuarioVM
{
    [Required(ErrorMessage = "Ingresa tu nombre."), StringLength(50)]
    public string Nombre { get; set; } = "";
    [Required, EmailAddress, StringLength(100)]
    public string Correo { get; set; } = "";
    [Required, StringLength(20), Phone]
    public string Telefono { get; set; } = "";
    [Required(ErrorMessage = "Ingresa tu fecha de nacimiento."), DataType(DataType.Date)]
    public DateOnly? Nacimiento { get; set; }
    [Required, StringLength(128, MinimumLength = 12, ErrorMessage = "Usa entre 12 y 128 caracteres."), DataType(DataType.Password)]
    public string Contraseña { get; set; } = "";
    [Required, Compare(nameof(Contraseña), ErrorMessage = "Las contraseñas no coinciden."), DataType(DataType.Password)]
    public string RepiteContraseña { get; set; } = "";
    public bool Consentimiento { get; set; }
}
