using System;
using System.Collections.Generic;

namespace events_api.Entities;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    public string Nombre { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? PasswordHash { get; set; }

    public string? Telefono { get; set; }

    public string? FotoPerfil { get; set; }

    public string? GoogleId { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public bool? Activo { get; set; }

    public virtual ICollection<Favorito> Favoritos { get; set; } = new List<Favorito>();

    public virtual ICollection<Notificacione> Notificaciones { get; set; } = new List<Notificacione>();

    public virtual ICollection<Pqr> Pqrs { get; set; } = new List<Pqr>();

    public virtual ICollection<Venta> Venta { get; set; } = new List<Venta>();
}
