using System.ComponentModel.DataAnnotations;

namespace events_api.DTOs;

public record EmployeeLoginRequest(
    [Required] string email,
    [Required] string password);

public record EmployeeLoginResponse(
    string token,
    DateTime expires_at,
    int id_staff,
    string nombre,
    string role,
    IReadOnlyCollection<string> permissions);

public record EmployeeMeDto(
    int id_staff,
    string nombre,
    string email,
    string role,
    IReadOnlyCollection<string> permissions);
