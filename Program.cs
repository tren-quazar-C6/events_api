using events_api.Data;
using events_api.Security;
using events_api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<MetricsService>();
builder.Services.AddScoped<ScanValidationService>();
builder.Services.AddScoped<EmployeeAuthService>();
builder.Services.AddScoped<SalesService>();

var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtSecret = jwtSection["Secret"] ?? throw new InvalidOperationException("Missing Jwt:Secret in appsettings.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("EventsUsers", policy =>
    {
        policy
            .WithOrigins(
                "https://quasar.andrescortes.dev",
                "http://127.0.0.1:8000",
                "http://localhost:8000",
                "http://quasar.local")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var connectionString =
    builder.Configuration.GetConnectionString("Quasar")
    ?? builder.Configuration.GetConnectionString("MySQL");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Configure ConnectionStrings:Quasar or ConnectionStrings:MySQL before starting the API.");
}

builder.Services.AddDbContext<QuasarDbContext>(options =>
{
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 0)));
});

var app = builder.Build();

app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.RoutePrefix = "swagger";
});

app.MapGet("/", () => Results.Redirect("/swagger"));

app.UseHttpsRedirection();
app.UseCors("EventsUsers");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
