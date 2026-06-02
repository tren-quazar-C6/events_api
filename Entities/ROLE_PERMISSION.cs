namespace events_api.Entities;

public partial class RolePermission
{
    public int IdRolePermission { get; set; }

    public int IdRolStaff { get; set; }

    public int IdPermission { get; set; }

    public bool? Active { get; set; }

    public virtual RolStaff IdRolStaffNavigation { get; set; } = null!;

    public virtual Permission IdPermissionNavigation { get; set; } = null!;
}
