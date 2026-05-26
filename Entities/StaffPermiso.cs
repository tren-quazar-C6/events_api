using System;
using System.Collections.Generic;

namespace events_api.Entities;

public partial class StaffPermiso
{
    public int IdPermiso { get; set; }

    public int IdStaff { get; set; }

    public string Portal { get; set; } = null!;

    public bool? Activo { get; set; }

    public DateTime? FechaAsignacion { get; set; }

    public virtual Staff IdStaffNavigation { get; set; } = null!;
}
