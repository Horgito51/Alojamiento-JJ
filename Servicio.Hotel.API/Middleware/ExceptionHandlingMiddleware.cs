// Middleware/ExceptionHandlingMiddleware.cs
using System.Net;
using System.Text.Json;
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

            // Definir código de estado y mensaje base según el tipo de excepción
            var (statusCode, message, validationErrors) = exception switch
            {
                Servicio.Hotel.Business.Exceptions.ValidationException valEx => (HttpStatusCode.BadRequest, valEx.Message, valEx.Errors),
                Servicio.Hotel.Business.Exceptions.UnauthorizedBusinessException authEx => (HttpStatusCode.Unauthorized, authEx.Message, null),
                Servicio.Hotel.Business.Exceptions.NotFoundException nfEx => (HttpStatusCode.NotFound, nfEx.Message, null),
                Servicio.Hotel.Business.Exceptions.BusinessException bizEx => (HttpStatusCode.UnprocessableEntity, bizEx.Message, null),
                KeyNotFoundException => (HttpStatusCode.NotFound, "El recurso solicitado no fue encontrado.", null),
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "No autorizado.", null),
                ArgumentException => (HttpStatusCode.BadRequest, "Solicitud inválida.", null),
                InvalidOperationException => (HttpStatusCode.Conflict, "Conflicto con el estado actual.", null),
                _ => (HttpStatusCode.InternalServerError, "Ha ocurrido un error interno en el servidor.", null)
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
    }
}