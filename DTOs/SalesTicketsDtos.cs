using System.ComponentModel.DataAnnotations;

namespace events_api.DTOs;

public record CreateSaleRequest(
    [Required] int id_usuario,
    [Required] int id_staff,
    [Required] string tipo_venta,
    string? metodo_pago,
    [Required] IReadOnlyCollection<int> id_evento_asientos);

public record CreateSaleResponse(
    int id_venta,
    decimal total,
    string estado_pago,
    IReadOnlyCollection<int> id_evento_asientos);

public record GenerateTicketsRequest(
    [Required] int id_venta);

public record GenerateTicketsResponse(
    int id_venta,
    int tickets_generados,
    IReadOnlyCollection<string> codigos_unicos);
