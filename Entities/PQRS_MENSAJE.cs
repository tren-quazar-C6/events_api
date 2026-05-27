using System;
using System.Collections.Generic;

namespace events_api.Entities;

public partial class PQRS_MENSAJE
{
    public int id_mensaje { get; set; }

    public int id_pqrs { get; set; }

    public string remitente { get; set; } = null!;

    public int id_remitente { get; set; }

    public string mensaje { get; set; } = null!;

    public DateTime? fecha { get; set; }

    public virtual PQR id_pqrsNavigation { get; set; } = null!;
}
