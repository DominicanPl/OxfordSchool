using System.ComponentModel.DataAnnotations;

namespace OxfordSchool.API.DTOs;

public class CreateMateriaRequest
{
    [Required]
    public string Nombre { get; set; } = string.Empty;
}
