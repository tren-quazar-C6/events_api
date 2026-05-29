using System.ComponentModel.DataAnnotations;

namespace events_api.DTOs;

public record TicketLookupDto(
    int id_ticket,
    string codigo_unico,
    string qr_token,
    string estado_ticket,
    int id_evento,
    string nombre_evento,
    DateTime fecha_evento,
    DateTime fecha_inicio_ventas,
    DateTime fecha_fin_ventas,
    decimal precio_pagado);

public record TodayEventDto(
    int id_evento,
    string nombre_evento,
    DateTime fecha_evento,
    bool publicado,
    bool activo);

public record ScanTicketRequest(
    [Required] string qr_token,
    [Required] int id_empleado,
    int? id_evento,
    string? dispositivo,
    DateTime? fecha_scan);

public record ScanTicketResponse(
    string resultado,
    string mensaje,
    int? id_ticket,
    int? id_scan,
    string? tipo_alerta);

public record CreateScanAlertRequest(
    int? id_scan,
    int? id_ticket,
    int? id_staff,
    [Required] string tipo_alerta,
    string? detalle,
    string? qr_token,
    string? dispositivo,
    string? payload_json);

public record ScanAlertDto(
    int id_scan_alert,
    string tipo_alerta,
    DateTime fecha_alerta,
    int? id_scan,
    int? id_ticket,
    int? id_staff,
    string? detalle,
    string? qr_token,
    string? dispositivo);
