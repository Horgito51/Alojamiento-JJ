using Microsoft.AspNetCore.Mvc;
using Servicio.Hotel.Business.Interfaces.Hospedaje;
using System.Threading.Tasks;

namespace Servicio.Hotel.API.Controllers.Internal.Hospedaje
{
    [ApiController]
    [Route("api/v1/internal/cargos-estadia")]
    public class CargoEstadiaController : ControllerBase
    {
        private readonly IEstadiaService _estadiaService;

        public CargoEstadiaController(IEstadiaService estadiaService)
        {
            _estadiaService = estadiaService;
        }

        [HttpPatch("{id}/anular")]
        public async Task<IActionResult> Anular(int id)
        {
            await _estadiaService.AnularCargoAsync(id, "Sistema");
            return NoContent();
        }
    }
}
