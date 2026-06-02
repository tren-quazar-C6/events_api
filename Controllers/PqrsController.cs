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

        if (id_usuario is not null)
            query = query.Where(p => p.IdUsuario == id_usuario);

        var pqrs = await query
            .OrderByDescending(p => p.FechaCreacion)
            .Select(p => new PublicPqrsResumenDto(
                p.IdPqrs,
                p.IdUsuario,
                p.AsignadoStaff,
                p.Tipo,
                p.Asunto,
                p.Estado,
                p.FechaCreacion,
                p.FechaUltimaRespuesta,
                p.IdUsuarioNavigation.Nombre,
                p.AsignadoStaffNavigation != null ? p.AsignadoStaffNavigation.Nombre : null,
                p.PqrsMensajes.Count))
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
        var usuarioExiste = await _db.Usuarios
            .AsNoTracking()
            .AnyAsync(u => u.IdUsuario == request.id_usuario && u.Activo == true, cancellationToken);

        if (!usuarioExiste)
            return BadRequest(ServiceResponse<PublicPqrsDetalleDto>.Fail("El usuario no existe o está inactivo"));

        if (!AllowedTypes.Contains(request.tipo))
            return BadRequest(ServiceResponse<PublicPqrsDetalleDto>.Fail("Tipo de PQRS inválido"));

        if (string.IsNullOrWhiteSpace(request.asunto) || request.asunto.Length > 255)
            return BadRequest(ServiceResponse<PublicPqrsDetalleDto>.Fail("El asunto es obligatorio y debe tener máximo 255 caracteres"));

        if (string.IsNullOrWhiteSpace(request.mensaje))
            return BadRequest(ServiceResponse<PublicPqrsDetalleDto>.Fail("El mensaje es obligatorio"));

        var pqr = new Pqr
        {
            IdUsuario = request.id_usuario,
            Tipo = NormalizeType(request.tipo),
            Asunto = request.asunto.Trim(),
            Estado = "ABIERTO",
            FechaCreacion = DateTime.UtcNow,
        };

        _db.Pqrs.Add(pqr);
        await _db.SaveChangesAsync(cancellationToken);

        _db.PqrsMensajes.Add(new PqrsMensaje
        {
            IdPqrs = pqr.IdPqrs,
            Remitente = "USUARIO",
            IdRemitente = request.id_usuario,
            Mensaje = request.mensaje.Trim(),
            Fecha = DateTime.UtcNow,
        });

        await _db.SaveChangesAsync(cancellationToken);

        var dto = await BuildDetalleDtoAsync(pqr.IdPqrs, cancellationToken);

        return CreatedAtAction(
            nameof(GetPqrs),
            new { id = pqr.IdPqrs },
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
        var pqr = await _db.Pqrs
            .AsNoTracking()
            .Include(p => p.IdUsuarioNavigation)
            .Include(p => p.AsignadoStaffNavigation)
            .Include(p => p.PqrsMensajes)
            .FirstOrDefaultAsync(p => p.IdPqrs == id, cancellationToken);

        if (pqr is null)
            return null;

        return new PublicPqrsDetalleDto(
            pqr.IdPqrs,
            pqr.IdUsuario,
            pqr.AsignadoStaff,
            pqr.Tipo,
            pqr.Asunto,
            pqr.Estado,
            pqr.FechaCreacion,
            pqr.FechaUltimaRespuesta,
            pqr.IdUsuarioNavigation.Nombre,
            pqr.AsignadoStaffNavigation?.Nombre,
            pqr.PqrsMensajes
                .OrderBy(m => m.Fecha)
                .Select(m => new PublicPqrsMensajeDto(
                    m.IdMensaje,
                    m.Remitente,
                    m.IdRemitente,
                    m.Mensaje,
                    m.Fecha))
                .ToList());
    }
}
