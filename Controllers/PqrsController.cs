using events_api.Data;
using events_api.DTOs;
using events_api.Entities;
using events_api.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace events_api.Controllers;

[ApiController]
[Route("api/pqrs")]
public class PqrsController : ControllerBase
{
    private static readonly HashSet<string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "PREGUNTA",
        "PETICION",
        "QUEJA",
        "RECLAMO",
        "SUGERENCIA",
    };

    private static readonly HashSet<string> AllowedStates = new(StringComparer.OrdinalIgnoreCase)
    {
        "ABIERTO",
        "EN_PROCESO",
        "RESPONDIDO",
        "CERRADO",
    };

    private readonly QuasarDbContext _db;

    public PqrsController(QuasarDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<ServiceResponse<IReadOnlyCollection<PublicPqrsResumenDto>>>> GetPqrs(
        [FromQuery] string? estado,
        [FromQuery] string? tipo,
        [FromQuery] int? id_usuario,
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

        if (id_usuario is not null)
            query = query.Where(p => p.id_usuario == id_usuario);

        var pqrs = await query
            .OrderByDescending(p => p.fecha_creacion)
            .Select(p => new PublicPqrsResumenDto(
                p.id_pqrs,
                p.id_usuario,
                p.asignado_staff,
                p.tipo,
                p.asunto,
                p.estado,
                p.fecha_creacion,
                p.fecha_ultima_respuesta,
                p.id_usuarioNavigation.nombre,
                p.asignado_staffNavigation != null ? p.asignado_staffNavigation.nombre : null,
                p.PQRS_MENSAJEs.Count))
            .ToListAsync(cancellationToken);

        return Ok(ServiceResponse<IReadOnlyCollection<PublicPqrsResumenDto>>.Ok(pqrs));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ServiceResponse<PublicPqrsDetalleDto>>> GetPqrs(
        int id,
        CancellationToken cancellationToken = default)
    {
        var dto = await BuildDetalleDtoAsync(id, cancellationToken);

        return dto is null
            ? NotFound(ServiceResponse<PublicPqrsDetalleDto>.Fail("PQRS no encontrada"))
            : Ok(ServiceResponse<PublicPqrsDetalleDto>.Ok(dto));
    }

    [HttpPost]
    public async Task<ActionResult<ServiceResponse<PublicPqrsDetalleDto>>> CreatePqrs(
        [FromBody] PublicPqrsCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        var usuarioExiste = await _db.USUARIOs
            .AsNoTracking()
            .AnyAsync(u => u.id_usuario == request.id_usuario && u.activo == true, cancellationToken);

        if (!usuarioExiste)
            return BadRequest(ServiceResponse<PublicPqrsDetalleDto>.Fail("El usuario no existe o está inactivo"));

        if (!AllowedTypes.Contains(request.tipo))
            return BadRequest(ServiceResponse<PublicPqrsDetalleDto>.Fail("Tipo de PQRS inválido"));

        if (string.IsNullOrWhiteSpace(request.asunto) || request.asunto.Length > 255)
            return BadRequest(ServiceResponse<PublicPqrsDetalleDto>.Fail("El asunto es obligatorio y debe tener máximo 255 caracteres"));

        if (string.IsNullOrWhiteSpace(request.mensaje))
            return BadRequest(ServiceResponse<PublicPqrsDetalleDto>.Fail("El mensaje es obligatorio"));

        var pqr = new PQR
        {
            id_usuario = request.id_usuario,
            tipo = NormalizeType(request.tipo),
            asunto = request.asunto.Trim(),
            estado = "ABIERTO",
            fecha_creacion = DateTime.UtcNow,
        };

        _db.PQRs.Add(pqr);
        await _db.SaveChangesAsync(cancellationToken);

        _db.PQRS_MENSAJEs.Add(new PQRS_MENSAJE
        {
            id_pqrs = pqr.id_pqrs,
            remitente = "USUARIO",
            id_remitente = request.id_usuario,
            mensaje = request.mensaje.Trim(),
            fecha = DateTime.UtcNow,
        });

        await _db.SaveChangesAsync(cancellationToken);

        var dto = await BuildDetalleDtoAsync(pqr.id_pqrs, cancellationToken);

        return CreatedAtAction(
            nameof(GetPqrs),
            new { id = pqr.id_pqrs },
            ServiceResponse<PublicPqrsDetalleDto>.Ok(dto!));
    }

    private static string NormalizeType(string tipo)
    {
        return tipo.Equals("PETICION", StringComparison.OrdinalIgnoreCase)
            ? "PREGUNTA"
            : tipo.ToUpperInvariant();
    }

    private async Task<PublicPqrsDetalleDto?> BuildDetalleDtoAsync(int id, CancellationToken cancellationToken)
    {
        var pqr = await _db.PQRs
            .AsNoTracking()
            .Include(p => p.id_usuarioNavigation)
            .Include(p => p.asignado_staffNavigation)
            .Include(p => p.PQRS_MENSAJEs)
            .FirstOrDefaultAsync(p => p.id_pqrs == id, cancellationToken);

        if (pqr is null)
            return null;

        return new PublicPqrsDetalleDto(
            pqr.id_pqrs,
            pqr.id_usuario,
            pqr.asignado_staff,
            pqr.tipo,
            pqr.asunto,
            pqr.estado,
            pqr.fecha_creacion,
            pqr.fecha_ultima_respuesta,
            pqr.id_usuarioNavigation.nombre,
            pqr.asignado_staffNavigation?.nombre,
            pqr.PQRS_MENSAJEs
                .OrderBy(m => m.fecha)
                .Select(m => new PublicPqrsMensajeDto(
                    m.id_mensaje,
                    m.remitente,
                    m.id_remitente,
                    m.mensaje,
                    m.fecha))
                .ToList());
    }
}
