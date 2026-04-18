using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using OxfordSchool.API.Data;
using OxfordSchool.API.DTOs;
using OxfordSchool.API.Models;

namespace OxfordSchool.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TareasController(OxfordSchoolDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int? materiaId)
    {
        var estudianteId = ObtenerUsuarioId();
        var esEstudiante = User.IsInRole("Estudiante");

        var tareas = await context.Tareas
            .Include(t => t.Materia)
            .Where(t => !materiaId.HasValue || t.MateriaId == materiaId.Value)
            .OrderBy(t => t.FechaLimite)
            .Select(t => new
            {
                t.Id,
                t.Titulo,
                t.Descripcion,
                t.FechaLimite,
                t.MateriaId,
                MateriaNombre = t.Materia != null ? t.Materia.Nombre : "",
                Estado = esEstudiante && estudianteId.HasValue
                    ? (context.Entregas.Any(e => e.TareaId == t.Id && e.EstudianteId == estudianteId.Value)
                        ? "Entregada"
                        : (t.FechaLimite < DateTime.UtcNow ? "Tarde" : "Pendiente"))
                    : null
            })
            .ToListAsync();

        return Ok(tareas);
    }

    [HttpPost]
    [Authorize(Roles = "Docente")]
    public async Task<IActionResult> Post([FromBody] CreateTareaRequest request)
    {
        var docenteId = ObtenerUsuarioId();
        if (docenteId is null)
        {
            return Unauthorized(new { message = "No se pudo identificar el usuario autenticado." });
        }

        if (request.FechaLimite <= DateTime.UtcNow)
        {
            return BadRequest(new { message = "La fecha límite debe ser mayor a la fecha actual." });
        }

        var materia = await context.Materias.FirstOrDefaultAsync(m => m.Id == request.MateriaId);
        if (materia is null)
        {
            return BadRequest(new { message = "Materia no encontrada." });
        }

        if (materia.DocenteId != docenteId.Value)
        {
            return Forbid();
        }

        var tarea = new Tarea
        {
            Titulo = request.Titulo,
            Descripcion = request.Descripcion,
            FechaLimite = request.FechaLimite,
            MateriaId = request.MateriaId
        };

        context.Tareas.Add(tarea);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = tarea.Id }, tarea);
    }

    private int? ObtenerUsuarioId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(ClaimTypes.Name)
            ?? User.FindFirstValue("sub");

        return int.TryParse(sub, out var id) ? id : null;
    }
}
