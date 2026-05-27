using System;
using System.Collections.Generic;

namespace events_api.Entities;

public partial class AUDITORIum
{
    public int id_auditoria { get; set; }

    public int id_staff { get; set; }

    public string accion { get; set; } = null!;

    public string tabla_afectada { get; set; } = null!;

    public int id_registro_afectado { get; set; }

    public string? detalle { get; set; }

    public DateTime? fecha { get; set; }

    public virtual STAFF id_staffNavigation { get; set; } = null!;
}
