using System;
using System.Collections.Generic;

namespace events_api.Entities;

public partial class Notificacione
{
    public int IdNotificacion { get; set; }

    public int IdUsuario { get; set; }

    public string? Tipo { get; set; }

    public int? IdEvento { get; set; }

    public string Titulo { get; set; } = null!;

    public string Mensaje { get; set; } = null!;

    public bool? Leido { get; set; }

    public DateTime? FechaEnvio { get; set; }

    public virtual Evento? IdEventoNavigation { get; set; }

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
