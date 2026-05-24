using events_api.Data;
using events_api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<MetricsService>();

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

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("EventsUsers");

app.MapControllers();

app.Run();
