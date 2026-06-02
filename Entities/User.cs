using System;
using System.Collections.Generic;

namespace events_api.Entities;

public partial class User
{
    public ulong Id { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public DateTime? EmailVerifiedAt { get; set; }

    public string Password { get; set; } = null!;

    public string? TwoFactorSecret { get; set; }

    public string? TwoFactorRecoveryCodes { get; set; }

    public DateTime? TwoFactorConfirmedAt { get; set; }

    public string? RememberToken { get; set; }

    public ulong? CurrentTeamId { get; set; }

    public string? ProfilePhotoPath { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Passkey> Passkeys { get; set; } = new List<Passkey>();
}
