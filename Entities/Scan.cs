using System;
using System.Collections.Generic;

namespace events_api.Entities;

public partial class Scan
{
    public int IdScan { get; set; }

    public int IdTicket { get; set; }

    public int IdStaff { get; set; }

    public DateTime? FechaScan { get; set; }

    public string Resultado { get; set; } = null!;

    public string? Observacion { get; set; }

    public string? Dispositivo { get; set; }

    public virtual Staff IdStaffNavigation { get; set; } = null!;

    public virtual Ticket IdTicketNavigation { get; set; } = null!;

    public virtual ICollection<ScanAlert> ScanAlerts { get; set; } = new List<ScanAlert>();
}
