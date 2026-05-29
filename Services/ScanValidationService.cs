using events_api.Data;
using events_api.DTOs;
using events_api.Entities;
using Microsoft.EntityFrameworkCore;

namespace events_api.Services;

public class ScanValidationService
{
    private static readonly string[] InactiveTicketStates = ["CANCELADO", "ANULADO", "INACTIVO"];

    private readonly QuasarDbContext _db;

    public ScanValidationService(QuasarDbContext db)
    {
        _db = db;
    }

    public async Task<ScanTicketResponse> ValidateAndRegisterScanAsync(
        ScanTicketRequest request,
        CancellationToken cancellationToken = default)
    {
        var scanTime = request.fecha_scan ?? DateTime.UtcNow;

        var staffExists = await _db.STAFF
            .AsNoTracking()
            .AnyAsync(staff => staff.id_staff == request.id_empleado && staff.activo == true, cancellationToken);

        if (!staffExists)
        {
            return new ScanTicketResponse("INVALIDO", "Empleado no existe o está inactivo.", null, null, "STAFF_INVALIDO");
        }

        var ticket = await _db.TICKETs
            .Include(t => t.id_estado_ticketNavigation)
            .Include(t => t.id_evento_asientoNavigation)
                .ThenInclude(ea => ea.id_eventoNavigation)
            .FirstOrDefaultAsync(t => t.qr_token == request.qr_token, cancellationToken);

        if (ticket is null)
        {
            await CreateAlertAsync(
                tipoAlerta: "QR_INEXISTENTE",
                detalle: "Intento de scan con QR no registrado.",
                qrToken: request.qr_token,
                idStaff: request.id_empleado,
                dispositivo: request.dispositivo,
                cancellationToken: cancellationToken);

            return new ScanTicketResponse("INVALIDO", "QR inexistente.", null, null, "QR_INEXISTENTE");
        }

        if (InactiveTicketStates.Contains(ticket.id_estado_ticketNavigation.nombre_estado.ToUpperInvariant()))
        {
            var scanId = await SaveScanAsync(ticket.id_ticket, request.id_empleado, "INVALIDO", "Ticket cancelado o inactivo.", request.dispositivo, scanTime, cancellationToken);
            await CreateAlertAsync("TICKET_CANCELADO", "Scan de ticket cancelado/inactivo.", request.qr_token, request.id_empleado, request.dispositivo, ticket.id_ticket, scanId, cancellationToken);
            return new ScanTicketResponse("INVALIDO", "Ticket cancelado o inactivo.", ticket.id_ticket, scanId, "TICKET_CANCELADO");
        }

        if (request.id_evento is not null && ticket.id_evento_asientoNavigation.id_evento != request.id_evento)
        {
            var scanId = await SaveScanAsync(ticket.id_ticket, request.id_empleado, "INVALIDO", "Ticket para evento incorrecto.", request.dispositivo, scanTime, cancellationToken);
            await CreateAlertAsync("EVENTO_INCORRECTO", "Ticket usado en evento incorrecto.", request.qr_token, request.id_empleado, request.dispositivo, ticket.id_ticket, scanId, cancellationToken);
            return new ScanTicketResponse("INVALIDO", "Ticket no corresponde al evento.", ticket.id_ticket, scanId, "EVENTO_INCORRECTO");
        }

        var evento = ticket.id_evento_asientoNavigation.id_eventoNavigation;
        var outOfSchedule = scanTime.Date != evento.fecha_evento.Date
            || scanTime < evento.fecha_inicio_ventas
            || scanTime > evento.fecha_evento.AddHours(6);

        if (outOfSchedule)
        {
            var scanId = await SaveScanAsync(ticket.id_ticket, request.id_empleado, "INVALIDO", "Ticket fuera de horario permitido.", request.dispositivo, scanTime, cancellationToken);
            await CreateAlertAsync("FUERA_DE_HORARIO", "Scan fuera de fecha/hora válida del evento.", request.qr_token, request.id_empleado, request.dispositivo, ticket.id_ticket, scanId, cancellationToken);
            return new ScanTicketResponse("INVALIDO", "Ticket fuera de fecha/hora válida.", ticket.id_ticket, scanId, "FUERA_DE_HORARIO");
        }

        var alreadyUsed = await _db.SCANs
            .AsNoTracking()
            .AnyAsync(scan => scan.id_ticket == ticket.id_ticket && scan.resultado == "VALIDO", cancellationToken);

        if (alreadyUsed)
        {
            var scanId = await SaveScanAsync(ticket.id_ticket, request.id_empleado, "DUPLICADO", "Intento de reutilización de ticket.", request.dispositivo, scanTime, cancellationToken);
            await CreateAlertAsync("QR_DUPLICADO", "Ticket ya escaneado previamente.", request.qr_token, request.id_empleado, request.dispositivo, ticket.id_ticket, scanId, cancellationToken);
            return new ScanTicketResponse("DUPLICADO", "Ticket ya fue usado.", ticket.id_ticket, scanId, "QR_DUPLICADO");
        }

        var validScanId = await SaveScanAsync(ticket.id_ticket, request.id_empleado, "VALIDO", "Acceso autorizado.", request.dispositivo, scanTime, cancellationToken);
        return new ScanTicketResponse("VALIDO", "Ticket válido.", ticket.id_ticket, validScanId, null);
    }

    private async Task<int> SaveScanAsync(
        int idTicket,
        int idStaff,
        string resultado,
        string observacion,
        string? dispositivo,
        DateTime fechaScan,
        CancellationToken cancellationToken)
    {
        var scan = new SCAN
        {
            id_ticket = idTicket,
            id_staff = idStaff,
            resultado = resultado,
            observacion = observacion,
            dispositivo = dispositivo,
            fecha_scan = fechaScan
        };

        _db.SCANs.Add(scan);
        await _db.SaveChangesAsync(cancellationToken);
        return scan.id_scan;
    }

    private async Task CreateAlertAsync(
        string tipoAlerta,
        string detalle,
        string? qrToken,
        int? idStaff,
        string? dispositivo,
        int? idTicket = null,
        int? idScan = null,
        CancellationToken cancellationToken = default)
    {
        var alert = new SCAN_ALERT
        {
            tipo_alerta = tipoAlerta,
            detalle = detalle,
            qr_token = qrToken,
            id_staff = idStaff,
            id_ticket = idTicket,
            id_scan = idScan,
            dispositivo = dispositivo,
            fecha_alerta = DateTime.UtcNow
        };

        _db.SCAN_ALERTs.Add(alert);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
