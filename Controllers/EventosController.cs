using events_api.Data;
using events_api.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace events_api.Controllers;

[ApiController]
[Route("api/eventos")]
public class EventosController : ControllerBase
{
    private readonly QuasarDbContext _db;

    public EventosController(QuasarDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<EventoResumenDto>>> GetEventos(
        [FromQuery] int? tipoEventoId,
        [FromQuery] string? busqueda,
        [FromQuery] bool soloProximos = false,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Eventos
            .AsNoTracking()
            .Where(evento => evento.Activo == true);

        if (tipoEventoId is not null)
        {
            query = query.Where(evento => evento.IdTipoEvento == tipoEventoId);
        }

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            query = query.Where(evento =>
                evento.NombreEvento.Contains(busqueda)
                || (evento.Descripcion != null && evento.Descripcion.Contains(busqueda)));
        }

        if (soloProximos)
        {
            query = query.Where(evento => evento.FechaEvento >= DateTime.Now);
        }

        var eventos = await query
            .OrderBy(evento => evento.FechaEvento)
            .Select(evento => new EventoResumenDto(
                evento.IdEvento,
                evento.NombreEvento,
                evento.Descripcion,
                evento.FechaEvento,
                evento.FechaInicioVentas,
                evento.FechaFinVentas,
                evento.CapacidadTotal,
                evento.IdTipoEvento,
                evento.IdTipoEventoNavigation.NombreTipo,
                evento.Imagenes
                    .Where(imagen => imagen.Principal == true)
                    .Select(imagen => imagen.RutaUrl)
                    .FirstOrDefault()
                    ?? evento.Imagenes
                        .Select(imagen => imagen.RutaUrl)
                        .FirstOrDefault(),
                evento.EventoAsientos.Count(asiento => asiento.Estado == "DISPONIBLE"),
                evento.EventoAsientos
                    .Where(asiento => asiento.Estado == "DISPONIBLE")
                    .Select(asiento => (decimal?)asiento.Precio)
                    .Min()))
            .ToListAsync(cancellationToken);

        return Ok(eventos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EventoDetalleDto>> GetEvento(
        int id,
        CancellationToken cancellationToken = default)
    {
        var evento = await _db.Eventos
            .AsNoTracking()
            .Where(evento => evento.IdEvento == id && evento.Activo == true)
            .Select(evento => new EventoDetalleDto(
                evento.IdEvento,
                evento.NombreEvento,
                evento.Descripcion,
                evento.FechaEvento,
                evento.FechaInicioVentas,
                evento.FechaFinVentas,
                evento.FechaCreacion,
                evento.CapacidadTotal,
                evento.IdTipoEvento,
                evento.IdTipoEventoNavigation.NombreTipo,
                evento.Imagenes
                    .OrderByDescending(imagen => imagen.Principal == true)
                    .Select(imagen => new ImagenEventoDto(
                        imagen.IdImagen,
                        imagen.RutaUrl,
                        imagen.Principal == true))
                    .ToList(),
                evento.EventoAsientos.Count(asiento => asiento.Estado == "DISPONIBLE"),
                evento.EventoAsientos.Count(asiento => asiento.Estado == "RESERVADO"),
                evento.EventoAsientos.Count(asiento => asiento.Estado == "VENDIDO"),
                evento.EventoAsientos
                    .Where(asiento => asiento.Estado == "DISPONIBLE")
                    .Select(asiento => (decimal?)asiento.Precio)
                    .Min()))
            .FirstOrDefaultAsync(cancellationToken);

        return evento is null ? NotFound() : Ok(evento);
    }

    [HttpGet("{id:int}/asientos")]
    public async Task<ActionResult<IReadOnlyCollection<EventoAsientoDto>>> GetAsientosEvento(
        int id,
        [FromQuery] bool soloDisponibles = false,
        CancellationToken cancellationToken = default)
    {
        var eventoExiste = await _db.Eventos
            .AsNoTracking()
            .AnyAsync(evento => evento.IdEvento == id && evento.Activo == true, cancellationToken);

        if (!eventoExiste)
        {
            return NotFound();
        }

        var query = _db.EventoAsientos
            .AsNoTracking()
            .Where(eventoAsiento => eventoAsiento.IdEvento == id);

        if (soloDisponibles)
        {
            query = query.Where(eventoAsiento => eventoAsiento.Estado == "DISPONIBLE");
        }

        var asientos = await query
            .OrderBy(eventoAsiento => eventoAsiento.Precio)
            .ThenBy(eventoAsiento => eventoAsiento.IdAsientoNavigation.IdZonaNavigation.NombreZona)
            .ThenBy(eventoAsiento => eventoAsiento.IdAsientoNavigation.Fila)
            .ThenBy(eventoAsiento => eventoAsiento.IdAsientoNavigation.Numero)
            .Select(eventoAsiento => new EventoAsientoDto(
                eventoAsiento.IdEventoAsiento,
                eventoAsiento.IdAsiento,
                eventoAsiento.IdAsientoNavigation.CodigoAsiento,
                eventoAsiento.IdAsientoNavigation.Fila,
                eventoAsiento.IdAsientoNavigation.Numero,
                eventoAsiento.IdAsientoNavigation.IdZona,
                eventoAsiento.IdAsientoNavigation.IdZonaNavigation.NombreZona,
                eventoAsiento.IdAsientoNavigation.IdZonaNavigation.ColorHex,
                eventoAsiento.Precio,
                eventoAsiento.Estado))
            .ToListAsync(cancellationToken);

        return Ok(asientos);
    }

    [HttpGet("/api/tipos-evento")]
    public async Task<ActionResult<IReadOnlyCollection<TipoEventoDto>>> GetTiposEvento(
        CancellationToken cancellationToken = default)
    {
        var tiposEvento = await _db.TipoEventos
            .AsNoTracking()
            .Where(tipoEvento => tipoEvento.Activo == true)
            .OrderBy(tipoEvento => tipoEvento.NombreTipo)
            .Select(tipoEvento => new TipoEventoDto(
                tipoEvento.IdTipoEvento,
                tipoEvento.NombreTipo))
            .ToListAsync(cancellationToken);

        return Ok(tiposEvento);
    }
}
