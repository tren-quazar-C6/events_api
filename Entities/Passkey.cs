using System;
using System.Collections.Generic;

namespace events_api.Entities;

public partial class Passkey
{
    public ulong Id { get; set; }

    public ulong UserId { get; set; }

    public string Name { get; set; } = null!;

    public string CredentialId { get; set; } = null!;

    public string Credential { get; set; } = null!;

    public DateTime? LastUsedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
