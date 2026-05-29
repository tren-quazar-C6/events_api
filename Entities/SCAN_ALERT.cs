using System;

namespace events_api.Entities;

public partial class SCAN_ALERT
{
    public int id_scan_alert { get; set; }

    public int? id_scan { get; set; }

    public int? id_ticket { get; set; }

    public int? id_staff { get; set; }

    public string tipo_alerta { get; set; } = null!;

    public string? detalle { get; set; }

    public string? qr_token { get; set; }

    public string? dispositivo { get; set; }

    public string? payload_json { get; set; }

    public DateTime? fecha_alerta { get; set; }

    public virtual SCAN? id_scanNavigation { get; set; }

    public virtual STAFF? id_staffNavigation { get; set; }

    public virtual TICKET? id_ticketNavigation { get; set; }
}
