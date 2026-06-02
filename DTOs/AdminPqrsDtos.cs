namespace events_api.DTOs;

/// <summary>
/// Resumen de un PQRS para la lista del panel admin.
/// </summary>
public record AdminPqrsResumenDto(
    int id_pqrs,
    string asunto,
    string tipo,
    string? estado,
    int id_usuario,
    string usuario_nombre,
    string usuario_email,
    int? id_staff_asignado,
    string? staff_nombre_asignado,
    DateTime? fecha_creacion,
    DateTime? fecha_ultima_respuesta,
    int total_mensajes
);

/// <summary>
/// Detalle completo de un PQRS con el hilo de mensajes.
/// </summary>
public record AdminPqrsDetalleDto(
    int id_pqrs,
    string asunto,
    string tipo,
    string? estado,
    int id_usuario,
    string usuario_nombre,
    string usuario_email,
    int? id_staff_asignado,
    string? staff_nombre_asignado,
    DateTime? fecha_creacion,
    DateTime? fecha_ultima_respuesta,
    IReadOnlyCollection<AdminPqrsMensajeDto> mensajes
);

/// <summary>
/// Un mensaje dentro del hilo de un PQRS.
/// Remitente puede ser "USUARIO" o "STAFF".
/// </summary>
public record AdminPqrsMensajeDto(
    int id_mensaje,
    string remitente,
    int id_remitente,
    string nombre_remitente,
    string mensaje,
    DateTime? fecha
);
