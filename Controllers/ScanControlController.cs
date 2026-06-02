using events_api.Data;
using events_api.DTOs;
using events_api.Entities;
using events_api.Responses;
using events_api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace events_api.Controllers;

[ApiController]
[Route("api")]
public class ScanControlController : ControllerBase
{
    private readonly QuasarDbContext _db;
    private readonly ScanValidationService _scanValidationService;

    public ScanControlController(
        QuasarDbContext db,
        ScanValidationService scanValidationService)
    {
        _db = db;
        _scanValidationService = scanValidationService;
    }

    [HttpPost("tickets/scan")]
    public async Task<ActionResult<ServiceResponse<ScanTicketResponse>>> ScanTicket(
        [FromBody] ScanTicketRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _scanValidationService.ValidateAndRegisterScanAsync(request, cancellationToken);
        return Ok(ServiceResponse<ScanTicketResponse>.Ok(result));
    }

    [HttpGet("events/today")]
    public async Task<ActionResult<ServiceResponse<IReadOnlyCollection<TodayEventDto>>>> GetTodayEvents(
        CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var eventsToday = await _db.Eventos
            .AsNoTracking()
            .Where(e => e.FechaEvento >= today && e.FechaEvento < tomorrow)
            .Where(e => e.Activo == true && e.Publicado == true)
            .OrderBy(e => e.FechaEvento)
            .Select(e => new TodayEventDto(
                e.IdEvento,
                e.NombreEvento,
                e.FechaEvento,
                e.Publicado ?? false,
                e.Activo ?? false))
            .ToListAsync(cancellationToken);

        return Ok(ServiceResponse<IReadOnlyCollection<TodayEventDto>>.Ok(eventsToday));
    }

    [HttpGet("tickets/{id:int}")]
    public async Task<ActionResult<ServiceResponse<TicketLookupDto>>> GetTicketById(
        int id,
        CancellationToken cancellationToken = default)
    {
        var ticket = await _db.Tickets
            .AsNoTracking()
            .Where(t => t.IdTicket == id)
            .Select(t => new TicketLookupDto(
                t.IdTicket,
                t.CodigoUnico,
                t.QrToken,
                t.IdEstadoTicketNavigation.NombreEstado,
                t.IdEventoAsientoNavigation.IdEvento,
                t.IdEventoAsientoNavigation.IdEventoNavigation.NombreEvento,
                t.IdEventoAsientoNavigation.IdEventoNavigation.FechaEvento,
                t.IdEventoAsientoNavigation.IdEventoNavigation.FechaInicioVentas,
                t.IdEventoAsientoNavigation.IdEventoNavigation.FechaFinVentas,
                t.PrecioPagado))
            .FirstOrDefaultAsync(cancellationToken);

        return ticket is null
            ? NotFound(ServiceResponse<TicketLookupDto>.Fail("Ticket no encontrado"))
            : Ok(ServiceResponse<TicketLookupDto>.Ok(ticket));
    }

    [HttpPost("scan-alerts")]
    public async Task<ActionResult<ServiceResponse<ScanAlertDto>>> CreateScanAlert(
        [FromBody] CreateScanAlertRequest request,
        CancellationToken cancellationToken = default)
    {
        var alert = new ScanAlert
        {
            IdScan = request.id_scan,
            IdTicket = request.id_ticket,
            IdStaff = request.id_staff,
            TipoAlerta = request.tipo_alerta,
            Detalle = request.detalle,
            QrToken = request.qr_token,
            Dispositivo = request.dispositivo,
            PayloadJson = request.payload_json,
            FechaAlerta = DateTime.UtcNow
        };

        _db.ScanAlerts.Add(alert);
        await _db.SaveChangesAsync(cancellationToken);

        var dto = new ScanAlertDto(
            alert.IdScanAlert,
            alert.TipoAlerta,
            alert.FechaAlerta ?? DateTime.UtcNow,
            alert.IdScan,
            alert.IdTicket,
            alert.IdStaff,
            alert.Detalle,
            alert.QrToken,
            alert.Dispositivo);

        return Ok(ServiceResponse<ScanAlertDto>.Ok(dto));
    }
}
