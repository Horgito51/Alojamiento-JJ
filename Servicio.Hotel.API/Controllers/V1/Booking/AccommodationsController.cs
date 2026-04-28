using Microsoft.AspNetCore.Mvc;
using Servicio.Hotel.Business.DTOs.Alojamiento;
using Servicio.Hotel.Business.DTOs.Valoraciones;
using Servicio.Hotel.Business.Interfaces.Alojamiento;
using Servicio.Hotel.Business.Interfaces.Valoraciones;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Servicio.Hotel.API.Controllers.V1.Booking
{
    [ApiController]
    [Route("api/v1/accommodations")]
    public class AccommodationsController : ControllerBase
    {
        private readonly IHabitacionService _habitacionService;
        private readonly IValoracionService _valoracionService;
        private readonly ITipoHabitacionService _tipoHabitacionService;

        public AccommodationsController(
            IHabitacionService habitacionService,
            IValoracionService valoracionService,
            ITipoHabitacionService tipoHabitacionService)
        {
            _habitacionService = habitacionService;
            _valoracionService = valoracionService;
            _tipoHabitacionService = tipoHabitacionService;
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<HabitacionDTO>>> Search([FromQuery] string query)
        {
            // HabitacionService ya maneja la lógica de precios vigentes
            var result = await _habitacionService.GetAllAsync(); 
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<HabitacionDTO>> GetById(int id)
        {
            var result = await _habitacionService.GetByIdAsync(id);
            return Ok(result);
        }

        [HttpGet("categories")]
        public async Task<ActionResult<IEnumerable<TipoHabitacionDTO>>> GetCategories()
        {
            var result = await _tipoHabitacionService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}/reviews")]
        public async Task<ActionResult<IEnumerable<ValoracionDTO>>> GetReviews(int id)
        {
            var filtro = new ValoracionFiltroDTO { IdHabitacion = id };
            var result = await _valoracionService.GetByFiltroAsync(filtro, 1, 10);
            return Ok(result.Items);
        }
    }
}
