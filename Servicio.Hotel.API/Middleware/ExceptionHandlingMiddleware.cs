using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Servicio.Hotel.API.Models.Common;

namespace Servicio.Hotel.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception, "Ocurrió una excepción no controlada: {Message}", exception.Message);

            var isDevelopment = context.RequestServices
                .GetService<IWebHostEnvironment>()?.IsDevelopment() ?? false;

            // Definir código de estado y mensaje base según el tipo de excepción
            var (statusCode, message, validationErrors) = exception switch
            {
                Servicio.Hotel.Business.Exceptions.ValidationException valEx => (HttpStatusCode.BadRequest, valEx.Message, valEx.Errors is null ? null : new Dictionary<string, string[]>(valEx.Errors)),
                Servicio.Hotel.Business.Exceptions.UnauthorizedBusinessException authEx => (HttpStatusCode.Unauthorized, authEx.Message, null),
                Servicio.Hotel.Business.Exceptions.NotFoundException nfEx => (HttpStatusCode.NotFound, nfEx.Message, null),
                Servicio.Hotel.Business.Exceptions.BusinessException bizEx => (HttpStatusCode.UnprocessableEntity, bizEx.Message, null),
                DbUpdateException dbEx => (HttpStatusCode.Conflict, ParseDbUpdateException(dbEx), null),
                KeyNotFoundException => (HttpStatusCode.NotFound, "El recurso solicitado no fue encontrado.", null),
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "No autorizado.", null),
                ArgumentException argEx => (HttpStatusCode.BadRequest, isDevelopment ? argEx.Message : "Solicitud inválida.", null),
                InvalidOperationException => (HttpStatusCode.Conflict, "Conflicto con el estado actual.", null),
                _ => (HttpStatusCode.InternalServerError,
                      isDevelopment
                          ? $"{exception.GetType().Name}: {exception.Message}{(exception.InnerException != null ? $" | Inner: {exception.InnerException.Message}" : "")}"
                          : "Ha ocurrido un error interno en el servidor.",
                      null)
            };

            // Construir respuesta estandarizada con ApiErrorResponse
            var errorResponse = new ApiErrorResponse(
                message: message,
                statusCode: (int)statusCode,
                errors: validationErrors,
                traceId: context.TraceIdentifier
            );

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var json = JsonSerializer.Serialize(errorResponse, jsonOptions);
            await context.Response.WriteAsync(json);
        }
        private static string ParseDbUpdateException(DbUpdateException ex)
        {
            var inner = ex.InnerException?.Message ?? ex.Message;

            if (inner.Contains("UQ_") || inner.Contains("UNIQUE KEY") || inner.Contains("duplicate key"))
            {
                // Extraer el valor duplicado si está disponible
                if (inner.Contains("duplicate key value is"))
                {
                    var start = inner.IndexOf('(');
                    var end = inner.LastIndexOf(')');
                    var valor = start >= 0 && end > start ? inner.Substring(start + 1, end - start - 1) : "";
                    return $"Ya existe un registro con ese valor: {valor}. Verifique los campos únicos.";
                }
                return "Ya existe un registro con los mismos datos únicos. Verifique los campos duplicados.";
            }

            if (inner.Contains("CHECK constraint"))
            {
                if (inner.Contains("CHK_CATALOGO_TIPO"))
                    return "El tipo de catálogo es inválido. Use 'AME' para amenidades o 'SRV' para servicios.";
                if (inner.Contains("CHK_HABITACION_ESTADO"))
                    return "El estado de habitación es inválido. Use: DIS, OCU, MNT, FDS o INA.";
                if (inner.Contains("CHK_RESERVAS_ESTADO"))
                    return "El estado de reserva es inválido. Use: PEN, CON, CAN, EXP, FIN o EMI.";
                if (inner.Contains("CHK_TARIFA_PRECIO"))
                    return "El precio por noche debe ser mayor a cero.";
                return $"Violación de restricción de datos: {inner}";
            }

            if (inner.Contains("NULL") || inner.Contains("cannot be null"))
                return "Faltan campos obligatorios en el registro.";

            if (inner.Contains("FOREIGN KEY") || inner.Contains("FK_"))
                return "El registro referencia un elemento que no existe. Verifique los IDs relacionados.";

            return $"Error al guardar los datos: {inner}";
        }
    }
}
