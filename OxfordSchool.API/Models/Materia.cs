using System.ComponentModel.DataAnnotations;

namespace OxfordSchool.API.Models;

public class Materia
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(120)]
    public string Nombre { get; set; } = string.Empty;

    public int DocenteId { get; set; }
    public Usuario? Docente { get; set; }

    public ICollection<Tarea> Tareas { get; set; } = new List<Tarea>();
}
