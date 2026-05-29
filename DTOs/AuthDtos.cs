using System.ComponentModel.DataAnnotations;

namespace events_api.DTOs;

/// <summary>
/// Request para hacer login.
/// Solo Staff puede autenticarse en la API admin.
/// </summary>
public record LoginRequest(
    [Required]
    [EmailAddress]
    string email,

    [Required]
    string password
);

/// <summary>
/// Respuesta del login exitoso.
/// Contiene el token JWT y datos básicos del staff autenticado.
/// </summary>
public record LoginResponse(
    string token,
    DateTime expira_en,
    int id_staff,
    string nombre,
    string email,
    string rol
);