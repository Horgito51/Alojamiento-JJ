using Microsoft.AspNetCore.Mvc;
using Servicio.Hotel.API.Models.Requests.Internal;
using Servicio.Hotel.Business.DTOs.Alojamiento;
using Servicio.Hotel.Business.Interfaces.Alojamiento;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Servicio.Hotel.API.Controllers.Internal.Alojamiento
{
    [ApiController]
    [Route("api/v1/internal/sucursales")]
    public class SucursalController : ControllerBase
    {
        private readonly ISucursalService _service;

        public SucursalController(ISucursalService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SucursalDTO>>> GetAll()
            => Ok(await _service.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<ActionResult<SucursalDTO>> GetById(int id)
            => Ok(await _service.GetByIdAsync(id));

        [HttpPost]
        public async Task<ActionResult<SucursalDTO>> Create([FromBody] SucursalUpsertRequest request)
        {
            var created = await _service.CreateAsync(request.ToCreateDto());
            return CreatedAtAction(nameof(GetById), new { id = created.IdSucursal }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] SucursalUpsertRequest request)
        {
            await _service.UpdateAsync(request.ToUpdateDto(id));
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
