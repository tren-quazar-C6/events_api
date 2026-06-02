using System;
using System.Collections.Generic;

namespace events_api.Entities;

public partial class RolStaff
{
    public int IdRolStaff { get; set; }

    public string NombreRol { get; set; } = null!;

    public bool? Activo { get; set; }

    public virtual ICollection<Staff> Staff { get; set; } = new List<Staff>();
}
