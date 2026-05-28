namespace events_api.DTOs;

public class LoginRequestDto
{
    public string Correo { get; set; } = null!;
    public string Contrasena { get; set; } = null!;
}

public class AuthResponseDto
{
    public string Token { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string Correo { get; set; } = null!;
    public string Rol { get; set; } = null!; // Admin, Taquilla o Acceso
    public DateTime Expiracion { get; set; }
}