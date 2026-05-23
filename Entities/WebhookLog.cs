using System;
using System.Collections.Generic;

namespace events_api.Entities;

public partial class WebhookLog
{
    public int IdLog { get; set; }

    public string Proveedor { get; set; } = null!;

    public string? Evento { get; set; }

    public string PayloadJson { get; set; } = null!;

    public bool? Procesado { get; set; }

    public string? Error { get; set; }

    public DateTime? Fecha { get; set; }
}
