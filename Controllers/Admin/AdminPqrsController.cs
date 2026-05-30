using events_api.Data;
using events_api.DTOs;
using events_api.Entities;
using events_api.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace events_api.Controllers.Admin;

/// <summary>
/// Gestión de PQRS para el panel de administración.
///
/// Endpoints:
///   GET    /api/admin/pqrs                    → listar con filtros
///   GET    /api/admin/pqrs/{id}               → detalle con hilo de mensajes
///   POST   /api/admin/pqrs/{id}/responder      → staff envía mensaje
///   PATCH  /api/admin/pqrs/{id}/status         → cambiar estado
///   PATCH  /api/admin/pqrs/{id}/asignar        → asignar a un staff
/// </summary>
[Authorize]
[ApiController]
[Route("api/admin/pqrs")]
public class AdminPqrsController : ControllerBase
{
    private readonly QuasarDbContext _db;

    public AdminPqrsController(QuasarDbContext db)
    {
        _db = db;
    }

    // ============================================================
    // GET /api/admin/pqrs
    // ============================================================

    /// <summary>
    /// Lista todos los PQRS con filtros opcionales.
    ///
    /// Filtros disponibles:
    ///   - estado: ABIERTO | EN_PROCESO | RESPONDIDO | CERRADO
    ///   - tipo: PREGUNTA | QUEJA | RECLAMO | SUGERENCIA
    ///   - id_staff: filtrar por staff asignado
    ///   - busqueda: busca en asunto y nombre del usuario
    ///
    /// Ordenados por fecha_creacion descendente (más recientes primero).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ServiceResponse<IReadOnlyCollection<PqrsResumenDto>>>> GetPqrs(
        [FromQuery] string? estado,
        [FromQuery] string? tipo,
        [FromQuery] int? id_staff,
        [FromQuery] string? busqueda,
        CancellationToken cancellationToken = default)
    {
        var query = _db.PQRs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(estado))
            query = query.Where(p => p.estado == estado);

        if (!string.IsNullOrWhiteSpace(tipo))
            query = query.Where(p => p.tipo == tipo);

        if (id_staff is not null)
            query = query.Where(p => p.asignado_staff == id_staff);

        if (!string.IsNullOrWhiteSpace(busqueda))
            query = query.Where(p =>
                p.asunto.Contains(busqueda) ||
                p.id_usuarioNavigation.nombre.Contains(busqueda) ||
                p.id_usuarioNavigation.email.Contains(busqueda));

        var pqrs = await query
            .OrderByDescending(p => p.fecha_creacion)
            .Select(p => new PqrsResumenDto(
                p.id_pqrs,
                p.tipo,
                p.asunto,
                p.estado ?? "ABIERTO",
                p.id_usuarioNavigation.nombre,
                p.id_usuarioNavigation.email,
                p.asignado_staffNavigation != null
                    ? p.asignado_staffNavigation.nombre
                    : null,
                p.fecha_creacion,
                p.fecha_ultima_respuesta,
                p.PQRS_MENSAJEs.Count))
            .ToListAsync(cancellationToken);

        return Ok(ServiceResponse<IReadOnlyCollection<PqrsResumenDto>>.Ok(pqrs));
    }

    // ============================================================
    // GET /api/admin/pqrs/{id}
    // ============================================================

    /// <summary>
    /// Detalle completo de un PQRS con el hilo de mensajes.
    ///
    /// El hilo muestra todos los mensajes ordenados por fecha,
    /// con el nombre del remitente (usuario o staff).
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ServiceResponse<PqrsDetalleDto>>> GetPqrs(
        int id,
        CancellationToken cancellationToken = default)
    {
        var pqrs = await _db.PQRs
            .AsNoTracking()
            .Where(p => p.id_pqrs == id)
            .Select(p => new PqrsDetalleDto(
                p.id_pqrs,
                p.tipo,
                p.asunto,
                p.estado ?? "ABIERTO",
                p.id_usuario,
                p.id_usuarioNavigation.nombre,
                p.id_usuarioNavigation.email,
                p.asignado_staff,
                p.asignado_staffNavigation != null
                    ? p.asignado_staffNavigation.nombre
                    : null,
                p.fecha_creacion,
                p.fecha_ultima_respuesta,
                p.PQRS_MENSAJEs
                    .OrderBy(m => m.fecha)
                    .Select(m => new PqrsMensajeDto(
                        m.id_mensaje,
                        m.remitente,
                        m.id_remitente,
                        // Nombre del remitente según si es usuario o staff
                        m.remitente == "STAFF"
                            ? _db.STAFF
                                .Where(s => s.id_staff == m.id_remitente)
                                .Select(s => s.nombre)
                                .FirstOrDefault() ?? "Staff"
                            : _db.USUARIOs
                                .Where(u => u.id_usuario == m.id_remitente)
                                .Select(u => u.nombre)
                                .FirstOrDefault() ?? "Usuario",
                        m.mensaje,
                        m.fecha))
                    .ToList()))
            .FirstOrDefaultAsync(cancellationToken);

        return pqrs is null
            ? NotFound(ServiceResponse<PqrsDetalleDto>.Fail("PQRS no encontrado"))
            : Ok(ServiceResponse<PqrsDetalleDto>.Ok(pqrs));
    }

    // ============================================================
    // POST /api/admin/pqrs/{id}/responder
    // ============================================================

    /// <summary>
    /// El staff envía un mensaje de respuesta al PQRS.
    ///
    /// - Agrega un mensaje con remitente = "STAFF"
    /// - El id del staff viene del JWT (no del body)
    /// - Actualiza fecha_ultima_respuesta
    /// - Cambia el estado a RESPONDIDO automáticamente
    /// - No se puede responder un PQRS CERRADO
    /// </summary>
    [HttpPost("{id:int}/responder")]
    public async Task<ActionResult<ServiceResponse<object>>> Responder(
        int id,
        [FromBody] ResponderPqrsRequest request,
        CancellationToken cancellationToken = default)
    {
        var pqrs = await _db.PQRs
            .FirstOrDefaultAsync(p => p.id_pqrs == id, cancellationToken);

        if (pqrs is null)
            return NotFound(ServiceResponse<object>.Fail("PQRS no encontrado"));

        if (pqrs.estado == "CERRADO")
            return BadRequest(ServiceResponse<object>.Fail(
                "No se puede responder un PQRS cerrado"));

        // Obtener id del staff desde el JWT
        var idStaff = int.Parse(User.FindFirst("sub")?.Value ?? "0");
        
        // Agregar temporalmente para debug
        Console.WriteLine($"DEBUG - idStaff del token: {idStaff}");
        Console.WriteLine($"DEBUG - id_pqrs: {id}");

        // Agregar el mensaje
        _db.PQRS_MENSAJEs.Add(new PQRS_MENSAJE
        {
            id_pqrs      = id,
            remitente    = "STAFF",
            id_remitente = idStaff,
            mensaje      = request.mensaje,
            fecha        = DateTime.UtcNow
        });
        
        // Guardar solo el mensaje
        await _db.SaveChangesAsync(cancellationToken);

        // Actualizar el PQRS
        pqrs.fecha_ultima_respuesta = DateTime.UtcNow;
        pqrs.estado                 = "RESPONDIDO";
        
        
        await _db.SaveChangesAsync(cancellationToken);

        return Ok(ServiceResponse<object>.Ok("Respuesta enviada correctamente"));
    }

    // ============================================================
    // PATCH /api/admin/pqrs/{id}/status
    // ============================================================

    /// <summary>
    /// Cambia el estado de un PQRS.
    ///
    /// Estados válidos: ABIERTO | EN_PROCESO | RESPONDIDO | CERRADO
    ///
    /// Un PQRS CERRADO no puede cambiar de estado.
    /// </summary>
    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<ServiceResponse<object>>> UpdateStatus(
        int id,
        [FromBody] UpdatePqrsStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var pqrs = await _db.PQRs
            .FirstOrDefaultAsync(p => p.id_pqrs == id, cancellationToken);

        if (pqrs is null)
            return NotFound(ServiceResponse<object>.Fail("PQRS no encontrado"));

        if (pqrs.estado == "CERRADO")
            return BadRequest(ServiceResponse<object>.Fail(
                "Un PQRS cerrado no puede cambiar de estado"));

        pqrs.estado = request.estado;
        await _db.SaveChangesAsync(cancellationToken);

        return Ok(ServiceResponse<object>.Ok($"Estado actualizado a {request.estado}"));
    }

    // ============================================================
    // PATCH /api/admin/pqrs/{id}/asignar
    // ============================================================

    /// <summary>
    /// Asigna el PQRS a un staff específico.
    /// Útil cuando hay varios admins y se quiere delegar un caso.
    /// </summary>
    [HttpPatch("{id:int}/asignar")]
    public async Task<ActionResult<ServiceResponse<object>>> Asignar(
        int id,
        [FromBody] AsignarPqrsRequest request,
        CancellationToken cancellationToken = default)
    {
        var pqrs = await _db.PQRs
            .FirstOrDefaultAsync(p => p.id_pqrs == id, cancellationToken);

        if (pqrs is null)
            return NotFound(ServiceResponse<object>.Fail("PQRS no encontrado"));

        if (pqrs.estado == "CERRADO")
            return BadRequest(ServiceResponse<object>.Fail(
                "No se puede reasignar un PQRS cerrado"));

        // Validar que el staff existe y está activo
        var staffExiste = await _db.STAFF
            .AnyAsync(s => s.id_staff == request.id_staff && s.activo == true,
                cancellationToken);

        if (!staffExiste)
            return BadRequest(ServiceResponse<object>.Fail(
                "El staff no existe o está inactivo"));

        pqrs.asignado_staff = request.id_staff;

        // Si estaba ABIERTO, pasa a EN_PROCESO al asignarse
        if (pqrs.estado == "ABIERTO")
            pqrs.estado = "EN_PROCESO";

        await _db.SaveChangesAsync(cancellationToken);

        return Ok(ServiceResponse<object>.Ok("PQRS asignado correctamente"));
    }
}