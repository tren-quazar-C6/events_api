namespace events_api.DTOs;

/// <summary>
/// Resumen de un PQRS para la lista del panel admin.
/// </summary>
public record PqrsResumenDto(
    int id_pqrs,
    string tipo,
    string asunto,
    string estado,
    string nombre_usuario,
    string email_usuario,
    string? nombre_staff_asignado,
    DateTime? fecha_creacion,
    DateTime? fecha_ultima_respuesta,
    int total_mensajes
);

/// <summary>
/// Detalle completo de un PQRS con el hilo de mensajes.
/// </summary>
public record PqrsDetalleDto(
    int id_pqrs,
    string tipo,
    string asunto,
    string estado,
    // Datos del usuario que creó el PQRS
    int id_usuario,
    string nombre_usuario,
    string email_usuario,
    // Staff asignado (puede ser null)
    int? id_staff_asignado,
    string? nombre_staff_asignado,
    DateTime? fecha_creacion,
    DateTime? fecha_ultima_respuesta,
    // Hilo de mensajes ordenado por fecha
    IReadOnlyCollection<PqrsMensajeDto> mensajes
);

/// <summary>
/// Un mensaje dentro del hilo de un PQRS.
/// Remitente puede ser "USUARIO" o "STAFF".
/// </summary>
public record PqrsMensajeDto(
    int id_mensaje,
    string remitente,    // "USUARIO" o "STAFF"
    int id_remitente,
    string nombre_remitente,
    string mensaje,
    DateTime? fecha
);