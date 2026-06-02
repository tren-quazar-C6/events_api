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
        var query = _db.Pqrs
            .AsNoTracking()
            .Include(p => p.IdUsuarioNavigation)
            .Include(p => p.AsignadoStaffNavigation)
            .Include(p => p.PqrsMensajes)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(estado))
            query = query.Where(p => p.Estado == estado);

        if (!string.IsNullOrWhiteSpace(tipo))
            query = query.Where(p => p.Tipo == NormalizeType(tipo));

        var pqrs = await query
            .OrderByDescending(p => p.FechaCreacion)
            .Select(p => new AdminPqrsResumenDto(
                p.IdPqrs,
                p.Asunto,
                p.Tipo,
                p.Estado,
                p.IdUsuario,
                p.IdUsuarioNavigation.Nombre,
                p.IdUsuarioNavigation.Email,
                p.AsignadoStaff,
                p.AsignadoStaffNavigation != null ? p.AsignadoStaffNavigation.Nombre : null,
                p.FechaCreacion,
                p.FechaUltimaRespuesta,
                p.PqrsMensajes.Count))
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
        var pqr = await _db.Pqrs
            .Include(p => p.PqrsMensajes)
            .FirstOrDefaultAsync(p => p.IdPqrs == id, cancellationToken);

        if (pqr is null)
            return NotFound(ServiceResponse<AdminPqrsDetalleDto>.Fail("PQRS no encontrada"));

        if (request.asignado_staff is not null)
        {
            var staffExiste = await _db.Staff
                .AnyAsync(s => s.IdStaff == request.asignado_staff && s.Activo == true, cancellationToken);

            if (!staffExiste)
                return BadRequest(ServiceResponse<AdminPqrsDetalleDto>.Fail("El staff asignado no existe o está inactivo"));

            pqr.AsignadoStaff = request.asignado_staff;
        }

        if (!string.IsNullOrWhiteSpace(request.estado))
        {
            if (!AllowedStates.Contains(request.estado))
                return BadRequest(ServiceResponse<AdminPqrsDetalleDto>.Fail("Estado de PQRS inválido"));

            pqr.Estado = request.estado.ToUpperInvariant();
        }

        if (!string.IsNullOrWhiteSpace(request.mensaje))
        {
            var remitenteStaff = request.id_staff ?? request.asignado_staff;

            if (remitenteStaff is null)
                return BadRequest(ServiceResponse<AdminPqrsDetalleDto>.Fail("Se requiere id_staff para registrar la respuesta"));

            pqr.FechaUltimaRespuesta = DateTime.UtcNow;
            pqr.PqrsMensajes.Add(new PqrsMensaje
            {
                IdPqrs = pqr.IdPqrs,
                Remitente = "STAFF",
                IdRemitente = remitenteStaff.Value,
                Mensaje = request.mensaje.Trim(),
                Fecha = DateTime.UtcNow,
            });
        }

        await _db.SaveChangesAsync(cancellationToken);

        var dto = await BuildDetalleDtoAsync(pqr.IdPqrs, cancellationToken);
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
        var pqr = await _db.Pqrs
            .AsNoTracking()
            .Include(p => p.IdUsuarioNavigation)
            .Include(p => p.AsignadoStaffNavigation)
            .Include(p => p.PqrsMensajes)
            .FirstOrDefaultAsync(p => p.IdPqrs == id, cancellationToken);

        if (pqr is null)
            return null;

        return new AdminPqrsDetalleDto(
            pqr.IdPqrs,
            pqr.Asunto,
            pqr.Tipo,
            pqr.Estado,
            pqr.IdUsuario,
            pqr.IdUsuarioNavigation.Nombre,
            pqr.IdUsuarioNavigation.Email,
            pqr.AsignadoStaff,
            pqr.AsignadoStaffNavigation?.Nombre,
            pqr.FechaCreacion,
            pqr.FechaUltimaRespuesta,
            pqr.PqrsMensajes
                .OrderBy(m => m.Fecha)
                .Select(m => new AdminPqrsMensajeDto(
                    m.IdMensaje,
                    m.Remitente,
                    m.IdRemitente,
                    m.Remitente == "STAFF"
                        ? (pqr.AsignadoStaffNavigation != null ? pqr.AsignadoStaffNavigation.Nombre : "STAFF")
                        : pqr.IdUsuarioNavigation.Nombre,
                    m.Mensaje,
                    m.Fecha))
                .ToList());
    }
}
