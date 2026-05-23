using System;
using System.Collections.Generic;

namespace events_api.Entities;

public partial class Venta
{
    public int IdVenta { get; set; }

    public int IdUsuario { get; set; }

    public int? IdStaff { get; set; }

    public string TipoVenta { get; set; } = null!;

    public decimal Total { get; set; }

    public string? Moneda { get; set; }

    public string? EstadoPago { get; set; }

    public string? MetodoPago { get; set; }

    public string? ReferenciaInterna { get; set; }

    public string? ReferenciaWompi { get; set; }

    public string? IdTransaccionWompi { get; set; }

    public string? JsonRespuesta { get; set; }

    public DateTime? FechaPago { get; set; }

    public DateTime? FechaVenta { get; set; }

    public virtual Staff? IdStaffNavigation { get; set; }

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    public virtual ICollection<TransaccionesPago> TransaccionesPagos { get; set; } = new List<TransaccionesPago>();
}
