using Microsoft.AspNetCore.Mvc;
using Servicio.Hotel.Business.DTOs.Reservas;
using Servicio.Hotel.Business.Interfaces.Reservas;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Servicio.Hotel.API.Controllers.Internal.Reservas
{
    [ApiController]
    [Route("api/v1/internal/[controller]")]
    public class ReservaController : ControllerBase
    {
        private readonly IReservaService _reservaService;

        public ReservaController(IReservaService reservaService)
        {
            _reservaService = reservaService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReservaDTO>>> GetAll()
        {
            var pagedResult = await _reservaService.GetByFiltroAsync(new ReservaFiltroDTO(), 1, 50);
            return Ok(pagedResult.Items);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ReservaDTO>> GetById(int id)
        {
            var result = await _reservaService.GetByIdAsync(id);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<ReservaDTO>> Create([FromBody] ReservaDTO dto)
        {
            var result = await _reservaService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.IdReserva }, result);
        }

        [HttpPatch("{id}/confirmar")]
        public async Task<IActionResult> Confirm(int id)
        {
            await _reservaService.ConfirmarAsync(id, "Sistema");
            return NoContent();
        }

        [HttpPatch("{id}/cancelar")]
        public async Task<IActionResult> Cancel(int id, [FromBody] string motivo)
        {
            await _reservaService.CancelarAsync(id, motivo, "Sistema");
            return NoContent();
        }
    }
}
