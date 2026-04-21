using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Servicio.Hotel.API.Models.Requests.Internal;
using Servicio.Hotel.Business.DTOs.Facturacion;
using Servicio.Hotel.Business.Interfaces.Facturacion;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Servicio.Hotel.API.Controllers.Internal.Facturacion
{
    [ApiController]
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/internal/facturas")]
    public class FacturaController : ControllerBase
    {
        private readonly IFacturaService _facturaService;

        public FacturaController(IFacturaService facturaService)
        {
            _facturaService = facturaService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FacturaDTO>>> GetAll()
        {
            var result = await _facturaService.GetByFiltroAsync(new FacturaFiltroDTO(), 1, 50);
            return Ok(result.Items);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FacturaDTO>> GetById(int id)
        {
            var result = await _facturaService.GetByIdAsync(id);
            return Ok(result);
        }

        [HttpPost("generar-reserva/{id}")]
        public async Task<ActionResult<int>> GenerarReserva(int id)
        {
            var result = await _facturaService.GenerarFacturaReservaAsync(id, "Sistema");
            return Ok(result);
        }

        [HttpPost("generar-final/{id}")]
        public async Task<ActionResult<int>> GenerarFinal(int id)
        {
            var result = await _facturaService.GenerarFacturaFinalAsync(id, "Sistema");
            return Ok(result);
        }

        [HttpPatch("{id}/anular")]
        public async Task<IActionResult> Anular(int id, [FromBody] AnularFacturaRequest request)
        {
            await _facturaService.AnularAsync(id, request.Motivo, "Sistema");
            return NoContent();
        }
    }
}
