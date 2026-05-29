using System;
using System.Collections.Generic;

namespace events_api.Entities;

public partial class PQR
{
    public int id_pqrs { get; set; }

    public int id_usuario { get; set; }

    public int? asignado_staff { get; set; }

    public string tipo { get; set; } = null!;

    public string asunto { get; set; } = null!;

    public string? estado { get; set; }

    public DateTime? fecha_creacion { get; set; }

    public DateTime? fecha_ultima_respuesta { get; set; }

    public virtual ICollection<PQRS_MENSAJE> PQRS_MENSAJEs { get; set; } = new List<PQRS_MENSAJE>();

    public virtual STAFF? asignado_staffNavigation { get; set; }

    public virtual USUARIO id_usuarioNavigation { get; set; } = null!;
}
