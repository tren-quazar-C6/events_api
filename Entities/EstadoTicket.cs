using System;
using System.Collections.Generic;

namespace events_api.Entities;

public partial class EstadoTicket
{
    public int IdEstadoTicket { get; set; }

    public string NombreEstado { get; set; } = null!;

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
