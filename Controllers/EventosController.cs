using events_api.Data;
using events_api.DTOs;
using events_api.Entities;
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
                evento.RutaUrl,
                evento.EventoAsientos.Count(ea => ea.Estado == "DISPONIBLE"),
                evento.EventoZonas
                    .Where(ez => ez.Activo == true)
                    .Select(ez => (decimal?)ez.Precio)
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
                evento.RutaUrl,
                evento.EventoAsientos.Count(ea => ea.Estado == "DISPONIBLE"),
                evento.EventoAsientos.Count(ea => ea.Estado == "RESERVADO"),
                evento.EventoAsientos.Count(ea => ea.Estado == "VENDIDO"),
                evento.EventoZonas
                    .Where(ez => ez.Activo == true)
                    .Select(ez => (decimal?)ez.Precio)
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
            .Where(ea => ea.IdEvento == id);

        if (soloDisponibles)
        {
            query = query.Where(ea => ea.Estado == "DISPONIBLE");
        }

        var asientos = await query
            .OrderBy(ea => ea.IdAsientoNavigation.IdZonaNavigation.NombreZona)
            .ThenBy(ea => ea.IdAsientoNavigation.Fila)
            .ThenBy(ea => ea.IdAsientoNavigation.Numero)
            .Select(ea => new EventoAsientoDto(
                ea.IdEventoAsiento,
                ea.IdAsiento,
                ea.IdAsientoNavigation.Fila + ea.IdAsientoNavigation.Numero.ToString(),
                ea.IdAsientoNavigation.Fila,
                ea.IdAsientoNavigation.Numero,
                ea.IdAsientoNavigation.IdZona,
                ea.IdAsientoNavigation.IdZonaNavigation.NombreZona,
                ea.IdAsientoNavigation.IdZonaNavigation.ColorHex,
                _db.EventoZonas
                    .Where(ez => ez.IdEvento == id && ez.IdZona == ea.IdAsientoNavigation.IdZona)
                    .Select(ez => ez.Precio)
                    .FirstOrDefault(),
                ea.Estado))
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