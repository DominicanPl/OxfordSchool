using System.ComponentModel.DataAnnotations;

namespace OxfordSchool.API.Models;

public class Tarea
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(120)]
    public string Titulo { get; set; } = string.Empty;

    [MaxLength(1200)]
    public string Descripcion { get; set; } = string.Empty;

    [Required]
    public DateTime FechaLimite { get; set; }

    public int MateriaId { get; set; }
    public Materia? Materia { get; set; }

    public ICollection<Entrega> Entregas { get; set; } = new List<Entrega>();
}
