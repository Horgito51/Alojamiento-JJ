using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Servicio.Hotel.API.Models.Requests.Internal;
using Servicio.Hotel.Business.DTOs.Reservas;
using Servicio.Hotel.Business.Interfaces.Reservas;
using System.Threading.Tasks;

namespace Servicio.Hotel.API.Controllers.V1.Booking
{
    [ApiController]
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/public/clientes")]
    public class ClientesPublicWriteController : ControllerBase
    {
        private readonly IClienteService _clienteService;

        public ClientesPublicWriteController(IClienteService clienteService)
        {
            _clienteService = clienteService;
        }

        [HttpGet("by-email")]
        public async Task<ActionResult<ClienteDTO>> GetByEmail([FromQuery] string correo)
        {
            var result = await _clienteService.GetByCorreoAsync(correo);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<ClienteDTO>> Create([FromBody] ClienteCreateRequest request)
        {
            var result = await _clienteService.CreateAsync(request.ToCreateDto());
            return CreatedAtAction(nameof(GetByEmail), new { correo = result.Correo }, result);
        }
    }
}
