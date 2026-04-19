using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Servicio.Hotel.Business.DTOs.Alojamiento;
using Servicio.Hotel.Business.Interfaces.Alojamiento;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Servicio.Hotel.API.Controllers.Internal.Alojamiento
{
    [ApiController]
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/internal/[controller]")]
    public class HabitacionController : ControllerBase
    {
        private readonly IHabitacionService _habitacionService;

        public HabitacionController(IHabitacionService habitacionService)
        {
            _habitacionService = habitacionService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<HabitacionDTO>>> GetAll()
        {
            var result = await _habitacionService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<HabitacionDTO>> GetById(int id)
        {
            var result = await _habitacionService.GetByIdAsync(id);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<HabitacionDTO>> Create([FromBody] HabitacionDTO dto)
        {
            var result = await _habitacionService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.IdHabitacion }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] HabitacionDTO dto)
        {
            if (id != dto.IdHabitacion) return BadRequest();
            await _habitacionService.UpdateAsync(dto);
            return NoContent();
        }

        [HttpPatch("{id}/estado")]
        public async Task<IActionResult> ChangeStatus(int id, [FromBody] string nuevoEstado)
        {
            await _habitacionService.UpdateEstadoAsync(id, nuevoEstado, "Sistema");
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _habitacionService.DeleteAsync(id);
            return NoContent();
        }
    }
}
