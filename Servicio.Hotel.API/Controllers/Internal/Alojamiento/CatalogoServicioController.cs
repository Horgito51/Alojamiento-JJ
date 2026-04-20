using Microsoft.AspNetCore.Mvc;
using Servicio.Hotel.API.Models.Requests.Internal;
using Servicio.Hotel.Business.DTOs.Alojamiento;
using Servicio.Hotel.Business.Interfaces.Alojamiento;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Servicio.Hotel.API.Controllers.Internal.Alojamiento
{
    [ApiController]
    [Route("api/v1/internal/catalogo-servicios")]
    public class CatalogoServicioController : ControllerBase
    {
        private readonly ICatalogoServicioService _service;

        public CatalogoServicioController(ICatalogoServicioService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CatalogoServicioDTO>>> GetAll()
            => Ok(await _service.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<ActionResult<CatalogoServicioDTO>> GetById(int id)
            => Ok(await _service.GetByIdAsync(id));

        [HttpPost]
        public async Task<ActionResult<CatalogoServicioDTO>> Create([FromBody] CatalogoServicioUpsertRequest request)
        {
            var created = await _service.CreateAsync(request.ToDto());
            return CreatedAtAction(nameof(GetById), new { id = created.IdCatalogo }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CatalogoServicioUpsertRequest request)
        {
            await _service.UpdateAsync(request.ToDto(id));
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}
