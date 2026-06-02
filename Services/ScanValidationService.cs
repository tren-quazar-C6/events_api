using events_api.Data;
using events_api.DTOs;
using events_api.Entities;
using Microsoft.EntityFrameworkCore;

namespace events_api.Services;

public class ScanValidationService
{
    private static readonly string[] AllowedTicketStates = ["ACTIVO", "PAGADO"];

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

        var staffExists = await _db.Staff
            .AsNoTracking()
            .AnyAsync(staff => staff.IdStaff == request.id_empleado && staff.Activo == true, cancellationToken);

        if (!staffExists)
        {
            return new ScanTicketResponse("INVALIDO", "Empleado no existe o está inactivo.", null, null, "STAFF_INVALIDO");
        }

        var ticket = await _db.Tickets
            .Include(t => t.IdEstadoTicketNavigation)
            .Include(t => t.IdEventoAsientoNavigation)
                .ThenInclude(ea => ea.IdEventoNavigation)
            .FirstOrDefaultAsync(t => t.QrToken == request.qr_token, cancellationToken);

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

        if (!AllowedTicketStates.Contains(ticket.IdEstadoTicketNavigation.NombreEstado.ToUpperInvariant()))
        {
            var scanId = await SaveScanAsync(ticket.IdTicket, request.id_empleado, "INVALIDO", "Ticket cancelado o inactivo.", request.dispositivo, scanTime, cancellationToken);
            await CreateAlertAsync("TICKET_CANCELADO", "Scan de ticket cancelado/inactivo.", request.qr_token, request.id_empleado, request.dispositivo, ticket.IdTicket, scanId, cancellationToken);
            return new ScanTicketResponse("INVALIDO", "Ticket cancelado o inactivo.", ticket.IdTicket, scanId, "TICKET_CANCELADO");
        }

        if (request.id_evento is not null && ticket.IdEventoAsientoNavigation.IdEvento != request.id_evento)
        {
            var scanId = await SaveScanAsync(ticket.IdTicket, request.id_empleado, "INVALIDO", "Ticket para evento incorrecto.", request.dispositivo, scanTime, cancellationToken);
            await CreateAlertAsync("EVENTO_INCORRECTO", "Ticket usado en evento incorrecto.", request.qr_token, request.id_empleado, request.dispositivo, ticket.IdTicket, scanId, cancellationToken);
            return new ScanTicketResponse("INVALIDO", "Ticket no corresponde al evento.", ticket.IdTicket, scanId, "EVENTO_INCORRECTO");
        }

        var evento = ticket.IdEventoAsientoNavigation.IdEventoNavigation;
        var outOfSchedule = scanTime.Date != evento.FechaEvento.Date
            || scanTime < evento.FechaInicioVentas
            || scanTime > evento.FechaEvento.AddHours(6);

        if (outOfSchedule)
        {
            var scanId = await SaveScanAsync(ticket.IdTicket, request.id_empleado, "INVALIDO", "Ticket fuera de horario permitido.", request.dispositivo, scanTime, cancellationToken);
            await CreateAlertAsync("FUERA_DE_HORARIO", "Scan fuera de fecha/hora válida del evento.", request.qr_token, request.id_empleado, request.dispositivo, ticket.IdTicket, scanId, cancellationToken);
            return new ScanTicketResponse("INVALIDO", "Ticket fuera de fecha/hora válida.", ticket.IdTicket, scanId, "FUERA_DE_HORARIO");
        }

        var alreadyUsed = await _db.Scans
            .AsNoTracking()
            .AnyAsync(scan => scan.IdTicket == ticket.IdTicket && scan.Resultado == "VALIDO", cancellationToken);

        if (alreadyUsed)
        {
            var scanId = await SaveScanAsync(ticket.IdTicket, request.id_empleado, "DUPLICADO", "Intento de reutilización de ticket.", request.dispositivo, scanTime, cancellationToken);
            await CreateAlertAsync("QR_DUPLICADO", "Ticket ya escaneado previamente.", request.qr_token, request.id_empleado, request.dispositivo, ticket.IdTicket, scanId, cancellationToken);
            return new ScanTicketResponse("DUPLICADO", "Ticket ya fue usado.", ticket.IdTicket, scanId, "QR_DUPLICADO");
        }

        var validScanId = await SaveScanAsync(ticket.IdTicket, request.id_empleado, "VALIDO", "Acceso autorizado.", request.dispositivo, scanTime, cancellationToken);
        return new ScanTicketResponse("VALIDO", "Ticket válido.", ticket.IdTicket, validScanId, null);
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
        var scan = new Scan
        {
            IdTicket = idTicket,
            IdStaff = idStaff,
            Resultado = resultado,
            Observacion = observacion,
            Dispositivo = dispositivo,
            FechaScan = fechaScan
        };

        _db.Scans.Add(scan);
        await _db.SaveChangesAsync(cancellationToken);
        return scan.IdScan;
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
        var alert = new ScanAlert
        {
            TipoAlerta = tipoAlerta,
            Detalle = detalle,
            QrToken = qrToken,
            IdStaff = idStaff,
            IdTicket = idTicket,
            IdScan = idScan,
            Dispositivo = dispositivo,
            FechaAlerta = DateTime.UtcNow
        };

        _db.ScanAlerts.Add(alert);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
