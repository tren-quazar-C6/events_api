using System;
using System.Collections.Generic;

namespace events_api.Entities;

public partial class ScanAlert
{
    public int IdScanAlert { get; set; }

    public int? IdScan { get; set; }

    public int? IdTicket { get; set; }

    public int? IdStaff { get; set; }

    public string TipoAlerta { get; set; } = null!;

    public string? Detalle { get; set; }

    public string? QrToken { get; set; }

    public string? Dispositivo { get; set; }

    public string? PayloadJson { get; set; }

    public DateTime? FechaAlerta { get; set; }

    public virtual Scan? IdScanNavigation { get; set; }

    public virtual Staff? IdStaffNavigation { get; set; }

    public virtual Ticket? IdTicketNavigation { get; set; }
}
