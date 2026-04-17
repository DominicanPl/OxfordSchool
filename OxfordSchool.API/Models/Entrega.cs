using System.ComponentModel.DataAnnotations;

namespace OxfordSchool.API.Models;

public class Entrega
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(600)]
    public string ArchivoAdjuntoUrl { get; set; } = string.Empty;

    public int? Calificacion { get; set; }

    [MaxLength(1200)]
    public string? ComentarioDocente { get; set; }

    public int EstudianteId { get; set; }
    public Usuario? Estudiante { get; set; }

    public int TareaId { get; set; }
    public Tarea? Tarea { get; set; }
}
