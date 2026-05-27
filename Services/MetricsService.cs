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
        return await _db.VENTAs
            .AsNoTracking()
            .Where(venta => venta.estado_pago == "APPROVED"
                && venta.fecha_venta >= desde
                && venta.fecha_venta <= hasta)
            .SumAsync(venta => venta.total, cancellationToken);
    }

    public async Task<int> GetTicketsSoldAsync(
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken = default)
    {
        return await _db.TICKETs
            .AsNoTracking()
            .Where(ticket => ticket.id_ventaNavigation.estado_pago == "APPROVED"
                && ticket.id_ventaNavigation.fecha_venta >= desde
                && ticket.id_ventaNavigation.fecha_venta <= hasta)
            .CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<(int week, decimal revenue)>> GetWeeklySalesAsync(
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken = default)
    {
        var sales = await _db.VENTAs
            .AsNoTracking()
            .Where(venta => venta.estado_pago == "APPROVED"
                && venta.fecha_venta >= desde
                && venta.fecha_venta <= hasta)
            .Select(venta => new { venta.fecha_venta, venta.total })
            .ToListAsync(cancellationToken);

        var weeklySales = sales
            .GroupBy(s => ISOWeek.GetWeekOfYear(DateOnly.FromDateTime(s.fecha_venta.Value)))
            .Select(g => (week: g.Key, revenue: g.Sum(s => s.total)))
            .OrderBy(ws => ws.week)
            .ToList();

        return weeklySales;
    }

    public async Task<decimal> GetAttendanceRateAsync(
        int idEvento,
        CancellationToken cancellationToken = default)
    {
        var validScans = await _db.SCANs
            .AsNoTracking()
            .CountAsync(scan => scan.id_ticketNavigation.id_evento_asientoNavigation.id_evento == idEvento
                && scan.resultado == "VALIDO", cancellationToken);

        var totalTickets = await _db.TICKETs
            .AsNoTracking()
            .CountAsync(ticket => ticket.id_evento_asientoNavigation.id_evento == idEvento, cancellationToken);

        if (totalTickets == 0)
            return 0;

        return (decimal)validScans / totalTickets * 100;
    }
}