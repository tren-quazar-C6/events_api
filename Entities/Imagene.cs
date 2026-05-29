using System;
using System.Collections.Generic;

namespace events_api.Entities;

public partial class IMAGENE
{
    public int id_imagen { get; set; }

    public int id_evento { get; set; }

    public string ruta_url { get; set; } = null!;

    public bool? principal { get; set; }

    public int? orden { get; set; }

    public bool? activo { get; set; }

    public virtual EVENTO id_eventoNavigation { get; set; } = null!;
}
