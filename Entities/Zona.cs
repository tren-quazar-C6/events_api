using System;
using System.Collections.Generic;

namespace events_api.Entities;

public partial class Zona
{
    public int IdZona { get; set; }

    public string NombreZona { get; set; } = null!;

    public string? ColorHex { get; set; }

    public bool? Activo { get; set; }

    public virtual ICollection<Asiento> Asientos { get; set; } = new List<Asiento>();
}
