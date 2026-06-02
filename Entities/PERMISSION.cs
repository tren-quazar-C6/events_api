using System.Collections.Generic;

namespace events_api.Entities;

public partial class Permission
{
    public int IdPermission { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public bool? Active { get; set; }

    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
