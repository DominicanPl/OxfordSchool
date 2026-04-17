using System.ComponentModel.DataAnnotations;

namespace OxfordSchool.API.DTOs;

public class CalificarEntregaRequest
{
    [Range(0, 100)]
    public int Calificacion { get; set; }

    public string? ComentarioDocente { get; set; }
}
