using System.ComponentModel.DataAnnotations;

namespace OxfordSchool.API.DTOs;

public class CreateTareaRequest
{
    [Required]
    public string Titulo { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    [Required]
    public DateTime FechaLimite { get; set; }

    [Required]
    public int MateriaId { get; set; }
}
