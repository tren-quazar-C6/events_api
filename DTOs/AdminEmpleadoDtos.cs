namespace events_api.DTOs;

/// <summary>
/// Detalle completo de un empleado.
/// NUNCA incluye password_hash.
/// </summary>
public record EmpleadoDetalleDto(
    int id_staff,
    string nombre,
    string email,
    int id_rol_staff,
    string nombre_rol,
    bool activo,
    DateTime? fecha_registro
);

/// <summary>
/// Versión resumida para listas.
/// </summary>
public record EmpleadoResumenDto(
    int id_staff,
    string nombre,
    string email,
    string nombre_rol,
    bool activo
);

/// <summary>
/// Información de un rol de staff.
/// </summary>
public record RolStaffDto(
    int id_rol_staff,
    string nombre_rol,
    bool activo
);