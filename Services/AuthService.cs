using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using events_api.Data;
using events_api.DTOs;
using events_api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace events_api.Services;

public class AuthService
{
    private readonly QuasarDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(QuasarDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }
    
    public async Task<bool> RegistrarStaffAsync(string nombre, string email, string contrasena, int idRol)
    {
        // Generar hash nativo y totalmente compatible
        string hashSeguro = BCrypt.Net.BCrypt.HashPassword(contrasena);

        var nuevoStaff = new STAFF
        {
            nombre = nombre,
            email = email,
            password_hash = hashSeguro,
            id_rol_staff = idRol,
            activo = true, // Tu BD usa INT para activo
            fecha_registro = DateTime.Now
        };

        await _context.STAFF.AddAsync(nuevoStaff);
        return await _context.SaveChangesAsync() > 0;
    }

    
    // Login Staff
    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
        {
            // 1. Buscar al empleado por correo e incluir su Rol de la BD
            var staff = await _context.STAFF
                .Include(s => s.id_rol_staffNavigation) // Asegura cargar la relación del rol
                .FirstOrDefaultAsync(s => s.email == request.Correo && s.activo == true);

            if (staff == null) return null;

            // 2. Verificar contraseña con BCrypt
            // Nota: En producción, las contraseñas en la BD deben haberse guardado usando BCrypt.HashPassword
            bool contrasenaValida = BCrypt.Net.BCrypt.Verify(request.Contrasena, staff.password_hash);
            if (!contrasenaValida) return null;

            // 3. Generar Claims basados en su rol de base de datos
            var nombreRol = staff.id_rol_staffNavigation?.nombre_rol ?? "Access"; 
            
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, staff.id_staff.ToString()),
                new Claim(ClaimTypes.Name, staff.nombre ?? ""),
                new Claim(ClaimTypes.Email, staff.email ?? ""),
                new Claim(ClaimTypes.Role, nombreRol) // 'Admin', 'Taquilla' o 'Access'
            };

            // 4. Crear el Token JWT
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiracion = DateTime.UtcNow.AddHours(8); // Turno laboral estándar

            var token = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiracion,
                SigningCredentials = creds,
                Issuer = _configuration["JwtSettings:Issuer"],
                Audience = _configuration["JwtSettings:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenCreado = tokenHandler.CreateToken(token);

            return new AuthResponseDto()
            {
                Token = tokenHandler.WriteToken(tokenCreado),
                Nombre = staff.nombre ?? "",
                Correo = staff.email ?? "",
                Rol = nombreRol,
                Expiracion = expiracion
            };
        }
}