using Microsoft.AspNetCore.Mvc;
using Servicio.Hotel.Business.DTOs.Facturacion;
using Servicio.Hotel.Business.Interfaces.Facturacion;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Servicio.Hotel.API.Controllers.Internal.Pagos
{
    [ApiController]
    [Route("api/v1/internal/[controller]")]
    public class PagoController : ControllerBase
    {
        private readonly IPagoService _pagoService;

        public PagoController(IPagoService pagoService)
        {
            _pagoService = pagoService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PagoDTO>>> GetAll()
        {
            // Paginación por defecto
            var result = await _pagoService.GetByFacturaAsync(0, 1, 50); 
            return Ok(result.Items);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PagoDTO>> GetById(int id)
        {
            var result = await _pagoService.GetByIdAsync(id);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<PagoDTO>> Create([FromBody] PagoDTO dto)
        {
            var result = await _pagoService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.IdPago }, result);
        }

        [HttpPatch("{id}/estado")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string nuevoEstado)
        {
            await _pagoService.UpdateEstadoAsync(id, nuevoEstado, "Sistema");
            return NoContent();
        }
    }
}
