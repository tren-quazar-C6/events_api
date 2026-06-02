using events_api.Data;
using events_api.DTOs;
using events_api.Entities;
using events_api.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace events_api.Controllers.Admin;

/// <summary>
/// CRUD de empleados (Staff) para el panel de administración.
///
/// Endpoints:
///   GET    /api/admin/empleados         → listar empleados
///   GET    /api/admin/empleados/{id}    → detalle
///   POST   /api/admin/empleados         → crear empleado
///   PUT    /api/admin/empleados/{id}    → editar
///   DELETE /api/admin/empleados/{id}    → soft delete
///   GET    /api/admin/roles             → listar roles disponibles
///
/// TODO: agregar [Authorize(Roles = "admin")] cuando esté el JWT
/// </summary>
[Authorize]
[ApiController]
[Route("api/admin/empleados")]
public class AdminEmpleadosController : ControllerBase
{
    private readonly QuasarDbContext _db;

    public AdminEmpleadosController(QuasarDbContext db)
    {
        _db = db;
    }

    // ============================================================
    // GET /api/admin/empleados
    // ============================================================
    
    [HttpGet]
    public async Task<ActionResult<ServiceResponse<IReadOnlyCollection<EmpleadoResumenDto>>>> GetEmpleados(
        [FromQuery] int? id_rol_staff,
        [FromQuery] string? busqueda,
        [FromQuery] bool soloActivos = true,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Staff.AsNoTracking();

        if (soloActivos)
            query = query.Where(s => s.Activo == true);

        if (id_rol_staff is not null)
            query = query.Where(s => s.IdRolStaff == id_rol_staff);

        if (!string.IsNullOrWhiteSpace(busqueda))
            query = query.Where(s =>
                s.Nombre.Contains(busqueda) ||
                s.Email.Contains(busqueda));

        var empleados = await query
            .OrderBy(s => s.Nombre)
            .Select(s => new EmpleadoResumenDto(
                s.IdStaff,
                s.Nombre,
                s.Email,
                s.IdRolStaffNavigation.NombreRol,
                s.Activo ?? true))
            .ToListAsync(cancellationToken);

        return Ok(ServiceResponse<IReadOnlyCollection<EmpleadoResumenDto>>.Ok(empleados));
    }

    // ============================================================
    // GET /api/admin/empleados/{id}
    // ============================================================
    
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ServiceResponse<EmpleadoDetalleDto>>> GetEmpleado(
        int id,
        CancellationToken cancellationToken = default)
    {
        var empleado = await _db.Staff
            .AsNoTracking()
            .Where(s => s.IdStaff == id)
            .Select(s => new EmpleadoDetalleDto(
                s.IdStaff,
                s.Nombre,
                s.Email,
                s.IdRolStaff,
                s.IdRolStaffNavigation.NombreRol,
                s.Activo ?? true,
                s.FechaRegistro))
            .FirstOrDefaultAsync(cancellationToken);

        return empleado is null
            ? NotFound(ServiceResponse<EmpleadoDetalleDto>.Fail("Empleado no encontrado"))
            : Ok(ServiceResponse<EmpleadoDetalleDto>.Ok(empleado));
    }

    // ============================================================
    // POST /api/admin/empleados
    // ============================================================
    
    [HttpPost]
    public async Task<ActionResult<ServiceResponse<EmpleadoDetalleDto>>> CreateEmpleado(
        [FromBody] CreateEmpleadoRequest request,
        CancellationToken cancellationToken = default)
    {
        // Validar que el rol existe
        var rolExiste = await _db.RolStaffs
            .AnyAsync(r => r.IdRolStaff == request.id_rol_staff && r.Activo == true,
                cancellationToken);

        if (!rolExiste)
            return BadRequest(ServiceResponse<EmpleadoDetalleDto>
                .Fail("El rol no existe o está inactivo"));

        // Validar email único
        var emailExiste = await _db.Staff
            .AnyAsync(s => s.Email == request.email, cancellationToken);

        if (emailExiste)
            return BadRequest(ServiceResponse<EmpleadoDetalleDto>
                .Fail("Ya existe un empleado con ese email"));

        var empleado = new Staff
        {
            IdRolStaff   = request.id_rol_staff,
            Nombre         = request.nombre,
            Email          = request.email,
            PasswordHash  = HashPassword(request.password),
            Activo         = true,
            FechaRegistro = DateTime.UtcNow
        };

        _db.Staff.Add(empleado);
        await _db.SaveChangesAsync(cancellationToken);

        // Traer el DTO completo con el nombre del rol
        var dto = await _db.Staff
            .AsNoTracking()
            .Where(s => s.IdStaff == empleado.IdStaff)
            .Select(s => new EmpleadoDetalleDto(
                s.IdStaff,
                s.Nombre,
                s.Email,
                s.IdRolStaff,
                s.IdRolStaffNavigation.NombreRol,
                s.Activo ?? true,
                s.FechaRegistro))
            .FirstAsync(cancellationToken);

        return CreatedAtAction(
            nameof(GetEmpleado),
            new { id = empleado.IdStaff },
            ServiceResponse<EmpleadoDetalleDto>.Ok(dto));
    }

