using System;
using System.Collections.Generic;

namespace events_api.Entities;

public partial class EventoAsiento
{
    public int IdEventoAsiento { get; set; }

    public int IdEvento { get; set; }

    public int IdAsiento { get; set; }

    public string? Estado { get; set; }

    public DateTime? FechaReserva { get; set; }

    public DateTime? ReservaExpira { get; set; }

    public virtual Asiento IdAsientoNavigation { get; set; } = null!;

    public virtual Evento IdEventoNavigation { get; set; } = null!;

    public virtual Ticket? Ticket { get; set; }
}
