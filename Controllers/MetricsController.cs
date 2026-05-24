using events_api.DTOs;
using events_api.Services;
using Microsoft.AspNetCore.Mvc;

namespace events_api.Controllers;

[ApiController]
[Route("api/metrics")]
public class MetricsController : ControllerBase
{
    private readonly MetricsService _metricsService;

    public MetricsController(MetricsService metricsService)
    {
        _metricsService = metricsService;
    }

    [HttpGet("revenue-total")]
    public async Task<ActionResult<MetricValueDto<decimal>>> GetRevenueTotal(
        [FromQuery] DateTime desde,
        [FromQuery] DateTime hasta,
        CancellationToken cancellationToken = default)
    {
        if (hasta < desde)
        {
            return BadRequest("La fecha 'hasta' debe ser mayor o igual a la fecha 'desde'.");
        }

        var total = await _metricsService.GetRevenueTotalAsync(desde, hasta, cancellationToken);

        return Ok(new MetricValueDto<decimal>(total));
    }

    [HttpGet("tickets-sold")]
    public async Task<ActionResult<MetricValueDto<int>>> GetTicketsSold(
        [FromQuery] DateTime desde,
        [FromQuery] DateTime hasta,
        CancellationToken cancellationToken = default)
    {
        if (hasta < desde)
        {
            return BadRequest("La fecha 'hasta' debe ser mayor o igual a la fecha 'desde'.");
        }

        var total = await _metricsService.GetTicketsSoldAsync(desde, hasta, cancellationToken);

        return Ok(new MetricValueDto<int>(total));
    }

    [HttpGet("weekly-sales")]
    public async Task<ActionResult<IReadOnlyCollection<WeeklySalesDto>>> GetWeeklySales(
        [FromQuery] DateTime desde,
        [FromQuery] DateTime hasta,
        CancellationToken cancellationToken = default)
    {
        if (hasta < desde)
        {
            return BadRequest("La fecha 'hasta' debe ser mayor o igual a la fecha 'desde'.");
        }

        var ventas = await _metricsService.GetWeeklySalesAsync(desde, hasta, cancellationToken);

        return Ok(ventas);
    }

    [HttpGet("eventos/{idEvento:int}/attendance-rate")]
    public async Task<ActionResult<AttendanceRateDto>> GetAttendanceRate(
        int idEvento,
        CancellationToken cancellationToken = default)
    {
        var asistencia = await _metricsService.GetAttendanceRateAsync(idEvento, cancellationToken);

        return Ok(asistencia);
    }
}
