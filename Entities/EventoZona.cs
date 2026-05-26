using System;
using System.Collections.Generic;

namespace events_api.Entities;

public partial class EventoZona
{
    public int IdEventoZona { get; set; }

    public int IdEvento { get; set; }

    public int IdZona { get; set; }

    public decimal Precio { get; set; }

    public decimal? CargoServicio { get; set; }

    public int Capacidad { get; set; }

    public bool? Activo { get; set; }

    public virtual Evento IdEventoNavigation { get; set; } = null!;

    public virtual Zona IdZonaNavigation { get; set; } = null!;
}
