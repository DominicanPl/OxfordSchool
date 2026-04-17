using System.ComponentModel.DataAnnotations;

namespace OxfordSchool.API.Models;

public class Usuario
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(120)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(180)]
    public string Correo { get; set; } = string.Empty;

    [Required]
    public string ContrasenaHash { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Rol { get; set; } = string.Empty;

    public ICollection<Materia> MateriasCreadas { get; set; } = new List<Materia>();
    public ICollection<Entrega> Entregas { get; set; } = new List<Entrega>();
}
