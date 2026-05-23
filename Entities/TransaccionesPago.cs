using System;
using System.Collections.Generic;

namespace events_api.Entities;

public partial class TransaccionesPago
{
    public int IdTransaccion { get; set; }

    public int IdVenta { get; set; }

    public string? ProveedorPago { get; set; }

    public string? IdTransaccionExt { get; set; }

    public string? Estado { get; set; }

    public string? MetodoPago { get; set; }

    public decimal Monto { get; set; }

    public string? Moneda { get; set; }

    public string? Referencia { get; set; }

    public string? RespuestaJson { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public virtual Venta IdVentaNavigation { get; set; } = null!;
}
