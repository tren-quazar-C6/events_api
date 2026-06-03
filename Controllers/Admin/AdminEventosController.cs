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
        var query = _db.Eventos
            .AsNoTracking()
            .Where(e => e.Activo == true);

        if (id_tipo_evento is not null)
            query = query.Where(e => e.IdTipoEvento == id_tipo_evento);

        if (!string.IsNullOrWhiteSpace(busqueda))
            query = query.Where(e =>
                e.NombreEvento.Contains(busqueda) ||
                (e.Descripcion != null && e.Descripcion.Contains(busqueda)));

        query = status switch
        {
            "DRAFT"     => query.Where(e => e.Publicado == false && e.FechaCancelacion == null),
            "PUBLISHED" => query.Where(e => e.Publicado == true  && e.FechaCancelacion == null),
            "CANCELLED" => query.Where(e => e.FechaCancelacion != null),
            _           => query
        };

        var eventos = await query
            .OrderByDescending(e => e.FechaCreacion)
            .Select(e => new AdminEventoResumenDto(
                e.IdEvento,
                e.NombreEvento,
                e.FechaEvento,
                e.FechaInicioVentas,
                e.FechaFinVentas,
                e.CapacidadTotal,
                e.IdTipoEventoNavigation.NombreTipo,
                e.RutaUrl,
                e.FechaCancelacion != null ? "CANCELLED" : 
                e.Publicado == true ? "PUBLISHED" : "DRAFT",
                e.EventoZonas.Count(ez => ez.Activo == true)))
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
        var tipoExiste = await _db.TipoEventos
            .AnyAsync(t => t.IdTipoEvento == request.id_tipo_evento && t.Activo == true,
                cancellationToken);

        if (!tipoExiste)
            return BadRequest(ServiceResponse<AdminEventoDetalleDto>
                .Fail("El tipo de evento no existe o está inactivo"));

        var staffExiste = await _db.Staff
            .AnyAsync(s => s.IdStaff == request.creado_por_staff && s.Activo == true,
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

        var evento = new Evento
        {
            IdTipoEvento      = request.id_tipo_evento,
            CreadoPorStaff    = request.creado_por_staff,
            NombreEvento       = request.nombre_evento,
            Descripcion         = request.descripcion,
            FechaEvento        = request.fecha_evento,
            FechaInicioVentas = request.fecha_inicio_ventas,
            FechaFinVentas    = request.fecha_fin_ventas,
            CapacidadTotal     = request.capacidad_total,
            RutaUrl            = request.ruta_url,
            Publicado           = true,
            Activo              = true,
            FechaCreacion      = DateTime.UtcNow
        };

        _db.Eventos.Add(evento);

        if (request.zonas?.Any() == true)
        {
            var id_zonas = request.zonas.Select(z => z.id_zona).ToList();

            var zonasExistentes = await _db.Zonas
                .Where(z => id_zonas.Contains(z.IdZona) && z.Activo == true)
                .Select(z => z.IdZona)
                .ToListAsync(cancellationToken);

            var zonasNoEncontradas = id_zonas.Except(zonasExistentes).ToList();
            if (zonasNoEncontradas.Any())
                return BadRequest(ServiceResponse<AdminEventoDetalleDto>
                    .Fail($"Zonas no encontradas: {string.Join(", ", zonasNoEncontradas)}"));

            var asientosPorZona = await _db.Asientos
                .Where(a => id_zonas.Contains(a.IdZona) && a.Activo == true)
                .GroupBy(a => a.IdZona)
                .ToDictionaryAsync(g => g.Key, g => g.ToList(), cancellationToken);

            foreach (var zonaReq in request.zonas)
            {
                evento.EventoZonas.Add(new EventoZona
                {
                    IdZona        = zonaReq.id_zona,
                    Precio         = zonaReq.precio,
                    CargoServicio = zonaReq.cargo_servicio,
                    Capacidad      = zonaReq.capacidad,
                    Activo         = true
                });

                if (asientosPorZona.TryGetValue(zonaReq.id_zona, out var asientos))
                {
                    foreach (var asiento in asientos.Take(zonaReq.capacidad))
                    {
                        evento.EventoAsientos.Add(new EventoAsiento
                        {
                            IdAsiento = asiento.IdAsiento,
                            Estado     = "DISPONIBLE"
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
            id_registro: evento.IdEvento,
            detalle: new { evento.NombreEvento, evento.FechaEvento },
            cancellationToken);

        var dto = await BuildDetalleDtoAsync(evento.IdEvento, cancellationToken);
        return CreatedAtAction(
            nameof(GetEvento),
            new { id = evento.IdEvento },
            ServiceResponse<AdminEventoDetalleDto>.Ok(dto!));
    }

    // PUT /api/admin/eventos/{id}
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ServiceResponse<object>>> UpdateEvento(
        int id,
        [FromBody] UpdateEventoRequest request,
        CancellationToken cancellationToken = default)
    {
        var evento = await _db.Eventos
            .FirstOrDefaultAsync(e => e.IdEvento == id && e.Activo == true, cancellationToken);

        if (evento is null)
            return NotFound(ServiceResponse<object>.Fail("Evento no encontrado"));

        if (evento.FechaCancelacion is not null)
            return BadRequest(ServiceResponse<object>.Fail("No se puede editar un evento cancelado"));

        if (request.id_tipo_evento is not null)
        {
            var tipoExiste = await _db.TipoEventos
                .AnyAsync(t => t.IdTipoEvento == request.id_tipo_evento && t.Activo == true,
                    cancellationToken);

            if (!tipoExiste)
                return BadRequest(ServiceResponse<object>.Fail("El tipo de evento no existe"));

            evento.IdTipoEvento = request.id_tipo_evento.Value;
        }

        if (request.nombre_evento       is not null) evento.NombreEvento       = request.nombre_evento;
        if (request.descripcion         is not null) evento.Descripcion         = request.descripcion;
        if (request.fecha_evento        is not null) evento.FechaEvento        = request.fecha_evento.Value;
        if (request.fecha_inicio_ventas is not null) evento.FechaInicioVentas = request.fecha_inicio_ventas.Value;
        if (request.fecha_fin_ventas    is not null) evento.FechaFinVentas    = request.fecha_fin_ventas.Value;
        if (request.capacidad_total     is not null) evento.CapacidadTotal     = request.capacidad_total.Value;
        if (request.ruta_url            is not null) evento.RutaUrl            = request.ruta_url;

        await _db.SaveChangesAsync(cancellationToken);

        await RegistrarAuditoriaAsync(
            id_staff: evento.CreadoPorStaff,
            accion: "UPDATE",
            tabla: "EVENTOS",
            id_registro: evento.IdEvento,
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
        var evento = await _db.Eventos
            .FirstOrDefaultAsync(e => e.IdEvento == id && e.Activo == true, cancellationToken);

        if (evento is null)
            return NotFound(ServiceResponse<object>.Fail("Evento no encontrado"));

        var tieneVendidos = await _db.EventoAsientos
            .AnyAsync(ea => ea.IdEvento == id && ea.Estado == "VENDIDO", cancellationToken);

        if (tieneVendidos)
            return BadRequest(ServiceResponse<object>.Fail(
                "No se puede eliminar un evento con tickets vendidos. Use CANCELLED en su lugar."));

        evento.Activo = false;
        await _db.SaveChangesAsync(cancellationToken);

        await RegistrarAuditoriaAsync(
            id_staff: evento.CreadoPorStaff,
            accion: "DELETE",
            tabla: "EVENTOS",
            id_registro: evento.IdEvento,
            detalle: new { evento.NombreEvento },
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
        var evento = await _db.Eventos
            .FirstOrDefaultAsync(e => e.IdEvento == id && e.Activo == true, cancellationToken);

        if (evento is null)
            return NotFound(ServiceResponse<object>.Fail("Evento no encontrado"));

        if (evento.FechaCancelacion is not null)
            return BadRequest(ServiceResponse<object>.Fail("El evento ya está cancelado"));

        switch (request.status)
        {
            case "DRAFT":
                var tieneVendidos = await _db.EventoAsientos
                    .AnyAsync(ea => ea.IdEvento == id && ea.Estado == "VENDIDO", cancellationToken);

                if (tieneVendidos)
                    return BadRequest(ServiceResponse<object>.Fail(
                        "No se puede volver a DRAFT: el evento ya tiene tickets vendidos"));

                evento.Publicado = false;
                break;

            case "PUBLISHED":
                var tieneZonas = await _db.EventoZonas
                    .AnyAsync(ez => ez.IdEvento == id && ez.Activo == true, cancellationToken);

                if (!tieneZonas)
                    return BadRequest(ServiceResponse<object>.Fail(
                        "No se puede publicar: el evento no tiene zonas asignadas"));

                evento.Publicado = true;
                break;

            case "CANCELLED":
                if (string.IsNullOrWhiteSpace(request.motivo_cancelacion))
                    return BadRequest(ServiceResponse<object>.Fail(
                        "Se requiere motivo_cancelacion para cancelar un evento"));

                evento.FechaCancelacion  = DateTime.UtcNow;
                evento.MotivoCancelacion = request.motivo_cancelacion;
                evento.Publicado          = false;

                var reservados = await _db.EventoAsientos
                    .Where(ea => ea.IdEvento == id && ea.Estado == "RESERVADO")
                    .ToListAsync(cancellationToken);

                foreach (var asiento in reservados)
                {
                    asiento.Estado        = "DISPONIBLE";
                    asiento.FechaReserva  = null;
                    asiento.ReservaExpira = null;
                }
                break;
        }

        await _db.SaveChangesAsync(cancellationToken);

        await RegistrarAuditoriaAsync(
            id_staff: evento.CreadoPorStaff,
            accion: $"STATUS_{request.status}",
            tabla: "EVENTOS",
            id_registro: evento.IdEvento,
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
        var eventoExiste = await _db.Eventos
            .AnyAsync(e => e.IdEvento == id && e.Activo == true, cancellationToken);

        if (!eventoExiste)
            return NotFound(ServiceResponse<object>.Fail("Evento no encontrado"));

        var id_zonas = request.zonas.Select(z => z.id_zona).ToList();

        var eventoZonas = await _db.EventoZonas
            .Where(ez => ez.IdEvento == id && id_zonas.Contains(ez.IdZona) && ez.Activo == true)
            .ToListAsync(cancellationToken);

        if (!eventoZonas.Any())
            return NotFound(ServiceResponse<object>.Fail(
                "No se encontraron zonas activas para este evento con los IDs proporcionados"));

        foreach (var zonaUpdate in request.zonas)
        {
            var eventoZona = eventoZonas.FirstOrDefault(ez => ez.IdZona == zonaUpdate.id_zona);
            if (eventoZona is null) continue;

            eventoZona.Precio = zonaUpdate.nuevo_precio;
            if (zonaUpdate.nuevo_cargo_servicio is not null)
                eventoZona.CargoServicio = zonaUpdate.nuevo_cargo_servicio;
        }

        await _db.SaveChangesAsync(cancellationToken);

        return Ok(ServiceResponse<object>.Ok("Precios actualizados correctamente"));
    }

    // PUT /api/admin/eventos/{id}/zonas
    [HttpPut("{id:int}/zonas")]
    public async Task<ActionResult<ServiceResponse<AdminEventoDetalleDto>>> UpdateEventZones(
        int id,
        [FromBody] UpdateEventoZonasRequest request,
        CancellationToken cancellationToken = default)
    {
        var evento = await _db.Eventos
            .Include(e => e.EventoZonas)
            .Include(e => e.EventoAsientos)
            .FirstOrDefaultAsync(e => e.IdEvento == id && e.Activo == true, cancellationToken);

        if (evento is null)
            return NotFound(ServiceResponse<AdminEventoDetalleDto>.Fail("Evento no encontrado"));

        if (evento.FechaCancelacion is not null)
            return BadRequest(ServiceResponse<AdminEventoDetalleDto>.Fail("No se pueden modificar las zonas de un evento cancelado"));

        if (evento.EventoAsientos.Any())
            return BadRequest(ServiceResponse<AdminEventoDetalleDto>.Fail(
                "No se pueden modificar las zonas cuando el evento ya tiene asientos generados"));

        var zonasDuplicadas = request.zonas
            .GroupBy(z => z.id_zona)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (zonasDuplicadas.Any())
        {
            return BadRequest(ServiceResponse<AdminEventoDetalleDto>.Fail(
                $"No se permiten zonas duplicadas: {string.Join(", ", zonasDuplicadas)}"));
        }

        var capacidadSolicitada = request.zonas.Sum(z => z.capacidad);
        if (capacidadSolicitada > evento.CapacidadTotal)
        {
            return BadRequest(ServiceResponse<AdminEventoDetalleDto>.Fail(
                $"La suma de capacidades por zona ({capacidadSolicitada}) excede la capacidad total del evento ({evento.CapacidadTotal})"));
        }

        var idZonas = request.zonas.Select(z => z.id_zona).ToList();
        var zonasActivas = await _db.Zonas
            .Where(z => idZonas.Contains(z.IdZona) && z.Activo == true)
            .Select(z => z.IdZona)
            .ToListAsync(cancellationToken);

        var zonasNoEncontradas = idZonas.Except(zonasActivas).ToList();
        if (zonasNoEncontradas.Any())
        {
            return BadRequest(ServiceResponse<AdminEventoDetalleDto>.Fail(
                $"Zonas no encontradas o inactivas: {string.Join(", ", zonasNoEncontradas)}"));
        }

        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            _db.EventoZonas.RemoveRange(evento.EventoZonas);

            foreach (var zonaReq in request.zonas)
            {
                evento.EventoZonas.Add(new EventoZona
                {
                    IdEvento = evento.IdEvento,
                    IdZona = zonaReq.id_zona,
                    Precio = zonaReq.precio,
                    CargoServicio = zonaReq.cargo_servicio,
                    Capacidad = zonaReq.capacidad,
                    Activo = true
                });
            }

            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        await RegistrarAuditoriaAsync(
            id_staff: evento.CreadoPorStaff,
            accion: "UPDATE_ZONAS",
            tabla: "EVENTOS",
            id_registro: evento.IdEvento,
            detalle: new
            {
                capacidad_total = evento.CapacidadTotal,
                capacidad_asignada = capacidadSolicitada,
                zonas = request.zonas
            },
            cancellationToken);

        var dto = await BuildDetalleDtoAsync(evento.IdEvento, cancellationToken);
        return Ok(ServiceResponse<AdminEventoDetalleDto>.Ok(dto!, "Zonas actualizadas correctamente"));
    }

    // ── Métodos privados ──────────────────────────────────────────

    private async Task<AdminEventoDetalleDto?> BuildDetalleDtoAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return await _db.Eventos
            .AsNoTracking()
            .Where(e => e.IdEvento == id && e.Activo == true)
            .Select(e => new AdminEventoDetalleDto(
                e.IdEvento,
                e.NombreEvento,
                e.Descripcion,
                e.FechaEvento,
                e.FechaInicioVentas,
                e.FechaFinVentas,
                e.FechaCreacion,
                e.CapacidadTotal,
                e.IdTipoEvento,
                e.IdTipoEventoNavigation.NombreTipo,
                e.FechaCancelacion != null ? "CANCELLED" :
                e.Publicado == true  ? "PUBLISHED" : "DRAFT",
                e.FechaCancelacion,
                e.MotivoCancelacion,
                e.RutaUrl,
                e.EventoZonas
                    .Where(ez => ez.Activo == true)
                    .Select(ez => new EventoZonaDto(
                        ez.IdEventoZona,
                        ez.IdZona,
                        ez.IdZonaNavigation.NombreZona,
                        ez.IdZonaNavigation.ColorHex,
                        ez.Precio,
                        ez.CargoServicio ?? 0,
                        ez.Capacidad,
                        ez.Activo ?? true))
                    .ToList(),
                e.EventoAsientos.Count(ea => ea.Estado == "DISPONIBLE"),
                e.EventoAsientos.Count(ea => ea.Estado == "RESERVADO"),
                e.EventoAsientos.Count(ea => ea.Estado == "VENDIDO")))
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
            _db.Auditoria.Add(new Auditorium
            {
                IdStaff             = id_staff,
                Accion               = accion,
                TablaAfectada       = tabla,
                IdRegistroAfectado = id_registro,
                Detalle              = JsonSerializer.Serialize(detalle),
                Fecha                = DateTime.UtcNow
            });

            await _db.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // No propagar errores de auditoría
        }
    }
}
