using System.Text;
using events_api.Data;
using events_api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// [NUEVO] Configurar CORS para tus subdominios frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("Events", policy =>
    {
        policy
            .WithOrigins(
                "https://access.quasar.andrescortes.dev/",
                "https://tickets.quasar.andrescortes.dev/",
                "https://andrescortes.dev",
                "http://localhost:3000",
                "https://quasar.andrescortes.dev",
                "http://127.0.0.1:8000",
                "http://localhost:8000",
                "http://quasar.local")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
// Configurar JWT desde appsettings.json
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(secretKey),
            ClockSkew = TimeSpan.Zero // Elimina el tiempo de gracia de 5 min para mayor seguridad
        };
    });

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<MetricsService>();
builder.Services.AddScoped<AuthService>();

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

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("Events");

// ¡El orden de estos dos middlewares es estricto y obligatorio!
app.UseAuthentication(); 
app.UseAuthorization(); 
app.MapControllers();

app.Run();
