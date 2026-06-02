using events_api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace events_api.Controllers;

[ApiController]
[Route("api/events")]
public class EventsPublicController : ControllerBase
{
    private readonly QuasarDbContext _db;

    public EventsPublicController(QuasarDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<object>> GetEvents(
        CancellationToken cancellationToken = default)
    {
        var events = await _db.Eventos
            .AsNoTracking()
            .Where(e => e.Activo == true && e.Publicado == true)
            .OrderBy(e => e.FechaEvento)
            .Select(e => new
            {
                id_evento = e.IdEvento,
                nombre_evento = e.NombreEvento,
                descripcion = e.Descripcion,
                fecha_evento = e.FechaEvento,
                fecha_inicio_ventas = e.FechaInicioVentas,
                fecha_fin_ventas = e.FechaFinVentas,
                capacidad_total = e.CapacidadTotal
            })
            .ToListAsync(cancellationToken);

        return Ok(events);
    }
}
