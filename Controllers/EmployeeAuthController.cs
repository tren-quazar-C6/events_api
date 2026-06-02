using System.Security.Claims;
using events_api.Data;
using events_api.DTOs;
using events_api.Responses;
using events_api.Security;
using events_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace events_api.Controllers;

[ApiController]
[Route("api")]
public class EmployeeAuthController : ControllerBase
{
    private readonly EmployeeAuthService _authService;
    private readonly QuasarDbContext _db;

    public EmployeeAuthController(EmployeeAuthService authService, QuasarDbContext db)
    {
        _authService = authService;
        _db = db;
    }

    [HttpPost("employee/login")]
    [AllowAnonymous]
    public async Task<ActionResult<ServiceResponse<EmployeeLoginResponse>>> Login(
        [FromBody] EmployeeLoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        if (result is null)
            return Unauthorized(ServiceResponse<EmployeeLoginResponse>.Fail("Credenciales inválidas."));

        return Ok(ServiceResponse<EmployeeLoginResponse>.Ok(result));
    }

    [HttpGet("employees/me")]
    [Authorize]
    [RequirePermission("employees.me.read")]
    public async Task<ActionResult<ServiceResponse<EmployeeMeDto>>> GetMe(
        CancellationToken cancellationToken = default)
    {
        var idStaffClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(ClaimTypes.Name)
            ?? User.FindFirstValue("sub");

        if (!int.TryParse(idStaffClaim, out var idStaff))
            return Unauthorized(ServiceResponse<EmployeeMeDto>.Fail("Token inválido."));

        var employee = await _db.Staff
            .AsNoTracking()
            .Where(s => s.IdStaff == idStaff && s.Activo == true)
            .Select(s => new
            {
                s.IdStaff,
                s.Nombre,
                s.Email,
                RoleId = s.IdRolStaff,
                RoleName = s.IdRolStaffNavigation.NombreRol
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (employee is null)
            return NotFound(ServiceResponse<EmployeeMeDto>.Fail("Empleado no encontrado."));

        var permissions = await _db.RolePermissions
            .AsNoTracking()
            .Where(rp => rp.IdRolStaff == employee.RoleId && rp.Active == true)
            .Select(rp => rp.IdPermissionNavigation.Code)
            .Distinct()
            .ToListAsync(cancellationToken);

        var dto = new EmployeeMeDto(
            employee.IdStaff,
            employee.Nombre,
            employee.Email,
            employee.RoleName,
            permissions);

        return Ok(ServiceResponse<EmployeeMeDto>.Ok(dto));
    }
}
