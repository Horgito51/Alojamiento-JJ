using Microsoft.AspNetCore.Mvc;
using Servicio.Hotel.Business.DTOs.Hospedaje;
using Servicio.Hotel.Business.Interfaces.Hospedaje;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Servicio.Hotel.API.Controllers.Internal.Hospedaje
{
    [ApiController]
    [Route("api/v1/internal/[controller]")]
    public class EstadiaController : ControllerBase
    {
        private readonly IEstadiaService _estadiaService;

        public EstadiaController(IEstadiaService estadiaService)
        {
            _estadiaService = estadiaService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EstadiaDTO>>> GetAll()
        {
            var result = await _estadiaService.GetByFiltroAsync(new EstadiaFiltroDTO(), 1, 50);
            return Ok(result.Items);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EstadiaDTO>> GetById(int id)
        {
            var result = await _estadiaService.GetByIdAsync(id);
            return Ok(result);
        }

        [HttpPost("checkin/{id_reserva}")]
        public async Task<ActionResult<int>> Checkin(int id_reserva)
        {
            var result = await _estadiaService.HacerCheckinAsync(id_reserva, "Sistema");
            return Ok(result);
        }

        [HttpPatch("{id}/checkout")]
        public async Task<IActionResult> Checkout(int id, [FromBody] CheckoutRequest request)
        {
            await _estadiaService.RegistrarCheckoutAsync(id, request.Observaciones, request.RequiereMantenimiento, "Sistema");
            return NoContent();
        }

        [HttpGet("{id}/cargos")]
        public async Task<ActionResult<IEnumerable<CargoEstadiaDTO>>> GetCargos(int id)
        {
            var result = await _estadiaService.GetCargosByEstadiaAsync(id, 1, 50);
            return Ok(result.Items);
        }

        [HttpPost("{id}/cargos")]
        public async Task<ActionResult<CargoEstadiaDTO>> AddCargo(int id, [FromBody] CargoEstadiaDTO dto)
        {
            var result = await _estadiaService.AddCargoAsync(id, dto);
            return Ok(result);
        }
    }

    public class CheckoutRequest
    {
        public string Observaciones { get; set; }
        public bool RequiereMantenimiento { get; set; }
    }
}
