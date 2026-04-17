using Microsoft.EntityFrameworkCore;
using OxfordSchool.API.Models;

namespace OxfordSchool.API.Data;

public class OxfordSchoolDbContext(DbContextOptions<OxfordSchoolDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Materia> Materias => Set<Materia>();
    public DbSet<Tarea> Tareas => Set<Tarea>();
    public DbSet<Entrega> Entregas => Set<Entrega>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.Correo)
            .IsUnique();

        modelBuilder.Entity<Materia>()
            .HasOne(m => m.Docente)
            .WithMany(u => u.MateriasCreadas)
            .HasForeignKey(m => m.DocenteId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Entrega>()
            .HasOne(e => e.Estudiante)
            .WithMany(u => u.Entregas)
            .HasForeignKey(e => e.EstudianteId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Entrega>()
            .HasIndex(e => new { e.EstudianteId, e.TareaId })
            .IsUnique();
    }
}
