using System;
using System.Collections.Generic;

namespace events_api.Entities;

public partial class Job
{
    public ulong Id { get; set; }

    public string Queue { get; set; } = null!;

    public string Payload { get; set; } = null!;

    public ushort Attempts { get; set; }

    public uint? ReservedAt { get; set; }

    public uint AvailableAt { get; set; }

    public uint CreatedAt { get; set; }
}
