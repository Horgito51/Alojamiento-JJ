using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Servicio.Hotel.API.Models.Settings;
using Servicio.Hotel.Business.Exceptions;
using Servicio.Hotel.Business.Interfaces.Facturacion;

namespace Servicio.Hotel.API.Services
{
    public class HttpPaymentGateway : IPaymentGateway
    {
        private readonly HttpClient _httpClient;
        private readonly PaymentGatewaySettings _settings;

        public HttpPaymentGateway(HttpClient httpClient, IOptions<PaymentGatewaySettings> options)
        {
            _httpClient = httpClient;
            _settings = options.Value;
        }

        public async Task<PaymentGatewayResult> ProcesarPagoAsync(PaymentGatewayRequest request, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(_settings.BaseUrl))
                throw new ValidationException("PAG-GW-001", "La pasarela de pagos no esta configurada. Defina PaymentGateway:BaseUrl.");

            if (string.IsNullOrWhiteSpace(request.TokenPago))
                throw new ValidationException("PAG-GW-002", "El token de pago emitido por la pasarela es obligatorio.");

            var url = new Uri(new Uri(_settings.BaseUrl.TrimEnd('/') + "/"), _settings.ChargePath.TrimStart('/'));
            using var message = new HttpRequestMessage(HttpMethod.Post, url);

            if (!string.IsNullOrWhiteSpace(_settings.ApiKey))
                message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);

            var body = new
            {
                amount = request.Monto,
                currency = request.Moneda,
                paymentToken = request.TokenPago,
                reference = request.Referencia,
                reservationId = request.IdReserva,
                invoiceId = request.IdFactura,
                reservationCode = request.CodigoReserva
            };

            message.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            using var response = await _httpClient.SendAsync(message, ct);
            var raw = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                return new PaymentGatewayResult
                {
                    Aprobado = false,
                    Estado = "REC",
                    Mensaje = $"Pago rechazado por la pasarela ({(int)response.StatusCode}).",
                    RespuestaRaw = raw
                };
            }

            using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(raw) ? "{}" : raw);
            var root = document.RootElement;
            var status = ReadString(root, "status", "estado", "state").ToUpperInvariant();
            var approved = status is "APPROVED" or "APROBADO" or "APR" or "PAID" or "PAGADO" or "OK" or "SUCCESS";

            return new PaymentGatewayResult
            {
                Aprobado = approved,
                Estado = approved ? "APR" : "REC",
                TransaccionExterna = ReadString(root, "transactionId", "transaccionExterna", "id"),
                CodigoAutorizacion = ReadString(root, "authorizationCode", "codigoAutorizacion", "authCode"),
                Mensaje = ReadString(root, "message", "mensaje") is { Length: > 0 } msg
                    ? msg
                    : approved ? "Pago aprobado por la pasarela." : "Pago rechazado por la pasarela.",
                RespuestaRaw = raw
            };
        }

        private static string ReadString(JsonElement root, params string[] names)
        {
            foreach (var name in names)
            {
                if (root.ValueKind == JsonValueKind.Object &&
                    root.TryGetProperty(name, out var value) &&
                    value.ValueKind != JsonValueKind.Null &&
                    value.ValueKind != JsonValueKind.Undefined)
                {
                    return value.ToString() ?? string.Empty;
                }
            }

            return string.Empty;
        }
    }
}
