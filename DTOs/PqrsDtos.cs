namespace events_api.DTOs;

public record PublicPqrsCreateRequest(
    int id_usuario,
    string tipo,
    string asunto,
    string mensaje);

public record PublicPqrsUpdateRequest(
    int? asignado_staff,
    string? estado,
    string? mensaje,
    int? id_staff);

public record PublicPqrsMensajeDto(
    int id_mensaje,
    string remitente,
    int id_remitente,
    string mensaje,
    DateTime? fecha);

public record PublicPqrsResumenDto(
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

public record PublicPqrsDetalleDto(
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
    IReadOnlyCollection<PublicPqrsMensajeDto> mensajes);
