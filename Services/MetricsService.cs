using System.Globalization;
using events_api.Data;
using events_api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace events_api.Services;

public class MetricsService
{
    private const string ApprovedPaymentStatus = "APPROVED";
    private const string ValidScanResult = "VALIDO";

    private readonly QuasarDbContext _db;

    public MetricsService(QuasarDbContext db)
    {
        _db = db;
    }

    public async Task<decimal> GetRevenueTotalAsync(
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken = default)
    {
        return await _db.Ventas
            .AsNoTracking()
            .Where(venta => venta.EstadoPago == ApprovedPaymentStatus)
            .Where(venta => venta.FechaVenta >= desde && venta.FechaVenta <= hasta)
            .SumAsync(venta => venta.Total, cancellationToken);
    }

    public async Task<int> GetTicketsSoldAsync(
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken = default)
    {
        return await _db.Tickets
            .AsNoTracking()
            .Where(ticket => ticket.IdVentaNavigation.EstadoPago == ApprovedPaymentStatus)
            .Where(ticket => ticket.FechaGeneracion >= desde && ticket.FechaGeneracion <= hasta)
            .CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<WeeklySalesDto>> GetWeeklySalesAsync(
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken = default)
    {
        var ventas = await _db.Ventas
            .AsNoTracking()
            .Where(venta => venta.EstadoPago == ApprovedPaymentStatus)
            .Where(venta => venta.FechaVenta >= desde && venta.FechaVenta <= hasta)
            .Select(venta => new
            {
                FechaVenta = venta.FechaVenta!.Value,
                venta.Total
            })
            .ToListAsync(cancellationToken);

        return ventas
            .GroupBy(venta => new
            {
                Anio = ISOWeek.GetYear(venta.FechaVenta),
                Semana = ISOWeek.GetWeekOfYear(venta.FechaVenta)
            })
            .Select(grupo => new WeeklySalesDto(
                grupo.Key.Anio,
                grupo.Key.Semana,
                grupo.Sum(venta => venta.Total),
                grupo.Count()))
            .OrderBy(venta => venta.Anio)
            .ThenBy(venta => venta.Semana)
            .ToList();
    }

    public async Task<AttendanceRateDto> GetAttendanceRateAsync(
        int idEvento,
        CancellationToken cancellationToken = default)
    {
        var totalTickets = await _db.Tickets
            .AsNoTracking()
            .Where(ticket => ticket.IdEventoAsientoNavigation.IdEvento == idEvento)
            .CountAsync(cancellationToken);

        if (totalTickets == 0)
        {
            return new AttendanceRateDto(idEvento, 0, 0, 0);
        }

        var asistentes = await _db.Scans
            .AsNoTracking()
            .Where(scan => scan.Resultado == ValidScanResult)
            .Where(scan => scan.IdTicketNavigation.IdEventoAsientoNavigation.IdEvento == idEvento)
            .Select(scan => scan.IdTicket)
            .Distinct()
            .CountAsync(cancellationToken);

        return new AttendanceRateDto(
            idEvento,
            totalTickets,
            asistentes,
            (double)asistentes / totalTickets);
    }
}
