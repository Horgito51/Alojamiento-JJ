using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Servicio.Hotel.API.Models.Requests.Internal;
using Servicio.Hotel.Business.Exceptions;
using Servicio.Hotel.Business.DTOs.Reservas;
using Servicio.Hotel.Business.Interfaces.Reservas;
using Servicio.Hotel.Business.Interfaces.Seguridad;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Servicio.Hotel.API.Controllers.V1.Booking
{
    [ApiController]
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/public/reservas")]
    public class ReservasPublicWriteController : ControllerBase
    {
        private readonly IReservaService _reservaService;
        private readonly IUsuarioService _usuarioService;
        private readonly IClienteService _clienteService;

        public ReservasPublicWriteController(IReservaService reservaService, IUsuarioService usuarioService, IClienteService clienteService)
        {
            _reservaService = reservaService;
            _usuarioService = usuarioService;
            _clienteService = clienteService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ReservaDTO>> GetById(int id)
        {
            var result = await _reservaService.GetByIdAsync(id);
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult> GetMisReservas(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 50,
            [FromQuery] string? estado = null)
        {
            var idCliente = await GetAuthenticatedClienteIdAsync();
            var filtro = new ReservaFiltroDTO 
            { 
                IdCliente = idCliente,
                EstadoReserva = estado
            };
            var result = await _reservaService.GetByFiltroAsync(filtro, page, limit);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<ReservaDTO>> Create([FromBody] ReservaCreateRequest request)
        {
            var idCliente = await GetAuthenticatedClienteIdAsync();

            if (request.FechaFin <= request.FechaInicio)
                throw new ValidationException("RES-PUB-001", "La fecha de fin debe ser posterior a la fecha de inicio.");

            if (request.Habitaciones == null || request.Habitaciones.Count == 0)
                throw new ValidationException("RES-PUB-002", "La reserva debe tener al menos una habitacion.");

            await _clienteService.GetByIdAsync(idCliente);

            request.IdCliente = idCliente;
            request.OrigenCanalReserva = "WEB";
            request.EstadoReserva = "PEN";

            var result = await _reservaService.CreateAsync(request.ToCreateDto());
            return CreatedAtAction(nameof(GetById), new { id = result.IdReserva }, result);
        }

        private async Task<int> GetAuthenticatedClienteIdAsync()
        {
            var idClienteClaim = User.Claims.FirstOrDefault(c => c.Type == "idCliente")?.Value;
            if (int.TryParse(idClienteClaim, out var idCliente) && idCliente > 0)
                return idCliente;

            var idUsuarioClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idUsuarioClaim, out var idUsuario))
                throw new UnauthorizedBusinessException("AUTH-CLIENTE-001", "Token sin identificacion de usuario.");

            var usuario = await _usuarioService.GetByIdAsync(idUsuario);
            if (usuario.IdCliente.HasValue && usuario.IdCliente.Value > 0)
                return usuario.IdCliente.Value;

            var cliente = await _clienteService.CreateAsync(new ClienteCreateDTO
            {
                TipoIdentificacion = "CLI",
                NumeroIdentificacion = $"CLI-{usuario.IdUsuario:D6}",
                Nombres = usuario.Nombres ?? string.Empty,
                Apellidos = usuario.Apellidos ?? string.Empty,
                RazonSocial = string.Empty,
                Correo = usuario.Correo ?? string.Empty,
                Telefono = "0000000000",
                Direccion = string.Empty,
                Estado = "ACT",
            });

            await _usuarioService.AsociarClienteAsync(idUsuario, cliente.IdCliente, User.Identity?.Name ?? usuario.Username, HttpContext.RequestAborted);
            return cliente.IdCliente;
        }
    }
}
