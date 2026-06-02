using events_api.Data;
using events_api.DTOs;
using events_api.Entities;
using events_api.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace events_api.Controllers.Admin;

[Authorize]
[ApiController]
[Route("api/admin/eventos")]
public class AdminEventosController : ControllerBase
{
    private readonly QuasarDbContext _db;

    public AdminEventosController(QuasarDbContext db)
    {
        _db = db;
    }

    // GET /api/admin/eventos
    [HttpGet]
    public async Task<ActionResult<ServiceResponse<IReadOnlyCollection<AdminEventoResumenDto>>>> GetEventos(
        [FromQuery] int? id_tipo_evento,
        [FromQuery] string? busqueda,
        [FromQuery] string? status,
        CancellationToken cancellationToken = default)
    {
        var query = _db.EVENTOs
            .AsNoTracking()
            .Where(e => e.activo == true);

        if (id_tipo_evento is not null)
            query = query.Where(e => e.id_tipo_evento == id_tipo_evento);

        if (!string.IsNullOrWhiteSpace(busqueda))
            query = query.Where(e =>
                e.nombre_evento.Contains(busqueda) ||
                (e.descripcion != null && e.descripcion.Contains(busqueda)));

        query = status switch
        {
            "DRAFT"     => query.Where(e => e.publicado == false && e.fecha_cancelacion == null),
            "PUBLISHED" => query.Where(e => e.publicado == true  && e.fecha_cancelacion == null),
            "CANCELLED" => query.Where(e => e.fecha_cancelacion != null),
            _           => query
        };

        var eventos = await query
            .OrderByDescending(e => e.fecha_creacion)
            .Select(e => new AdminEventoResumenDto(
                e.id_evento,
                e.nombre_evento,
                e.fecha_evento,
                e.fecha_inicio_ventas,
                e.fecha_fin_ventas,
                e.capacidad_total,
                e.id_tipo_eventoNavigation.nombre_tipo,
                e.ruta_url,
                e.fecha_cancelacion != null ? "CANCELLED" :
                e.publicado == true ? "PUBLISHED" : "DRAFT",
                e.EVENTO_ZONAs.Count(ez => ez.activo == true)))
            .ToListAsync(cancellationToken);

        return Ok(ServiceResponse<IReadOnlyCollection<AdminEventoResumenDto>>.Ok(eventos));
    }

