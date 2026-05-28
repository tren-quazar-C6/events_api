using events_api.Data;
using events_api.DTOs;
using events_api.Entities;
using events_api.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

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

    /// <summary>
    /// Lista todos los empleados.
    /// Filtra por rol o por nombre/email con el parámetro busqueda.
    /// Por defecto solo muestra activos, con soloActivos=false muestra todos.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ServiceResponse<IReadOnlyCollection<EmpleadoResumenDto>>>> GetEmpleados(
        [FromQuery] int? id_rol_staff,
        [FromQuery] string? busqueda,
        [FromQuery] bool soloActivos = true,
        CancellationToken cancellationToken = default)
    {
        var query = _db.STAFF.AsNoTracking();

        if (soloActivos)
            query = query.Where(s => s.activo == true);

        if (id_rol_staff is not null)
            query = query.Where(s => s.id_rol_staff == id_rol_staff);

        if (!string.IsNullOrWhiteSpace(busqueda))
            query = query.Where(s =>
                s.nombre.Contains(busqueda) ||
                s.email.Contains(busqueda));

        var empleados = await query
            .OrderBy(s => s.nombre)
            .Select(s => new EmpleadoResumenDto(
                s.id_staff,
                s.nombre,
                s.email,
                s.id_rol_staffNavigation.nombre_rol,
                s.activo ?? true))
            .ToListAsync(cancellationToken);

        return Ok(ServiceResponse<IReadOnlyCollection<EmpleadoResumenDto>>.Ok(empleados));
    }

    // ============================================================
    // GET /api/admin/empleados/{id}
    // ============================================================

    /// <summary>
    /// Detalle de un empleado por ID.
    /// Nunca devuelve el password_hash.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ServiceResponse<EmpleadoDetalleDto>>> GetEmpleado(
        int id,
        CancellationToken cancellationToken = default)
    {
        var empleado = await _db.STAFF
            .AsNoTracking()
            .Where(s => s.id_staff == id)
            .Select(s => new EmpleadoDetalleDto(
                s.id_staff,
                s.nombre,
                s.email,
                s.id_rol_staff,
                s.id_rol_staffNavigation.nombre_rol,
                s.activo ?? true,
                s.fecha_registro))
            .FirstOrDefaultAsync(cancellationToken);

        return empleado is null
            ? NotFound(ServiceResponse<EmpleadoDetalleDto>.Fail("Empleado no encontrado"))
            : Ok(ServiceResponse<EmpleadoDetalleDto>.Ok(empleado));
    }

    // ============================================================
    // POST /api/admin/empleados
    // ============================================================

    /// <summary>
    /// Crea un empleado nuevo.
    ///
    /// El password se hashea con SHA-256 antes de guardarse.
    /// Cuando implementemos JWT, cambiar a BCrypt o Argon2.
    ///
    /// Valida que el email no esté duplicado.
    /// Valida que el rol exista y esté activo.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ServiceResponse<EmpleadoDetalleDto>>> CreateEmpleado(
        [FromBody] CreateEmpleadoRequest request,
        CancellationToken cancellationToken = default)
    {
        // Validar que el rol existe
        var rolExiste = await _db.ROL_STAFFs
            .AnyAsync(r => r.id_rol_staff == request.id_rol_staff && r.activo == true,
                cancellationToken);

        if (!rolExiste)
            return BadRequest(ServiceResponse<EmpleadoDetalleDto>
                .Fail("El rol no existe o está inactivo"));

        // Validar email único
        var emailExiste = await _db.STAFF
            .AnyAsync(s => s.email == request.email, cancellationToken);

        if (emailExiste)
            return BadRequest(ServiceResponse<EmpleadoDetalleDto>
                .Fail("Ya existe un empleado con ese email"));

        var empleado = new STAFF
        {
            id_rol_staff   = request.id_rol_staff,
            nombre         = request.nombre,
            email          = request.email,
            password_hash  = HashPassword(request.password),
            activo         = true,
            fecha_registro = DateTime.UtcNow
        };

        _db.STAFF.Add(empleado);
        await _db.SaveChangesAsync(cancellationToken);

        // Traer el DTO completo con el nombre del rol
        var dto = await _db.STAFF
            .AsNoTracking()
            .Where(s => s.id_staff == empleado.id_staff)
            .Select(s => new EmpleadoDetalleDto(
                s.id_staff,
                s.nombre,
                s.email,
                s.id_rol_staff,
                s.id_rol_staffNavigation.nombre_rol,
                s.activo ?? true,
                s.fecha_registro))
            .FirstAsync(cancellationToken);

        return CreatedAtAction(
            nameof(GetEmpleado),
            new { id = empleado.id_staff },
            ServiceResponse<EmpleadoDetalleDto>.Ok(dto));
    }

    // ============================================================
    // PUT /api/admin/empleados/{id}
    // ============================================================

    /// <summary>
    /// Edita un empleado.
    /// Solo actualiza los campos que llegan con valor.
    /// Si password viene, se hashea y actualiza.
    /// Si password no viene, el password actual no se toca.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ServiceResponse<object>>> UpdateEmpleado(
        int id,
        [FromBody] UpdateEmpleadoRequest request,
        CancellationToken cancellationToken = default)
    {
        var empleado = await _db.STAFF
            .FirstOrDefaultAsync(s => s.id_staff == id, cancellationToken);

        if (empleado is null)
            return NotFound(ServiceResponse<object>.Fail("Empleado no encontrado"));

        // Validar rol si viene
        if (request.id_rol_staff is not null)
        {
            var rolExiste = await _db.ROL_STAFFs
                .AnyAsync(r => r.id_rol_staff == request.id_rol_staff && r.activo == true,
                    cancellationToken);

            if (!rolExiste)
                return BadRequest(ServiceResponse<object>.Fail("El rol no existe o está inactivo"));

            empleado.id_rol_staff = request.id_rol_staff.Value;
        }

        // Validar email único si viene y es diferente al actual
        if (request.email is not null && request.email != empleado.email)
        {
            var emailExiste = await _db.STAFF
                .AnyAsync(s => s.email == request.email && s.id_staff != id,
                    cancellationToken);

            if (emailExiste)
                return BadRequest(ServiceResponse<object>.Fail("Ya existe un empleado con ese email"));

            empleado.email = request.email;
        }

        if (request.nombre   is not null) empleado.nombre        = request.nombre;
        if (request.password is not null) empleado.password_hash = HashPassword(request.password);

        await _db.SaveChangesAsync(cancellationToken);

        return Ok(ServiceResponse<object>.Ok("Empleado actualizado correctamente"));
    }

    // ============================================================
    // DELETE /api/admin/empleados/{id}
    // ============================================================

    /// <summary>
    /// Soft delete: pone activo = false.
    /// No borra el registro para preservar historial de eventos y ventas.
    /// No permite desactivar al propio empleado autenticado.
    /// (La validación del "propio empleado" se hará cuando haya JWT)
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ServiceResponse<object>>> DeleteEmpleado(
        int id,
        CancellationToken cancellationToken = default)
    {
        var empleado = await _db.STAFF
            .FirstOrDefaultAsync(s => s.id_staff == id, cancellationToken);

        if (empleado is null)
            return NotFound(ServiceResponse<object>.Fail("Empleado no encontrado"));

        if (empleado.activo == false)
            return BadRequest(ServiceResponse<object>.Fail("El empleado ya está inactivo"));

        empleado.activo = false;
        await _db.SaveChangesAsync(cancellationToken);

        return Ok(ServiceResponse<object>.Ok("Empleado desactivado correctamente"));
    }

    // ============================================================
    // GET /api/admin/roles
    // ============================================================

    /// <summary>
    /// Lista todos los roles disponibles.
    /// Se usa para llenar el dropdown al crear/editar un empleado.
    /// </summary>
    [HttpGet("/api/admin/roles")]
    public async Task<ActionResult<ServiceResponse<IReadOnlyCollection<RolStaffDto>>>> GetRoles(
        CancellationToken cancellationToken = default)
    {
        var roles = await _db.ROL_STAFFs
            .AsNoTracking()
            .Where(r => r.activo == true)
            .OrderBy(r => r.nombre_rol)
            .Select(r => new RolStaffDto(
                r.id_rol_staff,
                r.nombre_rol,
                r.activo ?? true))
            .ToListAsync(cancellationToken);

        return Ok(ServiceResponse<IReadOnlyCollection<RolStaffDto>>.Ok(roles));
    }

    // ============================================================
    // MÉTODOS PRIVADOS
    // ============================================================

    /// <summary>
    /// Hashea un password con SHA-256.
    ///
    /// IMPORTANTE: SHA-256 es suficiente para el proyecto académico.
    /// En producción real usar BCrypt o Argon2 que incluyen salt automático
    /// y son resistentes a ataques de fuerza bruta.
    /// </summary>
    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLower();
    }
}