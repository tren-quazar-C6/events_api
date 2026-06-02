namespace events_api.DTOs;

public record EventoDetalleDto(
    int IdEvento,
    string NombreEvento,
    string? Descripcion,
    DateTime FechaEvento,
    DateTime FechaInicioVentas,
    DateTime FechaFinVentas,
    DateTime? FechaCreacion,
    int CapacidadTotal,
    int IdTipoEvento,
    string TipoEvento,
    string RutaUrl,
    int AsientosDisponibles,
    int AsientosReservados,
    int AsientosVendidos,
    decimal? PrecioDesde);
