using System;
using System.Collections.Generic;

namespace events_api.Entities;

public partial class Pqr
{
    public int IdPqrs { get; set; }

    public int IdUsuario { get; set; }

    public int? AsignadoStaff { get; set; }

    public string Tipo { get; set; } = null!;

    public string Asunto { get; set; } = null!;

    public string Mensaje { get; set; } = null!;

    public string? Estado { get; set; }

    public string? Respuesta { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public DateTime? FechaRespuesta { get; set; }

    public virtual Staff? AsignadoStaffNavigation { get; set; }

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
