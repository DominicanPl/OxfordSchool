using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OxfordSchool.API.Data;
using OxfordSchool.API.DTOs;
using OxfordSchool.API.Models;

namespace OxfordSchool.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(OxfordSchoolDbContext context, IConfiguration configuration) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (request.Rol is not ("Docente" or "Estudiante"))
        {
            return BadRequest(new { message = "El rol debe ser Docente o Estudiante." });
        }

        var correoExiste = await context.Usuarios.AnyAsync(u => u.Correo == request.Correo);
        if (correoExiste)
        {
            return Conflict(new { message = "El correo ya está registrado." });
        }

        var usuario = new Usuario
        {
            Nombre = request.Nombre,
            Correo = request.Correo,
            Rol = request.Rol,
            ContrasenaHash = BCrypt.Net.BCrypt.HashPassword(request.Contrasena)
        };

        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();

        return Ok(new { message = "Usuario registrado correctamente.", usuario.Id, usuario.Nombre, usuario.Correo, usuario.Rol });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var usuario = await context.Usuarios.FirstOrDefaultAsync(u => u.Correo == request.Correo);
        if (usuario is null || !BCrypt.Net.BCrypt.Verify(request.Contrasena, usuario.ContrasenaHash))
        {
            return Unauthorized(new { message = "Credenciales inválidas." });
        }

        var token = GenerarToken(usuario);
        return Ok(new
        {
            token,
            usuario = new { usuario.Id, usuario.Nombre, usuario.Correo, usuario.Rol }
        });
    }

    private string GenerarToken(Usuario usuario)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, usuario.Correo),
            new(ClaimTypes.Role, usuario.Rol),
            new(ClaimTypes.Name, usuario.Nombre)
        };

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(4),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
