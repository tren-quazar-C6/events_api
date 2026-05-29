using events_api.Data;
using events_api.DTOs;
using events_api.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace events_api.Controllers;

/// <summary>
/// Autenticación de Staff para el panel admin.
///
/// Endpoints:
///   POST /api/auth/login  → recibe email + password, devuelve JWT
///
/// El JWT contiene:
///   - id_staff
///   - nombre
///   - email
///   - rol (nombre del rol del staff)
///
/// El token se manda en cada request protegido como:
///   Authorization: Bearer {token}
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly QuasarDbContext _db;
    private readonly IConfiguration _config;

    public AuthController(QuasarDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    // ============================================================
    // POST /api/auth/login
    // ============================================================

    /// <summary>
    /// Login de staff.
    ///
    /// 1. Busca el staff por email
    /// 2. Verifica el password hasheado con SHA-256
    /// 3. Genera un JWT con los datos del staff
    /// 4. Devuelve el token
    ///
    /// El mensaje de error es genérico ("credenciales inválidas")
    /// para no revelar si el email existe o no.
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<ServiceResponse<LoginResponse>>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        // Buscar staff por email incluyendo su rol
        var staff = await _db.STAFF
            .AsNoTracking()
            .Include(s => s.id_rol_staffNavigation)
            .FirstOrDefaultAsync(s => s.email == request.email && s.activo == true,
                cancellationToken);

        // Email no existe o staff inactivo
        if (staff is null)
            return Unauthorized(ServiceResponse<LoginResponse>
                .Fail("Credenciales inválidas"));

        // Verificar password
        var passwordHash = HashPassword(request.password);
        if (staff.password_hash != passwordHash)
            return Unauthorized(ServiceResponse<LoginResponse>
                .Fail("Credenciales inválidas"));

        // Generar JWT
        var (token, expira) = GenerarToken(staff.id_staff, staff.nombre, staff.email,
            staff.id_rol_staffNavigation.nombre_rol);

        var response = new LoginResponse(
            token,
            expira,
            staff.id_staff,
            staff.nombre,
            staff.email,
            staff.id_rol_staffNavigation.nombre_rol
        );

        return Ok(ServiceResponse<LoginResponse>.Ok(response));
    }

    // ============================================================
    // MÉTODOS PRIVADOS
    // ============================================================

    /// <summary>
    /// Genera un JWT con los datos del staff.
    ///
    /// Claims incluidos en el token:
    ///   - JwtRegisteredClaimNames.Sub  → id_staff (como string)
    ///   - JwtRegisteredClaimNames.Name → nombre del staff
    ///   - JwtRegisteredClaimNames.Email → email
    ///   - ClaimTypes.Role              → nombre del rol
    ///
    /// Los claims permiten que los controllers lean el id_staff
    /// del token sin consultar la BD en cada request.
    /// </summary>
    private (string token, DateTime expira) GenerarToken(
        int idStaff,
        string nombre,
        string email,
        string rol)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiraHoras = int.Parse(_config["Jwt:ExpirationHours"] ?? "8");
        var expira = DateTime.UtcNow.AddHours(expiraHoras);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   idStaff.ToString()),
            new Claim(JwtRegisteredClaimNames.Name,  nombre),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(ClaimTypes.Role,               rol),
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer:   _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims:   claims,
            expires:  expira,
            signingCredentials: credentials
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expira);
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLower();
    }
}