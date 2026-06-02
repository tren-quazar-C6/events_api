namespace events_api.Entities;

public partial class EMPLOYEE_ROLE
{
    public int id_employee_role { get; set; }

    public int id_staff { get; set; }

    public int id_rol_staff { get; set; }

    public bool? active { get; set; }

    public virtual STAFF id_staffNavigation { get; set; } = null!;

    public virtual ROL_STAFF id_rol_staffNavigation { get; set; } = null!;
}
