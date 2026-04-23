using Microsoft.AspNetCore.Mvc;
using Servicio.Hotel.API.Models.Responses.Public;
using Servicio.Hotel.Business.Interfaces.Alojamiento;

namespace Servicio.Hotel.API.Controllers.V1.Booking
{
    [ApiController]
    [Route("api/v1/public/habitaciones")]
    public class HabitacionesPublicController : ControllerBase
    {
        private readonly IHabitacionService _habitacionService;
        private readonly ISucursalService _sucursalService;
        private readonly ITipoHabitacionService _tipoHabitacionService;

        public HabitacionesPublicController(IHabitacionService habitacionService, ISucursalService sucursalService, ITipoHabitacionService tipoHabitacionService)
        {
            _habitacionService = habitacionService;
            _sucursalService = sucursalService;
            _tipoHabitacionService = tipoHabitacionService;
        }

        [HttpGet("{habitacionGuid:guid}")]
        public async Task<ActionResult<HabitacionPublicDto>> GetByGuid(Guid habitacionGuid)
        {
            var habitacion = await _habitacionService.GetByGuidAsync(habitacionGuid);
            var sucursal = await _sucursalService.GetByIdAsync(habitacion.IdSucursal);
            var tipo = await _tipoHabitacionService.GetByIdAsync(habitacion.IdTipoHabitacion);

            var dto = new HabitacionPublicDto
            {
                HabitacionGuid = habitacion.HabitacionGuid,
                NumeroHabitacion = habitacion.NumeroHabitacion,
                Piso = habitacion.Piso,
                CapacidadHabitacion = habitacion.CapacidadHabitacion,
                PrecioBase = habitacion.PrecioBase,
                DescripcionHabitacion = habitacion.DescripcionHabitacion,
                EstadoHabitacion = habitacion.EstadoHabitacion,
                SucursalGuid = sucursal.SucursalGuid,
                TipoHabitacionGuid = tipo.TipoHabitacionGuid,
                TipoHabitacionSlug = tipo.Slug
            };

            return Ok(dto);
        }
    }
}