    // GET /api/admin/eventos/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ServiceResponse<AdminEventoDetalleDto>>> GetEvento(
        int id,
        CancellationToken cancellationToken = default)
    {
        var dto = await BuildDetalleDtoAsync(id, cancellationToken);

        return dto is null
            ? NotFound(ServiceResponse<AdminEventoDetalleDto>.Fail("Evento no encontrado"))
            : Ok(ServiceResponse<AdminEventoDetalleDto>.Ok(dto));
    }

    // POST /api/admin/eventos
    [HttpPost]
    public async Task<ActionResult<ServiceResponse<AdminEventoDetalleDto>>> CreateEvento(
        [FromBody] CreateEventoRequest request,
        CancellationToken cancellationToken = default)
    {
        var tipoExiste = await _db.TIPO_EVENTOs
            .AnyAsync(t => t.id_tipo_evento == request.id_tipo_evento && t.activo == true,
                cancellationToken);

        if (!tipoExiste)
            return BadRequest(ServiceResponse<AdminEventoDetalleDto>
                .Fail("El tipo de evento no existe o está inactivo"));

        var staffExiste = await _db.STAFF
            .AnyAsync(s => s.id_staff == request.creado_por_staff && s.activo == true,
                cancellationToken);

        if (!staffExiste)
            return BadRequest(ServiceResponse<AdminEventoDetalleDto>
                .Fail("El staff no existe o está inactivo"));

        if (request.fecha_evento <= DateTime.Now)
            return BadRequest(ServiceResponse<AdminEventoDetalleDto>
                .Fail("La fecha del evento debe ser futura"));

        if (request.fecha_inicio_ventas >= request.fecha_fin_ventas)
            return BadRequest(ServiceResponse<AdminEventoDetalleDto>
                .Fail("fecha_inicio_ventas debe ser anterior a fecha_fin_ventas"));

        if (request.fecha_fin_ventas >= request.fecha_evento)
            return BadRequest(ServiceResponse<AdminEventoDetalleDto>
                .Fail("Las ventas deben cerrar antes de la fecha del evento"));

        var evento = new EVENTO
        {
            id_tipo_evento      = request.id_tipo_evento,
            creado_por_staff    = request.creado_por_staff,
            nombre_evento       = request.nombre_evento,
            descripcion         = request.descripcion,
            fecha_evento        = request.fecha_evento,
            fecha_inicio_ventas = request.fecha_inicio_ventas,
            fecha_fin_ventas    = request.fecha_fin_ventas,
            capacidad_total     = request.capacidad_total,
            ruta_url            = request.ruta_url,
            publicado           = true,
            activo              = true,
            fecha_creacion      = DateTime.UtcNow
        };

        _db.EVENTOs.Add(evento);

        if (request.zonas?.Any() == true)
        {
            var id_zonas = request.zonas.Select(z => z.id_zona).ToList();

            var zonasExistentes = await _db.ZONAs
                .Where(z => id_zonas.Contains(z.id_zona) && z.activo == true)
                .Select(z => z.id_zona)
                .ToListAsync(cancellationToken);

            var zonasNoEncontradas = id_zonas.Except(zonasExistentes).ToList();
            if (zonasNoEncontradas.Any())
                return BadRequest(ServiceResponse<AdminEventoDetalleDto>
                    .Fail($"Zonas no encontradas: {string.Join(", ", zonasNoEncontradas)}"));

            var asientosPorZona = await _db.ASIENTOs
                .Where(a => id_zonas.Contains(a.id_zona) && a.activo == true)
                .GroupBy(a => a.id_zona)
                .ToDictionaryAsync(g => g.Key, g => g.ToList(), cancellationToken);

            foreach (var zonaReq in request.zonas)
            {
                evento.EVENTO_ZONAs.Add(new EVENTO_ZONA
                {
                    id_zona        = zonaReq.id_zona,
                    precio         = zonaReq.precio,
                    cargo_servicio = zonaReq.cargo_servicio,
                    capacidad      = zonaReq.capacidad,
                    activo         = true
                });

                if (asientosPorZona.TryGetValue(zonaReq.id_zona, out var asientos))
                {
                    foreach (var asiento in asientos.Take(zonaReq.capacidad))
                    {
                        evento.EVENTO_ASIENTOs.Add(new EVENTO_ASIENTO
                        {
                            id_asiento = asiento.id_asiento,
                            estado     = "DISPONIBLE"
                        });
                    }
                }
            }
        }

        await _db.SaveChangesAsync(cancellationToken);

        await RegistrarAuditoriaAsync(
            id_staff: request.creado_por_staff,
            accion: "CREATE",
            tabla: "EVENTOS",
            id_registro: evento.id_evento,
            detalle: new { evento.nombre_evento, evento.fecha_evento },
            cancellationToken);

        var dto = await BuildDetalleDtoAsync(evento.id_evento, cancellationToken);
        return CreatedAtAction(
            nameof(GetEvento),
            new { id = evento.id_evento },
            ServiceResponse<AdminEventoDetalleDto>.Ok(dto!));
    }

    // PUT /api/admin/eventos/{id}
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ServiceResponse<object>>> UpdateEvento(
        int id,
        [FromBody] UpdateEventoRequest request,
        CancellationToken cancellationToken = default)
    {
        var evento = await _db.EVENTOs
            .FirstOrDefaultAsync(e => e.id_evento == id && e.activo == true, cancellationToken);

        if (evento is null)
            return NotFound(ServiceResponse<object>.Fail("Evento no encontrado"));

        if (evento.fecha_cancelacion is not null)
            return BadRequest(ServiceResponse<object>.Fail("No se puede editar un evento cancelado"));

        if (request.id_tipo_evento is not null)
        {
            var tipoExiste = await _db.TIPO_EVENTOs
                .AnyAsync(t => t.id_tipo_evento == request.id_tipo_evento && t.activo == true,
                    cancellationToken);

            if (!tipoExiste)
                return BadRequest(ServiceResponse<object>.Fail("El tipo de evento no existe"));

            evento.id_tipo_evento = request.id_tipo_evento.Value;
        }

        if (request.nombre_evento       is not null) evento.nombre_evento       = request.nombre_evento;
        if (request.descripcion         is not null) evento.descripcion         = request.descripcion;
        if (request.fecha_evento        is not null) evento.fecha_evento        = request.fecha_evento.Value;
        if (request.fecha_inicio_ventas is not null) evento.fecha_inicio_ventas = request.fecha_inicio_ventas.Value;
        if (request.fecha_fin_ventas    is not null) evento.fecha_fin_ventas    = request.fecha_fin_ventas.Value;
        if (request.capacidad_total     is not null) evento.capacidad_total     = request.capacidad_total.Value;
        if (request.ruta_url            is not null) evento.ruta_url            = request.ruta_url;

        await _db.SaveChangesAsync(cancellationToken);

        await RegistrarAuditoriaAsync(
            id_staff: evento.creado_por_staff,
            accion: "UPDATE",
            tabla: "EVENTOS",
            id_registro: evento.id_evento,
            detalle: new { campos_actualizados = request },
            cancellationToken);

        return Ok(ServiceResponse<object>.Ok("Evento actualizado correctamente"));
    }

    // DELETE /api/admin/eventos/{id}
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ServiceResponse<object>>> DeleteEvento(
        int id,
        CancellationToken cancellationToken = default)
    {
        var evento = await _db.EVENTOs
            .FirstOrDefaultAsync(e => e.id_evento == id && e.activo == true, cancellationToken);

        if (evento is null)
            return NotFound(ServiceResponse<object>.Fail("Evento no encontrado"));

        var tieneVendidos = await _db.EVENTO_ASIENTOs
            .AnyAsync(ea => ea.id_evento == id && ea.estado == "VENDIDO", cancellationToken);

        if (tieneVendidos)
            return BadRequest(ServiceResponse<object>.Fail(
                "No se puede eliminar un evento con tickets vendidos. Use CANCELLED en su lugar."));

        evento.activo = false;
        await _db.SaveChangesAsync(cancellationToken);

        await RegistrarAuditoriaAsync(
            id_staff: evento.creado_por_staff,
            accion: "DELETE",
            tabla: "EVENTOS",
            id_registro: evento.id_evento,
            detalle: new { evento.nombre_evento },
            cancellationToken);

        return Ok(ServiceResponse<object>.Ok("Evento eliminado"));
    }

    // PATCH /api/admin/eventos/{id}/status
    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<ServiceResponse<object>>> UpdateStatus(
        int id,
        [FromBody] UpdateStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var evento = await _db.EVENTOs
            .FirstOrDefaultAsync(e => e.id_evento == id && e.activo == true, cancellationToken);

        if (evento is null)
            return NotFound(ServiceResponse<object>.Fail("Evento no encontrado"));

        if (evento.fecha_cancelacion is not null)
            return BadRequest(ServiceResponse<object>.Fail("El evento ya está cancelado"));

        switch (request.status)
        {
            case "DRAFT":
                var tieneVendidos = await _db.EVENTO_ASIENTOs
                    .AnyAsync(ea => ea.id_evento == id && ea.estado == "VENDIDO", cancellationToken);

                if (tieneVendidos)
                    return BadRequest(ServiceResponse<object>.Fail(
                        "No se puede volver a DRAFT: el evento ya tiene tickets vendidos"));

                evento.publicado = false;
                break;

            case "PUBLISHED":
                var tieneZonas = await _db.EVENTO_ZONAs
                    .AnyAsync(ez => ez.id_evento == id && ez.activo == true, cancellationToken);

                if (!tieneZonas)
                    return BadRequest(ServiceResponse<object>.Fail(
                        "No se puede publicar: el evento no tiene zonas asignadas"));

                evento.publicado = true;
                break;

            case "CANCELLED":
                if (string.IsNullOrWhiteSpace(request.motivo_cancelacion))
                    return BadRequest(ServiceResponse<object>.Fail(
                        "Se requiere motivo_cancelacion para cancelar un evento"));

                evento.fecha_cancelacion  = DateTime.UtcNow;
                evento.motivo_cancelacion = request.motivo_cancelacion;
                evento.publicado          = false;

                var reservados = await _db.EVENTO_ASIENTOs
                    .Where(ea => ea.id_evento == id && ea.estado == "RESERVADO")
                    .ToListAsync(cancellationToken);

                foreach (var asiento in reservados)
                {
                    asiento.estado        = "DISPONIBLE";
                    asiento.fecha_reserva  = null;
                    asiento.reserva_expira = null;
                }
                break;
        }

        await _db.SaveChangesAsync(cancellationToken);

        await RegistrarAuditoriaAsync(
            id_staff: evento.creado_por_staff,
            accion: $"STATUS_{request.status}",
            tabla: "EVENTOS",
            id_registro: evento.id_evento,
            detalle: new { nuevo_status = request.status, request.motivo_cancelacion },
            cancellationToken);

        return Ok(ServiceResponse<object>.Ok($"Status actualizado a {request.status}"));
    }

    // PATCH /api/admin/eventos/{id}/pricing
    [HttpPatch("{id:int}/pricing")]
    public async Task<ActionResult<ServiceResponse<object>>> UpdatePricing(
        int id,
        [FromBody] UpdatePricingRequest request,
        CancellationToken cancellationToken = default)
    {
        var eventoExiste = await _db.EVENTOs
            .AnyAsync(e => e.id_evento == id && e.activo == true, cancellationToken);

        if (!eventoExiste)
            return NotFound(ServiceResponse<object>.Fail("Evento no encontrado"));

        var id_zonas = request.zonas.Select(z => z.id_zona).ToList();

        var eventoZonas = await _db.EVENTO_ZONAs
            .Where(ez => ez.id_evento == id && id_zonas.Contains(ez.id_zona) && ez.activo == true)
            .ToListAsync(cancellationToken);

        if (!eventoZonas.Any())
            return NotFound(ServiceResponse<object>.Fail(
                "No se encontraron zonas activas para este evento con los IDs proporcionados"));

        foreach (var zonaUpdate in request.zonas)
        {
            var eventoZona = eventoZonas.FirstOrDefault(ez => ez.id_zona == zonaUpdate.id_zona);
            if (eventoZona is null) continue;

            eventoZona.precio = zonaUpdate.nuevo_precio;
            if (zonaUpdate.nuevo_cargo_servicio is not null)
                eventoZona.cargo_servicio = zonaUpdate.nuevo_cargo_servicio;
        }

        await _db.SaveChangesAsync(cancellationToken);

        return Ok(ServiceResponse<object>.Ok("Precios actualizados correctamente"));
    }

    // ── Métodos privados ──────────────────────────────────────────

    private async Task<AdminEventoDetalleDto?> BuildDetalleDtoAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return await _db.EVENTOs
            .AsNoTracking()
            .Where(e => e.id_evento == id && e.activo == true)
            .Select(e => new AdminEventoDetalleDto(
                e.id_evento,
                e.nombre_evento,
                e.descripcion,
                e.fecha_evento,
                e.fecha_inicio_ventas,
                e.fecha_fin_ventas,
                e.fecha_creacion,
                e.capacidad_total,
                e.id_tipo_evento,
                e.id_tipo_eventoNavigation.nombre_tipo,
                e.fecha_cancelacion != null ? "CANCELLED" :
                e.publicado == true  ? "PUBLISHED" : "DRAFT",
                e.fecha_cancelacion,
                e.motivo_cancelacion,
                e.ruta_url,
                e.EVENTO_ZONAs
                    .Where(ez => ez.activo == true)
                    .Select(ez => new EventoZonaDto(
                        ez.id_evento_zona,
                        ez.id_zona,
                        ez.id_zonaNavigation.nombre_zona,
                        ez.id_zonaNavigation.color_hex,
                        ez.precio,
                        ez.cargo_servicio ?? 0,
                        ez.capacidad,
                        ez.activo ?? true))
                    .ToList(),
                e.EVENTO_ASIENTOs.Count(ea => ea.estado == "DISPONIBLE"),
                e.EVENTO_ASIENTOs.Count(ea => ea.estado == "RESERVADO"),
                e.EVENTO_ASIENTOs.Count(ea => ea.estado == "VENDIDO")))
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task RegistrarAuditoriaAsync(
        int id_staff,
        string accion,
        string tabla,
        int id_registro,
        object detalle,
        CancellationToken cancellationToken)
    {
        try
        {
            _db.AUDITORIAs.Add(new AUDITORIum
            {
                id_staff             = id_staff,
                accion               = accion,
                tabla_afectada       = tabla,
                id_registro_afectado = id_registro,
                detalle              = JsonSerializer.Serialize(detalle),
                fecha                = DateTime.UtcNow
            });

            await _db.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // No propagar errores de auditoría
        }
    }
}