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

        var eventsToday = await _db.EVENTOs
            .AsNoTracking()
            .Where(e => e.fecha_evento >= today && e.fecha_evento < tomorrow)
            .Where(e => e.activo == true && e.publicado == true)
            .OrderBy(e => e.fecha_evento)
            .Select(e => new TodayEventDto(
                e.id_evento,
                e.nombre_evento,
                e.fecha_evento,
                e.publicado ?? false,
                e.activo ?? false))
            .ToListAsync(cancellationToken);

        return Ok(ServiceResponse<IReadOnlyCollection<TodayEventDto>>.Ok(eventsToday));
    }

    [HttpGet("tickets/{id:int}")]
    public async Task<ActionResult<ServiceResponse<TicketLookupDto>>> GetTicketById(
        int id,
        CancellationToken cancellationToken = default)
    {
        var ticket = await _db.TICKETs
            .AsNoTracking()
            .Where(t => t.id_ticket == id)
            .Select(t => new TicketLookupDto(
                t.id_ticket,
                t.codigo_unico,
                t.qr_token,
                t.id_estado_ticketNavigation.nombre_estado,
                t.id_evento_asientoNavigation.id_evento,
                t.id_evento_asientoNavigation.id_eventoNavigation.nombre_evento,
                t.id_evento_asientoNavigation.id_eventoNavigation.fecha_evento,
                t.id_evento_asientoNavigation.id_eventoNavigation.fecha_inicio_ventas,
                t.id_evento_asientoNavigation.id_eventoNavigation.fecha_fin_ventas,
                t.precio_pagado))
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
        var alert = new SCAN_ALERT
        {
            id_scan = request.id_scan,
            id_ticket = request.id_ticket,
            id_staff = request.id_staff,
            tipo_alerta = request.tipo_alerta,
            detalle = request.detalle,
            qr_token = request.qr_token,
            dispositivo = request.dispositivo,
            payload_json = request.payload_json,
            fecha_alerta = DateTime.UtcNow
        };

        _db.SCAN_ALERTs.Add(alert);
        await _db.SaveChangesAsync(cancellationToken);

        var dto = new ScanAlertDto(
            alert.id_scan_alert,
            alert.tipo_alerta,
            alert.fecha_alerta ?? DateTime.UtcNow,
            alert.id_scan,
            alert.id_ticket,
            alert.id_staff,
            alert.detalle,
            alert.qr_token,
            alert.dispositivo);

        return Ok(ServiceResponse<ScanAlertDto>.Ok(dto));
    }
}
