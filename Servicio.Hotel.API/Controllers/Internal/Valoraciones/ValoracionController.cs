using Microsoft.AspNetCore.Mvc;
using Servicio.Hotel.Business.DTOs.Valoraciones;
using Servicio.Hotel.Business.Interfaces.Valoraciones;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Servicio.Hotel.API.Controllers.Internal.Valoraciones
{
    [ApiController]
    [Route("api/v1/internal/[controller]")]
    public class ValoracionController : ControllerBase
    {
        private readonly IValoracionService _valoracionService;

        public ValoracionController(IValoracionService valoracionService)
        {
            _valoracionService = valoracionService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ValoracionDTO>>> GetAll()
        {
            var result = await _valoracionService.GetByFiltroAsync(new ValoracionFiltroDTO(), 1, 50);
            return Ok(result.Items);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ValoracionDTO>> GetById(int id)
        {
            var result = await _valoracionService.GetByIdAsync(id);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<ValoracionDTO>> Create([FromBody] ValoracionDTO dto)
        {
            var result = await _valoracionService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.IdValoracion }, result);
        }

        [HttpPatch("{id}/moderar")]
        public async Task<IActionResult> Moderar(int id, [FromBody] ModeracionRequest request)
        {
            await _valoracionService.ModerarAsync(id, request.NuevoEstado, request.Motivo, "Sistema");
            return NoContent();
        }

        [HttpPatch("{id}/responder")]
        public async Task<IActionResult> Responder(int id, [FromBody] string respuesta)
        {
            await _valoracionService.ResponderAsync(id, respuesta, "Sistema");
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _valoracionService.DeleteAsync(id);
            return NoContent();
        }
    }

    public class ModeracionRequest
    {
        public string NuevoEstado { get; set; }
        public string Motivo { get; set; }
    }
}
