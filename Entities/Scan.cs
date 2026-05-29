using System;
using System.Collections.Generic;

namespace events_api.Entities;

public partial class SCAN
{
    public int id_scan { get; set; }

    public int id_ticket { get; set; }

    public int id_staff { get; set; }

    public DateTime? fecha_scan { get; set; }

    public string resultado { get; set; } = null!;

    public string? observacion { get; set; }

    public string? dispositivo { get; set; }

    public virtual ICollection<SCAN_ALERT> SCAN_ALERTs { get; set; } = new List<SCAN_ALERT>();

    public virtual STAFF id_staffNavigation { get; set; } = null!;

    public virtual TICKET id_ticketNavigation { get; set; } = null!;
}
