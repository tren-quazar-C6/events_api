namespace events_api.DTOs;

public record PqrsCreateRequest(
    int id_usuario,
    string tipo,
    string asunto,
    string mensaje);

public record PqrsUpdateRequest(
    int? asignado_staff,
    string? estado,
    string? mensaje,
    int? id_staff);

public record PqrsMensajeDto(
    int id_mensaje,
    string remitente,
    int id_remitente,
    string mensaje,
    DateTime? fecha);

public record PqrsResumenDto(
    int id_pqrs,
    int id_usuario,
    int? asignado_staff,
    string tipo,
    string asunto,
    string? estado,
    DateTime? fecha_creacion,
    DateTime? fecha_ultima_respuesta,
    string usuario_nombre,
    string? staff_nombre,
    int total_mensajes);

public record PqrsDetalleDto(
    int id_pqrs,
    int id_usuario,
    int? asignado_staff,
    string tipo,
    string asunto,
    string? estado,
    DateTime? fecha_creacion,
    DateTime? fecha_ultima_respuesta,
    string usuario_nombre,
    string? staff_nombre,
    IReadOnlyCollection<PqrsMensajeDto> mensajes);
