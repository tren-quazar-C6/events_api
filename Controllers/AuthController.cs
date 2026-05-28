using events_api.DTOs;
using events_api.Services;
using Microsoft.AspNetCore.Mvc;

namespace events_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : Controller
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("registrar-prueba")]
    public async Task<IActionResult> RegistrarPrueba()
    {
        // Esto registrará un usuario limpio sobreescribiendo el formato del hash
        // bool creado = await _authService.RegistrarStaffAsync(
        //     "Taquilla Jose Generado", 
        //     "jose@quasar.com", 
        //     "taquillahash", 
        //     2
        // );
        
        // Esto registrará un usuario limpio sobreescribiendo el formato del hash
        bool creado = await _authService.RegistrarStaffAsync(
            "Admin Manuel Generado", 
            "manuel@quasar.com", 
            "admin123", 
            1
        );

        if (!creado) return BadRequest("No se pudo crear o el correo ya existe");
        return Ok("Usuario de taquilla creado con hash nativo de .NET");
    }

    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var resultado = await _authService.LoginAsync(request);

        if (resultado == null)
            return Unauthorized(new { mensaje = "Credenciales incorrectas o usuario inactivo" });

        return Ok(resultado);
    }
}