namespace events_api.Entities;

public partial class ROLE_PERMISSION
{
    public int id_role_permission { get; set; }

    public int id_rol_staff { get; set; }

    public int id_permission { get; set; }

    public bool? active { get; set; }

    public virtual ROL_STAFF id_rol_staffNavigation { get; set; } = null!;

    public virtual PERMISSION id_permissionNavigation { get; set; } = null!;
}
