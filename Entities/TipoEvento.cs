using System;
using System.Collections.Generic;

namespace events_api.Entities;

public partial class TipoEvento
{
    public int IdTipoEvento { get; set; }

    public string NombreTipo { get; set; } = null!;

    public bool? Activo { get; set; }

    public virtual ICollection<Evento> Eventos { get; set; } = new List<Evento>();
}
