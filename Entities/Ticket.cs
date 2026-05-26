using System;
using System.Collections.Generic;

namespace events_api.Entities;

public partial class Ticket
{
    public int IdTicket { get; set; }

    public int IdVenta { get; set; }

    public int IdEstadoTicket { get; set; }

    public int IdEventoAsiento { get; set; }

    public string CodigoUnico { get; set; } = null!;

    public string QrToken { get; set; } = null!;

    public decimal PrecioPagado { get; set; }

    public DateTime? FechaGeneracion { get; set; }

    public DateTime? FechaImpresion { get; set; }

    public virtual EstadoTicket IdEstadoTicketNavigation { get; set; } = null!;

    public virtual EventoAsiento IdEventoAsientoNavigation { get; set; } = null!;

    public virtual Venta IdVentaNavigation { get; set; } = null!;

    public virtual ICollection<Scan> Scans { get; set; } = new List<Scan>();
}
