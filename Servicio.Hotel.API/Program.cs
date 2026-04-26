using Microsoft.AspNetCore.Mvc;
using Servicio.Hotel.API.Extensions;
using Servicio.Hotel.API.Filters;
using Servicio.Hotel.API.Middleware;
using Servicio.Hotel.API.Models.Settings;

var builder = WebApplication.CreateBuilder(args);

// 1. Cargar configuraciones desde appsettings.json
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (jwtSettings is not null &&
    (string.IsNullOrWhiteSpace(jwtSettings.Secret) || jwtSettings.Secret.Length < 32))
{
    throw new InvalidOperationException("La configuracion 'Jwt:Secret' debe tener al menos 32 caracteres.");
}

if (jwtSettings is null)
{
    throw new InvalidOperationException("La configuración 'Jwt' es obligatoria.");
}

if (allowedOrigins is null || allowedOrigins.Length == 0)
{
    throw new InvalidOperationException("La configuración 'Cors:AllowedOrigins' debe contener al menos un origen.");
}

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("La cadena de conexión 'DefaultConnection' es obligatoria.");
}

// Registrar JwtSettings como IOptions para inyección tipada
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

// 2. Registrar servicios de capa de datos (DbContext, repositorios)
builder.Services.AddDataAccessServices(connectionString);

// 3. Registrar autenticación JWT
builder.Services.AddJwtAuthentication(jwtSettings, builder.Environment);
builder.Services.AddCustomAuthorization();

// 4. Registrar Swagger / OpenAPI
builder.Services.AddSwaggerDocumentation();

// 5. Registrar CORS
builder.Services.AddCustomCors(allowedOrigins);

// 6. Registrar versionado de API
builder.Services.AddApiVersioningConfiguration();

// 7. Controladores con filtro de validación global
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
});

// 8. (Opcional) Configurar supresión de ModelState automática si usas [ApiController]
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = false; // El ValidationFilter ya maneja errores
});

var app = builder.Build();

// 9. Pipeline de middlewares
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowSpecificOrigins");
app.UseAuthentication();
app.UseMiddleware<AdminProfileAccessMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();
