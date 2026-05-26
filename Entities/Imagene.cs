using System;
using System.Collections.Generic;

namespace events_api.Entities;

public partial class Imagene
{
    public int IdImagen { get; set; }

    public int IdEvento { get; set; }

    public string RutaUrl { get; set; } = null!;

    public bool? Principal { get; set; }

    public int? Orden { get; set; }

    public bool? Activo { get; set; }

    public virtual Evento IdEventoNavigation { get; set; } = null!;
}
