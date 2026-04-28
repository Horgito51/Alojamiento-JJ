using Microsoft.AspNetCore.Mvc;
using Servicio.Hotel.API.Models.Responses.Public;
using Servicio.Hotel.Business.DTOs.Alojamiento;
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

        public HabitacionesPublicController(
            IHabitacionService habitacionService, 
            ISucursalService sucursalService, 
            ITipoHabitacionService tipoHabitacionService)
        {
            _habitacionService = habitacionService;
            _sucursalService = sucursalService;
            _tipoHabitacionService = tipoHabitacionService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<HabitacionPublicDto>>> GetAll(
            [FromQuery] DateTime? fechaInicio = null, 
            [FromQuery] DateTime? fechaFin = null,
            [FromQuery] int sucursalId = 1)
        {
            IEnumerable<HabitacionDTO> habitaciones;

            if (fechaInicio.HasValue && fechaFin.HasValue)
            {
                habitaciones = await _habitacionService.GetDisponiblesAsync(sucursalId, fechaInicio.Value, fechaFin.Value);
            }
            else
            {
                habitaciones = await _habitacionService.GetBySucursalAsync(sucursalId);
            }

            var sucursales = await _sucursalService.GetAllAsync();
            var tipos = await _tipoHabitacionService.GetAllAsync();
            
            var result = habitaciones
                .Where(h => h.EstadoHabitacion == "DIS") // Solo habitaciones disponibles
                .Select(h => {
                    var sucursal = sucursales.FirstOrDefault(s => s.IdSucursal == h.IdSucursal);
                    var tipo = tipos.FirstOrDefault(t => t.IdTipoHabitacion == h.IdTipoHabitacion);
                    
                    return new HabitacionPublicDto
                    {
                        IdHabitacion = h.IdHabitacion,
                        HabitacionGuid = h.HabitacionGuid,
                        NumeroHabitacion = h.NumeroHabitacion,
                        Piso = h.Piso,
                        CapacidadHabitacion = h.CapacidadHabitacion,
                        PrecioBase = h.PrecioBase, // Ya viene calculado por HabitacionService
                        DescripcionHabitacion = h.DescripcionHabitacion,
                        EstadoHabitacion = h.EstadoHabitacion,
                        SucursalGuid = sucursal?.SucursalGuid ?? Guid.Empty,
                        TipoHabitacionGuid = tipo?.TipoHabitacionGuid ?? Guid.Empty,
                        TipoHabitacionSlug = tipo?.Slug ?? string.Empty,
                        ImagenUrl = h.Url
                    };
                });

            return Ok(result);
        }

        [HttpGet("{habitacionGuid:guid}")]
        public async Task<ActionResult<HabitacionPublicDto>> GetByGuid(Guid habitacionGuid)
        {
            var habitacion = await _habitacionService.GetByGuidAsync(habitacionGuid);
            var sucursal = await _sucursalService.GetByIdAsync(habitacion.IdSucursal);
            var tipo = await _tipoHabitacionService.GetByIdAsync(habitacion.IdTipoHabitacion);
            
            var dto = new HabitacionPublicDto
            {
                IdHabitacion = habitacion.IdHabitacion,
                HabitacionGuid = habitacion.HabitacionGuid,
                NumeroHabitacion = habitacion.NumeroHabitacion,
                Piso = habitacion.Piso,
                CapacidadHabitacion = habitacion.CapacidadHabitacion,
                PrecioBase = habitacion.PrecioBase, // Ya viene calculado por HabitacionService
                DescripcionHabitacion = habitacion.DescripcionHabitacion,
                EstadoHabitacion = habitacion.EstadoHabitacion,
                SucursalGuid = sucursal.SucursalGuid,
                TipoHabitacionGuid = tipo.TipoHabitacionGuid,
                TipoHabitacionSlug = tipo.Slug,
                ImagenUrl = habitacion.Url
            };

            return Ok(dto);
        }
    }
}
