using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Servicio.Hotel.API.Models.Requests.Public;
using Servicio.Hotel.API.Models.Responses.Public;
using Servicio.Hotel.Business.Exceptions;
using Servicio.Hotel.Business.Interfaces.Facturacion;
using Servicio.Hotel.Business.Interfaces.Reservas;

namespace Servicio.Hotel.API.Controllers.V1.Booking
{
    [ApiController]
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/public/pagos")]
    public class PagosPublicController : ControllerBase
    {
        private readonly IPagoService _pagoService;
        private readonly IReservaService _reservaService;

        public PagosPublicController(IPagoService pagoService, IReservaService reservaService)
        {
            _pagoService = pagoService;
            _reservaService = reservaService;
        }

        [HttpPost("simular")]
        public async Task<ActionResult<PagoSimuladoPublicDto>> Simular([FromBody] PublicPagoSimularRequest request)
        {
            request.ValidateNoIds();

            if (request.ReservaGuid == Guid.Empty)
                throw new ValidationException("PAG-PUB-001", "reservaGuid es obligatorio.");

            var reserva = await _reservaService.GetByGuidAsync(request.ReservaGuid);
            var usuario = User.Identity?.Name ?? "CLIENTE_PUBLICO";

            if (!string.Equals(reserva.EstadoReserva, "CON", StringComparison.OrdinalIgnoreCase))
                await _reservaService.ConfirmarAsync(reserva.IdReserva, usuario, HttpContext.RequestAborted);

            var result = await _pagoService.SimularPagoAsync(
                reserva.IdReserva,
                request.Monto,
                usuario,
                request.TokenPago,
                request.Referencia,
                HttpContext.RequestAborted);

            return Ok(new PagoSimuladoPublicDto
            {
                ReservaGuid = reserva.GuidReserva,
                CodigoReserva = result.CodigoReserva,
                Monto = result.Monto,
                EstadoPago = result.EstadoPago,
                EstadoReserva = result.EstadoReserva,
                TransaccionExterna = result.TransaccionExterna,
                CodigoAutorizacion = result.CodigoAutorizacion,
                Mensaje = result.Mensaje,
                FechaPagoUtc = result.FechaPagoUtc
            });
        }
    }
}
