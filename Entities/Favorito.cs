using System;
using System.Collections.Generic;

namespace events_api.Entities;

public partial class Favorito
{
    public int IdFavorito { get; set; }

    public int IdUsuario { get; set; }

    public int IdEvento { get; set; }

    public DateTime? FechaAgregado { get; set; }

    public virtual Evento IdEventoNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
