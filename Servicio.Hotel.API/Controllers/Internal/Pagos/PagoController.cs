using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Servicio.Hotel.API.Models.Requests.Internal;
using Servicio.Hotel.Business.DTOs.Facturacion;
using Servicio.Hotel.Business.Interfaces.Facturacion;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Servicio.Hotel.API.Controllers.Internal.Pagos
{
    [ApiController]
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

        [HttpPatch("{id}/estado")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] PagoEstadoRequest request)
        {
            await _pagoService.UpdateEstadoAsync(id, request.NuevoEstado, "Sistema");
            return NoContent();
        }
    }
}
