using System.ComponentModel.DataAnnotations;

namespace events_api.DTOs;

/// <summary>
/// Request para crear un empleado nuevo.
/// El password se hashea en el controller antes de guardarse.
/// </summary>
public record CreateEmpleadoRequest(
    [Required] int id_rol_staff,

    [Required]
    [StringLength(100, MinimumLength = 2)]
    string nombre,

    [Required]
    [EmailAddress]
    [StringLength(100)]
    string email,

    [Required]
    [StringLength(100, MinimumLength = 6)]
    string password
);

/// <summary>
/// Request para editar un empleado.
/// Todos los campos son opcionales — solo se actualiza lo que llega.
/// El password es opcional: si no viene, no se cambia.
/// </summary>
public record UpdateEmpleadoRequest(
    int? id_rol_staff,

    [StringLength(100, MinimumLength = 2)]
    string? nombre,

    [EmailAddress]
    [StringLength(100)]
    string? email,

    // Solo si quieren cambiar la contraseña
    [StringLength(100, MinimumLength = 6)]
    string? password
);