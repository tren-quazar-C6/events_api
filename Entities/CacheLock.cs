using System;
using System.Collections.Generic;

namespace events_api.Entities;

public partial class CacheLock
{
    public string Key { get; set; } = null!;

    public string Owner { get; set; } = null!;

    public long Expiration { get; set; }
}
