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
public class MateriasController(OxfordSchoolDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var materias = await context.Materias
            .Include(m => m.Docente)
            .Select(m => new { m.Id, m.Nombre, m.DocenteId, DocenteNombre = m.Docente != null ? m.Docente.Nombre : "" })
            .ToListAsync();

        return Ok(materias);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var materia = await context.Materias
            .Include(m => m.Docente)
            .Where(m => m.Id == id)
            .Select(m => new { m.Id, m.Nombre, m.DocenteId, DocenteNombre = m.Docente != null ? m.Docente.Nombre : "" })
            .FirstOrDefaultAsync();

        if (materia is null)
        {
            return NotFound(new { message = "Materia no encontrada." });
        }

        return Ok(materia);
    }

    [HttpPost]
    [Authorize(Roles = "Docente")]
    public async Task<IActionResult> Post([FromBody] CreateMateriaRequest request)
    {
        var docenteId = ObtenerUsuarioId();
        if (docenteId is null)
        {
            return Unauthorized(new { message = "No se pudo identificar el usuario autenticado." });
        }

        var docenteExiste = await context.Usuarios.AnyAsync(u => u.Id == docenteId.Value && u.Rol == "Docente");
        if (!docenteExiste)
        {
            return BadRequest(new { message = "Docente no encontrado." });
        }

        var materia = new Materia
        {
            Nombre = request.Nombre,
            DocenteId = docenteId.Value
        };

        context.Materias.Add(materia);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = materia.Id }, new { materia.Id, materia.Nombre, materia.DocenteId });
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Docente")]
    public async Task<IActionResult> Put(int id, [FromBody] CreateMateriaRequest request)
    {
        var docenteId = ObtenerUsuarioId();
        if (docenteId is null)
        {
            return Unauthorized(new { message = "No se pudo identificar el usuario autenticado." });
        }

        var materia = await context.Materias.FirstOrDefaultAsync(m => m.Id == id);
        if (materia is null)
        {
            return NotFound(new { message = "Materia no encontrada." });
        }

        if (materia.DocenteId != docenteId.Value)
        {
            return Forbid();
        }

        materia.Nombre = request.Nombre;
        await context.SaveChangesAsync();

        return Ok(new { message = "Materia actualizada correctamente." });
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Docente")]
    public async Task<IActionResult> Delete(int id)
    {
        var docenteId = ObtenerUsuarioId();
        if (docenteId is null)
        {
            return Unauthorized(new { message = "No se pudo identificar el usuario autenticado." });
        }

        var materia = await context.Materias.FirstOrDefaultAsync(m => m.Id == id);
        if (materia is null)
        {
            return NotFound(new { message = "Materia no encontrada." });
        }

        if (materia.DocenteId != docenteId.Value)
        {
            return Forbid();
        }

        context.Materias.Remove(materia);
        await context.SaveChangesAsync();

        return NoContent();
    }

    private int? ObtenerUsuarioId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(ClaimTypes.Name)
            ?? User.FindFirstValue("sub");

        return int.TryParse(sub, out var id) ? id : null;
    }
}
