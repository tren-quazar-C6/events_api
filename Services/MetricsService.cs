using System.Globalization;
using events_api.Data;
using events_api.DTOs;
using events_api.Entities;
using Microsoft.EntityFrameworkCore;

namespace events_api.Services;

public class MetricsService
{
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
            .Where(venta => venta.EstadoPago == "APPROVED"
                && venta.FechaVenta >= desde
                && venta.FechaVenta <= hasta)
            .SumAsync(venta => venta.Total, cancellationToken);
    }

    public async Task<int> GetTicketsSoldAsync(
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken = default)
    {
        return await _db.Tickets
            .AsNoTracking()
            .Where(ticket => ticket.IdVentaNavigation.EstadoPago == "APPROVED"
                && ticket.IdVentaNavigation.FechaVenta >= desde
                && ticket.IdVentaNavigation.FechaVenta <= hasta)
            .CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<WeeklySalesDto>> GetWeeklySalesAsync(
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken = default)
    {
        var sales = await _db.Ventas
            .AsNoTracking()
            .Where(venta => venta.EstadoPago == "APPROVED"
                && venta.FechaVenta >= desde
                && venta.FechaVenta <= hasta)
            .Where(venta => venta.FechaVenta.HasValue)
            .Select(venta => new { fecha_venta = venta.FechaVenta!.Value, venta.Total })
            .ToListAsync(cancellationToken);

        var weeklySales = sales
            .GroupBy(s => new
            {
                Anio = ISOWeek.GetYear(s.fecha_venta),
                Semana = ISOWeek.GetWeekOfYear(s.fecha_venta)
            })
            .Select(g => new WeeklySalesDto(
                g.Key.Anio,
                g.Key.Semana,
                g.Sum(s => s.Total),
                g.Count()))
            .OrderBy(ws => ws.Anio)
            .ThenBy(ws => ws.Semana)
            .ToList();

        return weeklySales;
    }

    public async Task<AttendanceRateDto> GetAttendanceRateAsync(
        int idEvento,
        CancellationToken cancellationToken = default)
    {
        var validScans = await _db.Scans
            .AsNoTracking()
            .CountAsync(scan => scan.IdTicketNavigation.IdEventoAsientoNavigation.IdEvento == idEvento
                && scan.Resultado == "VALIDO", cancellationToken);

        var totalTickets = await _db.Tickets
            .AsNoTracking()
            .CountAsync(ticket => ticket.IdEventoAsientoNavigation.IdEvento == idEvento, cancellationToken);

        var attendanceRate = totalTickets == 0
            ? 0d
            : (double)validScans / totalTickets * 100;

        return new AttendanceRateDto(
            idEvento,
            totalTickets,
            validScans,
            attendanceRate);
    }
}
