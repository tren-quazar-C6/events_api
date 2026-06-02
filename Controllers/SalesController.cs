using events_api.DTOs;
using events_api.Responses;
using events_api.Security;
using events_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace events_api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class SalesController : ControllerBase
{
    private readonly SalesService _salesService;

    public SalesController(SalesService salesService)
    {
        _salesService = salesService;
    }

    [HttpPost("sales")]
    [RequirePermission("sales.create")]
    public async Task<ActionResult<ServiceResponse<CreateSaleResponse>>> CreateSale(
        [FromBody] CreateSaleRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var sale = await _salesService.CreateSaleAsync(request, cancellationToken);
            return Ok(ServiceResponse<CreateSaleResponse>.Ok(sale));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ServiceResponse<CreateSaleResponse>.Fail(ex.Message));
        }
    }

    [HttpPost("tickets/generate")]
    [RequirePermission("tickets.generate")]
    public async Task<ActionResult<ServiceResponse<GenerateTicketsResponse>>> GenerateTickets(
        [FromBody] GenerateTicketsRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _salesService.GenerateTicketsAsync(request, cancellationToken);
            return Ok(ServiceResponse<GenerateTicketsResponse>.Ok(result));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ServiceResponse<GenerateTicketsResponse>.Fail(ex.Message));
        }
    }
}
