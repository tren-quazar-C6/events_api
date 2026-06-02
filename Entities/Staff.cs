using System;
using System.Collections.Generic;

namespace events_api.Entities;

public partial class Staff
{
    public int IdStaff { get; set; }

    public int IdRolStaff { get; set; }

    public string Nombre { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public bool? Activo { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public virtual ICollection<Auditorium> Auditoria { get; set; } = new List<Auditorium>();

    public virtual ICollection<Evento> Eventos { get; set; } = new List<Evento>();

    public virtual RolStaff IdRolStaffNavigation { get; set; } = null!;

    public virtual ICollection<EmployeeRole> EmployeeRoles { get; set; } = new List<EmployeeRole>();

    public virtual ICollection<Pqr> Pqrs { get; set; } = new List<Pqr>();

    public virtual ICollection<ScanAlert> ScanAlerts { get; set; } = new List<ScanAlert>();

    public virtual ICollection<Scan> Scans { get; set; } = new List<Scan>();

    public virtual ICollection<Venta> Venta { get; set; } = new List<Venta>();
}
