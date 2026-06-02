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
        {
            throw new InvalidOperationException("La venta debe incluir al menos un asiento.");
        }

        var userExists = await _db.USUARIOs
            .AsNoTracking()
            .AnyAsync(u => u.id_usuario == request.id_usuario && u.activo == true, cancellationToken);
        if (!userExists)
        {
            throw new InvalidOperationException("Usuario no existe o está inactivo.");
        }

        var staffExists = await _db.STAFF
            .AsNoTracking()
            .AnyAsync(s => s.id_staff == request.id_staff && s.activo == true, cancellationToken);
        if (!staffExists)
        {
            throw new InvalidOperationException("Empleado no existe o está inactivo.");
        }

        var seats = await _db.EVENTO_ASIENTOs
            .Include(ea => ea.id_asientoNavigation)
            .Where(ea => request.id_evento_asientos.Contains(ea.id_evento_asiento))
            .ToListAsync(cancellationToken);

        if (seats.Count != request.id_evento_asientos.Count)
        {
            throw new InvalidOperationException("Uno o más asientos de evento no existen.");
        }

        if (seats.Any(seat => seat.estado != "DISPONIBLE"))
        {
            throw new InvalidOperationException("Solo se pueden vender asientos en estado DISPONIBLE.");
        }

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);

        var sale = new VENTA
        {
            id_usuario = request.id_usuario,
            id_staff = request.id_staff,
            tipo_venta = request.tipo_venta,
            total = 0m,
            estado_pago = "APPROVED",
            metodo_pago = request.metodo_pago,
            referencia_interna = $"SALE-{Guid.NewGuid():N}",
            fecha_pago = DateTime.UtcNow,
            fecha_venta = DateTime.UtcNow
        };

        _db.VENTAs.Add(sale);
        await _db.SaveChangesAsync(cancellationToken);

        decimal runningTotal = 0m;
        foreach (var seat in seats)
        {
            var price = await _db.EVENTO_ZONAs
                .AsNoTracking()
                .Where(ez => ez.id_evento == seat.id_evento && ez.id_zona == seat.id_asientoNavigation.id_zona && ez.activo == true)
                .Select(ez => (decimal?)ez.precio)
                .FirstOrDefaultAsync(cancellationToken) ?? 0m;

            runningTotal += price;

            _db.SALE_DETAILs.Add(new SALE_DETAIL
            {
                id_venta = sale.id_venta,
                id_evento_asiento = seat.id_evento_asiento,
                unit_price = price,
                quantity = 1,
                subtotal = price
            });

            seat.estado = "VENDIDO";
        }

        sale.total = runningTotal;
        await _db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);

        return new CreateSaleResponse(
            sale.id_venta,
            sale.total,
            sale.estado_pago ?? "APPROVED",
            seats.Select(s => s.id_evento_asiento).ToList());
    }

    public async Task<GenerateTicketsResponse> GenerateTicketsAsync(
        GenerateTicketsRequest request,
        CancellationToken cancellationToken = default)
    {
        var sale = await _db.VENTAs
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.id_venta == request.id_venta, cancellationToken);

        if (sale is null)
        {
            throw new InvalidOperationException("La venta no existe.");
        }

        var detailSeats = await _db.SALE_DETAILs
            .AsNoTracking()
            .Where(sd => sd.id_venta == request.id_venta)
            .Select(sd => sd.id_evento_asiento)
            .ToListAsync(cancellationToken);

        if (detailSeats.Count == 0)
        {
            throw new InvalidOperationException("La venta no tiene detalles para generar tickets.");
        }

        var existingTickets = await _db.TICKETs
            .AsNoTracking()
            .Where(t => t.id_venta == request.id_venta)
            .Select(t => t.id_evento_asiento)
            .ToListAsync(cancellationToken);

        var pendingSeats = detailSeats.Except(existingTickets).ToList();
        if (pendingSeats.Count == 0)
        {
            throw new InvalidOperationException("Todos los tickets de esta venta ya fueron generados.");
        }

        var activeStatusId = await _db.ESTADO_TICKETs
            .AsNoTracking()
            .Where(status => status.nombre_estado == "ACTIVO")
            .Select(status => (int?)status.id_estado_ticket)
            .FirstOrDefaultAsync(cancellationToken);

        if (activeStatusId is null)
        {
            throw new InvalidOperationException("No existe el estado ACTIVO en ESTADO_TICKET.");
        }

        var createdCodes = new List<string>();
        foreach (var idEventoAsiento in pendingSeats)
        {
            var uniqueCode = $"TK-{Guid.NewGuid():N[..10].ToUpperInvariant()}";
            var qrToken = $"QR-{Guid.NewGuid():N}";

            _db.TICKETs.Add(new TICKET
            {
                id_venta = request.id_venta,
                id_estado_ticket = activeStatusId.Value,
                id_evento_asiento = idEventoAsiento,
                codigo_unico = uniqueCode,
                qr_token = qrToken,
                precio_pagado = 0m,
                fecha_generacion = DateTime.UtcNow
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
