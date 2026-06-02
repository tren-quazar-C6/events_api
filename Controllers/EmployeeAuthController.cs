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
        {
            return Unauthorized(ServiceResponse<EmployeeLoginResponse>.Fail("Credenciales inválidas."));
        }

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
        {
            return Unauthorized(ServiceResponse<EmployeeMeDto>.Fail("Token inválido."));
        }

        var employee = await _db.STAFF
            .AsNoTracking()
            .Where(s => s.id_staff == idStaff && s.activo == true)
            .Select(s => new
            {
                s.id_staff,
                s.nombre,
                s.email,
                RoleId = s.id_rol_staff,
                RoleName = s.id_rol_staffNavigation.nombre_rol
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (employee is null)
        {
            return NotFound(ServiceResponse<EmployeeMeDto>.Fail("Empleado no encontrado."));
        }

        var permissions = await _db.ROLE_PERMISSIONs
            .AsNoTracking()
            .Where(rp => rp.id_rol_staff == employee.RoleId && rp.active == true)
            .Select(rp => rp.id_permissionNavigation.code)
            .Distinct()
            .ToListAsync(cancellationToken);

        var dto = new EmployeeMeDto(
            employee.id_staff,
            employee.nombre,
            employee.email,
            employee.RoleName,
            permissions);

        return Ok(ServiceResponse<EmployeeMeDto>.Ok(dto));
    }
}
