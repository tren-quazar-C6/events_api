using System.Globalization;
using events_api.Data;
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

    public async Task<IReadOnlyCollection<(int week, decimal revenue)>> GetWeeklySalesAsync(
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
            .GroupBy(s => ISOWeek.GetWeekOfYear(DateOnly.FromDateTime(s.fecha_venta)))
            .Select(g => (week: g.Key, revenue: g.Sum(s => s.Total)))
            .OrderBy(ws => ws.week)
            .ToList();

        return weeklySales;
    }

    public async Task<decimal> GetAttendanceRateAsync(
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

        if (totalTickets == 0)
            return 0;

        return (decimal)validScans / totalTickets * 100;
    }
}
