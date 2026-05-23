using System;
using System.Collections.Generic;

namespace events_api.Entities;

public partial class Asiento
{
    public int IdAsiento { get; set; }

    public int IdZona { get; set; }

    public string Fila { get; set; } = null!;

    public int Numero { get; set; }

    public string CodigoAsiento { get; set; } = null!;

    public int PosX { get; set; }

    public int PosY { get; set; }

    public bool? Activo { get; set; }

    public virtual ICollection<EventoAsiento> EventoAsientos { get; set; } = new List<EventoAsiento>();

    public virtual Zona IdZonaNavigation { get; set; } = null!;
}
