namespace events_api.DTOs;

public record ImagenEventoDto(
    int IdImagen,
    string RutaUrl,
    bool Principal);
