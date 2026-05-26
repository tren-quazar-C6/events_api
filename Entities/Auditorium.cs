using System;
using System.Collections.Generic;

namespace events_api.Entities;

public partial class Auditorium
{
    public int IdAuditoria { get; set; }

    public int IdStaff { get; set; }

    public string Accion { get; set; } = null!;

    public string TablaAfectada { get; set; } = null!;

    public int IdRegistroAfectado { get; set; }

    public string? Detalle { get; set; }

    public DateTime? Fecha { get; set; }

    public virtual Staff IdStaffNavigation { get; set; } = null!;
}
