using System.ComponentModel.DataAnnotations;

namespace OxfordSchool.API.DTOs;

public class RegisterRequest
{
    [Required]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Correo { get; set; } = string.Empty;

    [Required]
    public string Contrasena { get; set; } = string.Empty;

    [Required]
    public string Rol { get; set; } = string.Empty;
}
