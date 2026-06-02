using System;
using System.Collections.Generic;

namespace events_api.Entities;

public partial class Cache
{
    public string Key { get; set; } = null!;

    public string Value { get; set; } = null!;

    public long Expiration { get; set; }
}
