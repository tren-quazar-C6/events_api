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
        var query = _db.EVENTOs
            .AsNoTracking()
            .Where(evento => evento.activo == true);

        if (tipoEventoId is not null)
        {
            query = query.Where(evento => evento.id_tipo_evento == tipoEventoId);
        }

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            query = query.Where(evento =>
                evento.nombre_evento.Contains(busqueda)
                || (evento.descripcion != null && evento.descripcion.Contains(busqueda)));
        }

        if (soloProximos)
        {
            query = query.Where(evento => evento.fecha_evento >= DateTime.Now);
        }

        var eventos = await query
            .OrderBy(evento => evento.fecha_evento)
            .Select(evento => new EventoResumenDto(
                evento.id_evento,
                evento.nombre_evento,
                evento.descripcion,
                evento.fecha_evento,
                evento.fecha_inicio_ventas,
                evento.fecha_fin_ventas,
                evento.capacidad_total,
                evento.id_tipo_evento,
                evento.id_tipo_eventoNavigation.nombre_tipo,
                evento.IMAGENEs
                    .Where(imagen => imagen.principal == true)
                    .Select(imagen => imagen.ruta_url)
                    .FirstOrDefault()
                    ?? evento.IMAGENEs
                        .Select(imagen => imagen.ruta_url)
                        .FirstOrDefault(),
                evento.EVENTO_ASIENTOs.Count(ea => ea.estado == "DISPONIBLE"),
                evento.EVENTO_ZONAs
                    .Where(ez => ez.activo == true)
                    .Select(ez => (decimal?)ez.precio)
                    .Min()))
            .ToListAsync(cancellationToken);

        return Ok(eventos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EventoDetalleDto>> GetEvento(
        int id,
        CancellationToken cancellationToken = default)
    {
        var evento = await _db.EVENTOs
            .AsNoTracking()
            .Where(evento => evento.id_evento == id && evento.activo == true)
            .Select(evento => new EventoDetalleDto(
                evento.id_evento,
                evento.nombre_evento,
                evento.descripcion,
                evento.fecha_evento,
                evento.fecha_inicio_ventas,
                evento.fecha_fin_ventas,
                evento.fecha_creacion,
                evento.capacidad_total,
                evento.id_tipo_evento,
                evento.id_tipo_eventoNavigation.nombre_tipo,
                evento.IMAGENEs
                    .OrderByDescending(imagen => imagen.principal == true)
                    .Select(imagen => new ImagenEventoDto(
                        imagen.id_imagen,
                        imagen.ruta_url,
                        imagen.principal == true))
                    .ToList(),
                evento.EVENTO_ASIENTOs.Count(ea => ea.estado == "DISPONIBLE"),
                evento.EVENTO_ASIENTOs.Count(ea => ea.estado == "RESERVADO"),
                evento.EVENTO_ASIENTOs.Count(ea => ea.estado == "VENDIDO"),
                evento.EVENTO_ZONAs
                    .Where(ez => ez.activo == true)
                    .Select(ez => (decimal?)ez.precio)
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
        var eventoExiste = await _db.EVENTOs
            .AsNoTracking()
            .AnyAsync(evento => evento.id_evento == id && evento.activo == true, cancellationToken);

        if (!eventoExiste)
        {
            return NotFound();
        }

        var query = _db.EVENTO_ASIENTOs
            .AsNoTracking()
            .Where(ea => ea.id_evento == id);

        if (soloDisponibles)
        {
            query = query.Where(ea => ea.estado == "DISPONIBLE");
        }

        var asientos = await query
            .OrderBy(ea => ea.id_asientoNavigation.id_zonaNavigation.nombre_zona)
            .ThenBy(ea => ea.id_asientoNavigation.fila)
            .ThenBy(ea => ea.id_asientoNavigation.numero)
            .Select(ea => new EventoAsientoDto(
                ea.id_evento_asiento,
                ea.id_asiento,
                ea.id_asientoNavigation.fila + ea.id_asientoNavigation.numero.ToString(),
                ea.id_asientoNavigation.fila,
                ea.id_asientoNavigation.numero,
                ea.id_asientoNavigation.id_zona,
                ea.id_asientoNavigation.id_zonaNavigation.nombre_zona,
                ea.id_asientoNavigation.id_zonaNavigation.color_hex,
                _db.EVENTO_ZONAs
                    .Where(ez => ez.id_evento == id && ez.id_zona == ea.id_asientoNavigation.id_zona)
                    .Select(ez => ez.precio)
                    .FirstOrDefault(),
                ea.estado))
            .ToListAsync(cancellationToken);

        return Ok(asientos);
    }

    [HttpGet("/api/tipos-evento")]
    public async Task<ActionResult<IReadOnlyCollection<TipoEventoDto>>> GetTiposEvento(
        CancellationToken cancellationToken = default)
    {
        var tiposEvento = await _db.TIPO_EVENTOs
            .AsNoTracking()
            .Where(tipoEvento => tipoEvento.activo == true)
            .OrderBy(tipoEvento => tipoEvento.nombre_tipo)
            .Select(tipoEvento => new TipoEventoDto(
                tipoEvento.id_tipo_evento,
                tipoEvento.nombre_tipo))
            .ToListAsync(cancellationToken);

        return Ok(tiposEvento);
    }
}