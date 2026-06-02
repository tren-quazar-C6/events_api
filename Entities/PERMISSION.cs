using System.Collections.Generic;

namespace events_api.Entities;

public partial class PERMISSION
{
    public int id_permission { get; set; }

    public string code { get; set; } = null!;

    public string name { get; set; } = null!;

    public bool? active { get; set; }

    public virtual ICollection<ROLE_PERMISSION> ROLE_PERMISSIONs { get; set; } = new List<ROLE_PERMISSION>();
}
