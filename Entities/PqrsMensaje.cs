using System;
using System.Collections.Generic;

namespace events_api.Entities;

public partial class PqrsMensaje
{
    public int IdMensaje { get; set; }

    public int IdPqrs { get; set; }

    public string Remitente { get; set; } = null!;

    public int IdRemitente { get; set; }

    public string Mensaje { get; set; } = null!;

    public DateTime? Fecha { get; set; }

    public virtual Pqr IdPqrsNavigation { get; set; } = null!;
}
