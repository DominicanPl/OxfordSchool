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
public class EntregasController(OxfordSchoolDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var usuarioId = ObtenerUsuarioId();
        var esEstudiante = User.IsInRole("Estudiante");
        var esDocente = User.IsInRole("Docente");

        var entregas = await context.Entregas
            .Include(e => e.Estudiante)
            .Include(e => e.Tarea)
            .ThenInclude(t => t!.Materia)
            .Where(e =>
                (esEstudiante && usuarioId.HasValue && e.EstudianteId == usuarioId.Value)
                || (esDocente && usuarioId.HasValue && e.Tarea != null && e.Tarea.Materia != null && e.Tarea.Materia.DocenteId == usuarioId.Value))
            .Select(e => new
            {
                e.Id,
                e.ArchivoAdjuntoUrl,
                e.Calificacion,
                e.ComentarioDocente,
                e.EstudianteId,
                EstudianteNombre = e.Estudiante != null ? e.Estudiante.Nombre : "",
                e.TareaId,
                TareaTitulo = e.Tarea != null ? e.Tarea.Titulo : ""
            })
            .ToListAsync();

        return Ok(entregas);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var usuarioId = ObtenerUsuarioId();
        if (usuarioId is null)
        {
            return Unauthorized(new { message = "No se pudo identificar el usuario autenticado." });
        }

        var esEstudiante = User.IsInRole("Estudiante");
        var esDocente = User.IsInRole("Docente");

        var entrega = await context.Entregas
            .Include(e => e.Estudiante)
            .Include(e => e.Tarea)
            .ThenInclude(t => t!.Materia)
            .Where(e => e.Id == id)
            .Where(e =>
                (esEstudiante && e.EstudianteId == usuarioId.Value)
                || (esDocente && e.Tarea != null && e.Tarea.Materia != null && e.Tarea.Materia.DocenteId == usuarioId.Value))
            .Select(e => new
            {
                e.Id,
                e.ArchivoAdjuntoUrl,
                e.Calificacion,
                e.ComentarioDocente,
                e.EstudianteId,
                EstudianteNombre = e.Estudiante != null ? e.Estudiante.Nombre : "",
                e.TareaId,
                TareaTitulo = e.Tarea != null ? e.Tarea.Titulo : ""
            })
            .FirstOrDefaultAsync();

        if (entrega is null)
        {
            return NotFound(new { message = "Entrega no encontrada." });
        }

        return Ok(entrega);
    }

    [HttpPost]
    [Authorize(Roles = "Estudiante")]
    public async Task<IActionResult> Post([FromBody] CreateEntregaRequest request)
    {
        var estudianteId = ObtenerUsuarioId();
        if (estudianteId is null)
        {
            return Unauthorized(new { message = "No se pudo identificar el usuario autenticado." });
        }

        var estudiante = await context.Usuarios.FirstOrDefaultAsync(u => u.Id == estudianteId.Value && u.Rol == "Estudiante");
        if (estudiante is null)
        {
            return BadRequest(new { message = "Estudiante no encontrado." });
        }

        var tarea = await context.Tareas.FindAsync(request.TareaId);
        if (tarea is null)
        {
            return BadRequest(new { message = "Tarea no encontrada." });
        }

        var existeEntrega = await context.Entregas.AnyAsync(e => e.TareaId == request.TareaId && e.EstudianteId == estudianteId.Value);
        if (existeEntrega)
        {
            return Conflict(new { message = "Ya existe una entrega para esta tarea." });
        }

        var entrega = new Entrega
        {
            ArchivoAdjuntoUrl = request.ArchivoAdjuntoUrl,
            EstudianteId = estudianteId.Value,
            TareaId = request.TareaId
        };

        context.Entregas.Add(entrega);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = entrega.Id }, new
        {
            entrega.Id,
            entrega.ArchivoAdjuntoUrl,
            entrega.Calificacion,
            entrega.ComentarioDocente,
            entrega.EstudianteId,
            entrega.TareaId
        });
    }

    [HttpPut("{id:int}/calificar")]
    [Authorize(Roles = "Docente")]
    public async Task<IActionResult> Calificar(int id, [FromBody] CalificarEntregaRequest request)
    {
        var docenteId = ObtenerUsuarioId();
        if (docenteId is null)
        {
            return Unauthorized(new { message = "No se pudo identificar el usuario autenticado." });
        }

        var entrega = await context.Entregas
            .Include(e => e.Tarea)
            .ThenInclude(t => t!.Materia)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (entrega is null)
        {
            return NotFound(new { message = "Entrega no encontrada." });
        }

        if (entrega.Tarea?.Materia is null)
        {
            return BadRequest(new { message = "La entrega no tiene materia asociada." });
        }

        if (entrega.Tarea.Materia.DocenteId != docenteId.Value)
        {
            return Forbid();
        }

        entrega.Calificacion = request.Calificacion;
        entrega.ComentarioDocente = request.ComentarioDocente;
        await context.SaveChangesAsync();

        return Ok(new { message = "Entrega calificada correctamente." });
    }

    [HttpGet("mis-calificaciones")]
    [Authorize(Roles = "Estudiante")]
    public async Task<IActionResult> MisCalificaciones()
    {
        var estudianteId = ObtenerUsuarioId();
        if (estudianteId is null)
        {
            return Unauthorized(new { message = "No se pudo identificar el usuario autenticado." });
        }

        var calificaciones = await context.Entregas
            .Include(e => e.Tarea)
            .Where(e => e.EstudianteId == estudianteId.Value && e.Calificacion.HasValue)
            .OrderByDescending(e => e.Id)
            .Select(e => new
            {
                e.Id,
                e.TareaId,
                TareaTitulo = e.Tarea != null ? e.Tarea.Titulo : "",
                e.Calificacion,
                e.ComentarioDocente
            })
            .ToListAsync();

        return Ok(calificaciones);
    }

    private int? ObtenerUsuarioId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(ClaimTypes.Name)
            ?? User.FindFirstValue("sub");

        return int.TryParse(sub, out var id) ? id : null;
    }
}
