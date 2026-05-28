namespace events_api.DTOs;

public record AdminEventoDetalleDto(
    int id_evento,
    string nombre_evento,
    string? descripcion,
    DateTime fecha_evento,
    DateTime fecha_inicio_ventas,
    DateTime fecha_fin_ventas,
    DateTime? fecha_creacion,
    int capacidad_total,
    int id_tipo_evento,
    string tipo_evento,
    string status,
    DateTime? fecha_cancelacion,
    string? motivo_cancelacion,
    IReadOnlyCollection<ImagenEventoDto> imagenes,
    IReadOnlyCollection<EventoZonaDto> zonas,
    int asientos_disponibles,
    int asientos_reservados,
    int asientos_vendidos
);

public record EventoZonaDto(
    int id_evento_zona,
    int id_zona,
    string nombre_zona,
    string? color_hex,
    decimal precio,
    decimal cargo_servicio,
    int capacidad,
    bool activo
);

public record AdminEventoResumenDto(
    int id_evento,
    string nombre_evento,
    DateTime fecha_evento,
    DateTime fecha_inicio_ventas,
    DateTime fecha_fin_ventas,
    int capacidad_total,
    string tipo_evento,
    string? imagen_principal,
    string status,
    int total_zonas
);