using events_api.Data;
using events_api.DTOs;
using events_api.Entities;
using Microsoft.EntityFrameworkCore;

namespace events_api.Services;

public class SalesService
{
    private readonly QuasarDbContext _db;

    public SalesService(QuasarDbContext db)
    {
        _db = db;
    }

    public async Task<CreateSaleResponse> CreateSaleAsync(
        CreateSaleRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.id_evento_asientos.Count == 0)
            throw new InvalidOperationException("La venta debe incluir al menos un asiento.");

        var userExists = await _db.Usuarios
            .AsNoTracking()
            .AnyAsync(u => u.IdUsuario == request.id_usuario && u.Activo == true, cancellationToken);

        if (!userExists)
            throw new InvalidOperationException("Usuario no existe o está inactivo.");

        var staffExists = await _db.Staff
            .AsNoTracking()
            .AnyAsync(s => s.IdStaff == request.id_staff && s.Activo == true, cancellationToken);

        if (!staffExists)
            throw new InvalidOperationException("Empleado no existe o está inactivo.");

        var seats = await _db.EventoAsientos
            .Include(ea => ea.IdAsientoNavigation)
            .Where(ea => request.id_evento_asientos.Contains(ea.IdEventoAsiento))
            .ToListAsync(cancellationToken);

        if (seats.Count != request.id_evento_asientos.Count)
            throw new InvalidOperationException("Uno o más asientos de evento no existen.");

        if (seats.Any(seat => seat.Estado != "DISPONIBLE"))
            throw new InvalidOperationException("Solo se pueden vender asientos en estado DISPONIBLE.");

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);

        var sale = new Venta
        {
            IdUsuario = request.id_usuario,
            IdStaff = request.id_staff,
            TipoVenta = request.tipo_venta,
            Total = 0m,
            EstadoPago = "APPROVED",
            MetodoPago = request.metodo_pago,
            ReferenciaInterna = $"SALE-{Guid.NewGuid():N}",
            FechaPago = DateTime.UtcNow,
            FechaVenta = DateTime.UtcNow
        };

        _db.Ventas.Add(sale);
        await _db.SaveChangesAsync(cancellationToken);

        decimal runningTotal = 0m;
        foreach (var seat in seats)
        {
            var price = await _db.EventoZonas
                .AsNoTracking()
                .Where(ez => ez.IdEvento == seat.IdEvento && ez.IdZona == seat.IdAsientoNavigation.IdZona && ez.Activo == true)
                .Select(ez => (decimal?)ez.Precio)
                .FirstOrDefaultAsync(cancellationToken) ?? 0m;

            runningTotal += price;

            _db.SaleDetails.Add(new SaleDetail
            {
                IdVenta = sale.IdVenta,
                IdEventoAsiento = seat.IdEventoAsiento,
                UnitPrice = price,
                Quantity = 1,
                Subtotal = price
            });

            seat.Estado = "VENDIDO";
        }

        sale.Total = runningTotal;
        await _db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);

        return new CreateSaleResponse(
            sale.IdVenta,
            sale.Total,
            sale.EstadoPago ?? "APPROVED",
            seats.Select(s => s.IdEventoAsiento).ToList());
    }

    public async Task<GenerateTicketsResponse> GenerateTicketsAsync(
        GenerateTicketsRequest request,
        CancellationToken cancellationToken = default)
    {
        var sale = await _db.Ventas
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.IdVenta == request.id_venta, cancellationToken);

        if (sale is null)
            throw new InvalidOperationException("La venta no existe.");

        var detailSeats = await _db.SaleDetails
            .AsNoTracking()
            .Where(sd => sd.IdVenta == request.id_venta)
            .Select(sd => sd.IdEventoAsiento)
            .ToListAsync(cancellationToken);

        if (detailSeats.Count == 0)
            throw new InvalidOperationException("La venta no tiene detalles para generar tickets.");

        var existingTickets = await _db.Tickets
            .AsNoTracking()
            .Where(t => t.IdVenta == request.id_venta)
            .Select(t => t.IdEventoAsiento)
            .ToListAsync(cancellationToken);

        var pendingSeats = detailSeats.Except(existingTickets).ToList();
        if (pendingSeats.Count == 0)
            throw new InvalidOperationException("Todos los tickets de esta venta ya fueron generados.");

        var activeStatusId = await _db.EstadoTickets
            .AsNoTracking()
            .Where(status => status.NombreEstado == "ACTIVO")
            .Select(status => (int?)status.IdEstadoTicket)
            .FirstOrDefaultAsync(cancellationToken);

        if (activeStatusId is null)
            throw new InvalidOperationException("No existe el estado ACTIVO en ESTADO_TICKET.");

        var createdCodes = new List<string>();
        foreach (var idEventoAsiento in pendingSeats)
        {
            var uniqueCode = $"TK-{Guid.NewGuid():N[..10].ToUpperInvariant()}";
            var qrToken = $"QR-{Guid.NewGuid():N}";

            _db.Tickets.Add(new Ticket
            {
                IdVenta = request.id_venta,
                IdEstadoTicket = activeStatusId.Value,
                IdEventoAsiento = idEventoAsiento,
                CodigoUnico = uniqueCode,
                QrToken = qrToken,
                PrecioPagado = 0m,
                FechaGeneracion = DateTime.UtcNow
            });

            createdCodes.Add(uniqueCode);
        }

        await _db.SaveChangesAsync(cancellationToken);

        return new GenerateTicketsResponse(
            request.id_venta,
            createdCodes.Count,
            createdCodes);
    }
}
