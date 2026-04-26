using System.Security.Claims;
using System.Text.Json;
using Servicio.Hotel.API.Authorization;
using Servicio.Hotel.API.Models.Common;

namespace Servicio.Hotel.API.Middleware
{
    public class AdminProfileAccessMiddleware
    {
        private readonly RequestDelegate _next;

        public AdminProfileAccessMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!RequiresAdminProfile(context.Request.Path))
            {
                await _next(context);
                return;
            }

            if (context.User?.Identity?.IsAuthenticated != true)
            {
                await WriteErrorAsync(
                    context,
                    StatusCodes.Status401Unauthorized,
                    "No autorizado. Se requiere autenticacion.");
                return;
            }

            var isCliente = context.User.Claims.Any(claim =>
                claim.Type == ClaimTypes.Role &&
                string.Equals(claim.Value, AuthorizationPolicies.ClienteRole, StringComparison.OrdinalIgnoreCase));

            if (isCliente)
            {
                await WriteErrorAsync(
                    context,
                    StatusCodes.Status403Forbidden,
                    "Acceso denegado. El rol CLIENTE no puede ingresar al perfil administrativo.");
                return;
            }

            await _next(context);
        }

        private static bool RequiresAdminProfile(PathString path)
        {
            var value = path.Value ?? string.Empty;

            if (!value.Contains("/internal/", StringComparison.OrdinalIgnoreCase))
                return false;

            return !value.Contains("/internal/auth/", StringComparison.OrdinalIgnoreCase);
        }

        private static async Task WriteErrorAsync(HttpContext context, int statusCode, string message)
        {
            if (context.Response.HasStarted)
                return;

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var response = new ApiErrorResponse(
                message,
                statusCode,
                null,
                context.TraceIdentifier);

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
        }
    }
}
