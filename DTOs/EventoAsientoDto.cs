namespace events_api.DTOs;

public record EventoAsientoDto(
    int IdEventoAsiento,
    int IdAsiento,
    string CodigoAsiento,
    string Fila,
    int Numero,
    int IdZona,
    string Zona,
    string? ColorZona,
    decimal Precio,
    string? Estado);
