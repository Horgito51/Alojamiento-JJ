using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Servicio.Hotel.API.Models.Requests.Internal;
using Servicio.Hotel.Business.DTOs.Alojamiento;
using Servicio.Hotel.Business.Interfaces.Alojamiento;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Servicio.Hotel.API.Controllers.Internal.Alojamiento
{
    [ApiController]
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/internal/habitaciones")]
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
        public async Task<ActionResult<HabitacionDTO>> Create([FromBody] HabitacionCreateRequest request)
        {
            var dto = request.ToDto();
            var result = await _habitacionService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.IdHabitacion }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] HabitacionUpdateRequest request)
        {
            var dto = request.ToDto(id);
            await _habitacionService.UpdateAsync(dto);
            return NoContent();
        }

        [HttpPatch("{id}/estado")]
        public async Task<IActionResult> ChangeStatus(int id, [FromBody] HabitacionEstadoRequest request)
        {
            await _habitacionService.UpdateEstadoAsync(id, request.NuevoEstado, "Sistema");
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
