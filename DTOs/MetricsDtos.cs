namespace events_api.DTOs;

public record MetricValueDto<T>(T Valor);

public record WeeklySalesDto(
    int Anio,
    int Semana,
    decimal Total,
    int Cantidad);

public record AttendanceRateDto(
    int IdEvento,
    int TicketsEmitidos,
    int Asistentes,
    double TasaAsistencia);
