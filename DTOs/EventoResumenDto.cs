namespace events_api.DTOs;

public record EventoResumenDto(
    int IdEvento,
    string NombreEvento,
    string? Descripcion,
    DateTime FechaEvento,
    DateTime FechaInicioVentas,
    DateTime FechaFinVentas,
    int CapacidadTotal,
    int IdTipoEvento,
    string TipoEvento,
    string? ImagenPrincipal,
    int AsientosDisponibles);
