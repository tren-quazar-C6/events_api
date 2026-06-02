using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using events_api.Data;
using events_api.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace events_api.Services;

public class EmployeeAuthService
{
    private readonly QuasarDbContext _db;
    private readonly IConfiguration _configuration;

    public EmployeeAuthService(QuasarDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    public async Task<EmployeeLoginResponse?> LoginAsync(
        EmployeeLoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var hashedPassword = HashPassword(request.password);

        var staff = await _db.STAFF
            .AsNoTracking()
            .Where(s => s.email == request.email && s.password_hash == hashedPassword && s.activo == true)
            .Select(s => new
            {
                s.id_staff,
                s.nombre,
                s.email,
                RoleId = s.id_rol_staff,
                RoleName = s.id_rol_staffNavigation.nombre_rol
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (staff is null)
        {
            return null;
        }

        var permissions = await _db.ROLE_PERMISSIONs
            .AsNoTracking()
            .Where(rp => rp.id_rol_staff == staff.RoleId && rp.active == true && rp.id_permissionNavigation.active == true)
            .Select(rp => rp.id_permissionNavigation.code)
            .Distinct()
            .ToListAsync(cancellationToken);

        var expiresAt = DateTime.UtcNow.AddHours(8);
        var token = BuildJwtToken(staff.id_staff, staff.nombre, staff.email, staff.RoleName, permissions, expiresAt);

        return new EmployeeLoginResponse(
            token,
            expiresAt,
            staff.id_staff,
            staff.nombre,
            staff.RoleName,
            permissions);
    }

    public static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private string BuildJwtToken(
        int idStaff,
        string nombre,
        string email,
        string role,
        IReadOnlyCollection<string> permissions,
        DateTime expiresAt)
    {
        var jwtSection = _configuration.GetSection("Jwt");
        var issuer = jwtSection["Issuer"] ?? "events_api";
        var audience = jwtSection["Audience"] ?? "events_api_clients";
        var secret = jwtSection["Secret"] ?? throw new InvalidOperationException("Missing Jwt:Secret in appsettings.");

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, idStaff.ToString()),
            new(ClaimTypes.NameIdentifier, idStaff.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new("name", nombre),
            new(ClaimTypes.Role, role),
            new("role", role)
        };

        claims.AddRange(permissions.Select(permission => new Claim("permission", permission)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
