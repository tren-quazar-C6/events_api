using events_api.Data;
using events_api.DTOs;
using events_api.Entities;
using events_api.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace events_api.Controllers.Admin;

[ApiController]
[Route("api/admin/pqrs")]
public class AdminPqrsController : ControllerBase
{
    private static readonly HashSet<string> AllowedStates = new(StringComparer.OrdinalIgnoreCase)
    {
        "ABIERTO",
        "EN_PROCESO",
        "RESPONDIDO",
        "CERRADO",
    };

    private readonly QuasarDbContext _db;

    public AdminPqrsController(QuasarDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<ServiceResponse<IReadOnlyCollection<AdminPqrsResumenDto>>>> GetPqrs(
        [FromQuery] string? estado,
        [FromQuery] string? tipo,
        CancellationToken cancellationToken = default)
    {
        var query = _db.PQRs
            .AsNoTracking()
            .Include(p => p.id_usuarioNavigation)
            .Include(p => p.asignado_staffNavigation)
            .Include(p => p.PQRS_MENSAJEs)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(estado))
            query = query.Where(p => p.estado == estado);

        if (!string.IsNullOrWhiteSpace(tipo))
            query = query.Where(p => p.tipo == NormalizeType(tipo));

        var pqrs = await query
            .OrderByDescending(p => p.fecha_creacion)
            .Select(p => new AdminPqrsResumenDto(
                p.id_pqrs,
                p.asunto,
                p.tipo,
                p.estado,
                p.id_usuario,
                p.id_usuarioNavigation.nombre,
                p.id_usuarioNavigation.email,
                p.asignado_staff,
                p.asignado_staffNavigation != null ? p.asignado_staffNavigation.nombre : null,
                p.fecha_creacion,
                p.fecha_ultima_respuesta,
                p.PQRS_MENSAJEs.Count))
            .ToListAsync(cancellationToken);

        return Ok(ServiceResponse<IReadOnlyCollection<AdminPqrsResumenDto>>.Ok(pqrs));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ServiceResponse<AdminPqrsDetalleDto>>> GetPqrs(
        int id,
        CancellationToken cancellationToken = default)
    {
        var dto = await BuildDetalleDtoAsync(id, cancellationToken);

        return dto is null
            ? NotFound(ServiceResponse<AdminPqrsDetalleDto>.Fail("PQRS no encontrada"))
            : Ok(ServiceResponse<AdminPqrsDetalleDto>.Ok(dto));
    }

    [HttpPatch("{id:int}")]
    public async Task<ActionResult<ServiceResponse<AdminPqrsDetalleDto>>> UpdatePqrs(
        int id,
        [FromBody] PublicPqrsUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        var pqr = await _db.PQRs
            .Include(p => p.PQRS_MENSAJEs)
            .FirstOrDefaultAsync(p => p.id_pqrs == id, cancellationToken);

        if (pqr is null)
            return NotFound(ServiceResponse<AdminPqrsDetalleDto>.Fail("PQRS no encontrada"));

        if (request.asignado_staff is not null)
        {
            var staffExiste = await _db.STAFF
                .AnyAsync(s => s.id_staff == request.asignado_staff && s.activo == true, cancellationToken);

            if (!staffExiste)
                return BadRequest(ServiceResponse<AdminPqrsDetalleDto>.Fail("El staff asignado no existe o está inactivo"));

            pqr.asignado_staff = request.asignado_staff;
        }

        if (!string.IsNullOrWhiteSpace(request.estado))
        {
            if (!AllowedStates.Contains(request.estado))
                return BadRequest(ServiceResponse<AdminPqrsDetalleDto>.Fail("Estado de PQRS inválido"));

            pqr.estado = request.estado.ToUpperInvariant();
        }

        if (!string.IsNullOrWhiteSpace(request.mensaje))
        {
            var remitenteStaff = request.id_staff ?? request.asignado_staff;

            if (remitenteStaff is null)
                return BadRequest(ServiceResponse<AdminPqrsDetalleDto>.Fail("Se requiere id_staff para registrar la respuesta"));

            pqr.fecha_ultima_respuesta = DateTime.UtcNow;
            pqr.PQRS_MENSAJEs.Add(new PQRS_MENSAJE
            {
                id_pqrs = pqr.id_pqrs,
                remitente = "STAFF",
                id_remitente = remitenteStaff.Value,
                mensaje = request.mensaje.Trim(),
                fecha = DateTime.UtcNow,
            });
        }

        await _db.SaveChangesAsync(cancellationToken);

        var dto = await BuildDetalleDtoAsync(pqr.id_pqrs, cancellationToken);
        return Ok(ServiceResponse<AdminPqrsDetalleDto>.Ok(dto!));
    }

    private static string NormalizeType(string tipo)
    {
        return tipo.Equals("PETICION", StringComparison.OrdinalIgnoreCase)
            ? "PREGUNTA"
            : tipo.ToUpperInvariant();
    }

    private async Task<AdminPqrsDetalleDto?> BuildDetalleDtoAsync(int id, CancellationToken cancellationToken)
    {
        var pqr = await _db.PQRs
            .AsNoTracking()
            .Include(p => p.id_usuarioNavigation)
            .Include(p => p.asignado_staffNavigation)
            .Include(p => p.PQRS_MENSAJEs)
            .FirstOrDefaultAsync(p => p.id_pqrs == id, cancellationToken);

        if (pqr is null)
            return null;

        return new AdminPqrsDetalleDto(
            pqr.id_pqrs,
            pqr.asunto,
            pqr.tipo,
            pqr.estado,
            pqr.id_usuario,
            pqr.id_usuarioNavigation.nombre,
            pqr.id_usuarioNavigation.email,
            pqr.asignado_staff,
            pqr.asignado_staffNavigation?.nombre,
            pqr.fecha_creacion,
            pqr.fecha_ultima_respuesta,
            pqr.PQRS_MENSAJEs
                .OrderBy(m => m.fecha)
                .Select(m => new AdminPqrsMensajeDto(
                    m.id_mensaje,
                    m.remitente,
                    m.id_remitente,
                    m.remitente == "STAFF"
                        ? (pqr.asignado_staffNavigation != null ? pqr.asignado_staffNavigation.nombre : "STAFF")
                        : pqr.id_usuarioNavigation.nombre,
                    m.mensaje,
                    m.fecha))
                .ToList());
    }
}
