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
    public class TipoHabitacionController : ControllerBase
    {
        private readonly ITipoHabitacionService _tipoHabitacionService;

        public TipoHabitacionController(ITipoHabitacionService tipoHabitacionService)
        {
            _tipoHabitacionService = tipoHabitacionService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TipoHabitacionDTO>>> GetAll()
        {
            var result = await _tipoHabitacionService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TipoHabitacionDTO>> GetById(int id)
        {
            var result = await _tipoHabitacionService.GetByIdAsync(id);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<TipoHabitacionDTO>> Create([FromBody] TipoHabitacionDTO dto)
        {
            var result = await _tipoHabitacionService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.IdTipoHabitacion }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TipoHabitacionDTO dto)
        {
            if (id != dto.IdTipoHabitacion) return BadRequest();
            await _tipoHabitacionService.UpdateAsync(dto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _tipoHabitacionService.DeleteAsync(id);
            return NoContent();
        }
    }
}
