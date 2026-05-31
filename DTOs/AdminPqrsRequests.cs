using System.ComponentModel.DataAnnotations;

namespace events_api.DTOs;

/// <summary>
/// Request para enviar un mensaje de respuesta a un PQRS.
/// El staff responde agregando un mensaje al hilo.
/// </summary>
public record ResponderPqrsRequest(
    [Required]
    [StringLength(2000, MinimumLength = 10)]
    string mensaje
);

/// <summary>
/// Request para cambiar el estado de un PQRS.
///
/// Transiciones válidas:
///   ABIERTO    → EN_PROCESO  (staff toma el caso)
///   EN_PROCESO → RESPONDIDO  (staff responde)
///   RESPONDIDO → CERRADO     (se cierra el caso)
///   RESPONDIDO → EN_PROCESO  (usuario replica, se reabre)
///   cualquiera → CERRADO     (cierre directo)
/// </summary>
public record UpdatePqrsStatusRequest(
    [Required]
    [RegularExpression("^(ABIERTO|EN_PROCESO|RESPONDIDO|CERRADO)$",
        ErrorMessage = "Estado debe ser ABIERTO, EN_PROCESO, RESPONDIDO o CERRADO")]
    string estado
);

/// <summary>
/// Request para asignar un PQRS a un staff específico.
/// </summary>
public record AsignarPqrsRequest(
    [Required] int id_staff
);