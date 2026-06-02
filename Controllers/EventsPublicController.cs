using events_api.Data;
using events_api.Responses;
using events_api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace events_api.Controllers;

[ApiController]
[Route("api/events")]
[Authorize]
public class EventsPublicController : ControllerBase
{
    private readonly QuasarDbContext _db;

    public EventsPublicController(QuasarDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    [RequirePermission("events.read")]
    public async Task<ActionResult<ServiceResponse<object>>> GetEvents(
        CancellationToken cancellationToken = default)
    {
        var events = await _db.EVENTOs
            .AsNoTracking()
            .Where(e => e.activo == true && e.publicado == true)
            .OrderBy(e => e.fecha_evento)
            .Select(e => new
            {
                e.id_evento,
                e.nombre_evento,
                e.descripcion,
                e.fecha_evento,
                e.fecha_inicio_ventas,
                e.fecha_fin_ventas,
                e.capacidad_total
            })
            .ToListAsync(cancellationToken);

        return Ok(ServiceResponse<object>.Ok(events));
    }
}
