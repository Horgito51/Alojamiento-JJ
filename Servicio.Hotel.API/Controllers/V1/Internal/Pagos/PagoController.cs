using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Servicio.Hotel.API.Models.Requests.Internal;
using Servicio.Hotel.Business.DTOs.Facturacion;
using Servicio.Hotel.Business.Interfaces.Facturacion;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Servicio.Hotel.API.Controllers.V1.Internal.Pagos
{
    [ApiController]
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/internal/pagos")]
    public class PagoController : ControllerBase
    {
        private readonly IPagoService _pagoService;

        public PagoController(IPagoService pagoService)
        {
            _pagoService = pagoService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PagoDTO>>> GetAll([FromQuery] int facturaId = 0, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            var result = await _pagoService.GetByFacturaAsync(facturaId, page, pageSize);
            return Ok(result.Items);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PagoDTO>> GetById(int id)
        {
            var result = await _pagoService.GetByIdAsync(id);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<PagoDTO>> Create([FromBody] PagoCreateRequest request)
        {
            var dto = request.ToDto();
            var result = await _pagoService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.IdPago }, result);
        }

        [HttpPost("/api/v{version:apiVersion}/pagos/simular")]
        [HttpPost("/api/v{version:apiVersion}/public/pagos/simular")]
        public async Task<ActionResult<PagoSimuladoDTO>> Simular([FromBody] PagoSimularRequest request)
        {
            var usuario = User.Identity?.Name ?? "Cliente";
            var result = await _pagoService.SimularPagoAsync(request.IdReserva, request.Monto, usuario);
            return Ok(result);
        }

        [HttpPatch("{id}/estado")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] PagoEstadoRequest request)
        {
            await _pagoService.UpdateEstadoAsync(id, request.NuevoEstado, "Sistema");
            return NoContent();
        }
    }
}
