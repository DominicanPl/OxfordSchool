using System.ComponentModel.DataAnnotations;

namespace OxfordSchool.API.DTOs;

public class CreateEntregaRequest
{
    [Required]
    [Url]
    public string ArchivoAdjuntoUrl { get; set; } = string.Empty;

    [Required]
    public int TareaId { get; set; }
}