    // ============================================================
    // PUT /api/admin/empleados/{id}
    // ============================================================

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ServiceResponse<object>>> UpdateEmpleado(
        int id,
        [FromBody] UpdateEmpleadoRequest request,
        CancellationToken cancellationToken = default)
    {
        var empleado = await _db.Staff
            .FirstOrDefaultAsync(s => s.IdStaff == id, cancellationToken);

        if (empleado is null)
            return NotFound(ServiceResponse<object>.Fail("Empleado no encontrado"));

        // Validar rol si viene
        if (request.id_rol_staff is not null)
        {
            var rolExiste = await _db.RolStaffs
                .AnyAsync(r => r.IdRolStaff == request.id_rol_staff && r.Activo == true,
                    cancellationToken);

            if (!rolExiste)
                return BadRequest(ServiceResponse<object>.Fail("El rol no existe o está inactivo"));

            empleado.IdRolStaff = request.id_rol_staff.Value;
        }

        // Validar email único si viene y es diferente al actual
        if (request.email is not null && request.email != empleado.Email)
        {
            var emailExiste = await _db.Staff
                .AnyAsync(s => s.Email == request.email && s.IdRolStaff != id,
                    cancellationToken);

            if (emailExiste)
                return BadRequest(ServiceResponse<object>.Fail("Ya existe un empleado con ese email"));

            empleado.Email = request.email;
        }

        if (request.nombre   is not null) empleado.Nombre        = request.nombre;
        if (request.password is not null) empleado.PasswordHash = HashPassword(request.password);

        await _db.SaveChangesAsync(cancellationToken);

        return Ok(ServiceResponse<object>.Ok("Empleado actualizado correctamente"));
    }

    // ============================================================
    // DELETE /api/admin/empleados/{id}
    // ============================================================

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ServiceResponse<object>>> DeleteEmpleado(
        int id,
        CancellationToken cancellationToken = default)
    {
        var empleado = await _db.Staff
            .FirstOrDefaultAsync(s => s.IdStaff == id, cancellationToken);

        if (empleado is null)
            return NotFound(ServiceResponse<object>.Fail("Empleado no encontrado"));

        if (empleado.Activo == false)
            return BadRequest(ServiceResponse<object>.Fail("El empleado ya está inactivo"));

        empleado.Activo = false;
        await _db.SaveChangesAsync(cancellationToken);

        return Ok(ServiceResponse<object>.Ok("Empleado desactivado correctamente"));
    }

    // ============================================================
    // GET /api/admin/roles
    // ============================================================

    [HttpGet("/api/admin/roles")]
    public async Task<ActionResult<ServiceResponse<IReadOnlyCollection<RolStaffDto>>>> GetRoles(
        CancellationToken cancellationToken = default)
    {
        var roles = await _db.RolStaffs
            .AsNoTracking()
            .Where(r => r.Activo == true)
            .OrderBy(r => r.NombreRol)
            .Select(r => new RolStaffDto(
                r.IdRolStaff,
                r.NombreRol,
                r.Activo ?? true))
            .ToListAsync(cancellationToken);

        return Ok(ServiceResponse<IReadOnlyCollection<RolStaffDto>>.Ok(roles));
    }

    // ============================================================
    // MÉTODOS PRIVADOS
    // ============================================================
    
    /// Hashea un password con SHA-256.
    
    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLower();
    }
}