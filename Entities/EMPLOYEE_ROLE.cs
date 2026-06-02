namespace events_api.Entities;

public partial class EmployeeRole
{
    public int IdEmployeeRole { get; set; }

    public int IdStaff { get; set; }

    public int IdRolStaff { get; set; }

    public bool? Active { get; set; }

    public virtual Staff IdStaffNavigation { get; set; } = null!;

    public virtual RolStaff IdRolStaffNavigation { get; set; } = null!;
}
