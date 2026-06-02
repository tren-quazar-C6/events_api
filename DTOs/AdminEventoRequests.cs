using System.ComponentModel.DataAnnotations;

namespace events_api.DTOs;

public record CreateEventoRequest(
    [Required] int id_tipo_evento,
    [Required] int creado_por_staff,

    [Required]
    [StringLength(200, MinimumLength = 3)]
    string nombre_evento,

    string? descripcion,

    [Required] DateTime fecha_evento,
    [Required] DateTime fecha_inicio_ventas,
    [Required] DateTime fecha_fin_ventas,

    [Range(1, 100000)] int capacidad_total,
    
    string? ruta_url,

    IReadOnlyCollection<EventoZonaRequest>? zonas
);

public record EventoZonaRequest(
    [Required] int id_zona,
    [Range(0, 10_000_000)] decimal precio,
    [Range(0, 10_000_000)] decimal cargo_servicio = 0,
    [Range(1, 100_000)] int capacidad = 1
);

public record UpdateEventoRequest(
    int? id_tipo_evento,

    [StringLength(200, MinimumLength = 3)]
    string? nombre_evento,

    string? descripcion,
    DateTime? fecha_evento,
    DateTime? fecha_inicio_ventas,
    DateTime? fecha_fin_ventas,

    [Range(1, 100_000)]
    int? capacidad_total,
    
    string? ruta_url
);

public record UpdateStatusRequest(
    [Required]
    [RegularExpression("^(DRAFT|PUBLISHED|CANCELLED)$",
        ErrorMessage = "Status debe ser DRAFT, PUBLISHED o CANCELLED")]
    string status,

    string? motivo_cancelacion
);

public record UpdatePricingRequest(
    [Required]
    [MinLength(1, ErrorMessage = "Debe enviar al menos una zona")]
    IReadOnlyCollection<ZonaPrecioRequest> zonas
);

public record ZonaPrecioRequest(
    [Required] int id_zona,
    [Range(0, 10_000_000)] decimal nuevo_precio,
    [Range(0, 10_000_000)] decimal? nuevo_cargo_servicio
);