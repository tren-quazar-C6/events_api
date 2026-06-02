using System;
using System.Collections.Generic;

namespace events_api.Entities;

public partial class Evento
{
    public int IdEvento { get; set; }

    public int IdTipoEvento { get; set; }

    public int CreadoPorStaff { get; set; }

    public string NombreEvento { get; set; } = null!;

    public string? Descripcion { get; set; }

    public DateTime FechaEvento { get; set; }

    public DateTime FechaInicioVentas { get; set; }

    public DateTime FechaFinVentas { get; set; }

    public int CapacidadTotal { get; set; }

    public bool? Publicado { get; set; }

    public bool? Activo { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public DateTime? FechaCancelacion { get; set; }

    public string? MotivoCancelacion { get; set; }

    public string RutaUrl { get; set; } = null!;

    public virtual Staff CreadoPorStaffNavigation { get; set; } = null!;

    public virtual ICollection<EventoAsiento> EventoAsientos { get; set; } = new List<EventoAsiento>();

    public virtual ICollection<EventoZona> EventoZonas { get; set; } = new List<EventoZona>();

    public virtual ICollection<Favorito> Favoritos { get; set; } = new List<Favorito>();

    public virtual TipoEvento IdTipoEventoNavigation { get; set; } = null!;

    public virtual ICollection<Notificacione> Notificaciones { get; set; } = new List<Notificacione>();
}
