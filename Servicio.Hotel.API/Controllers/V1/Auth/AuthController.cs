using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Servicio.Hotel.API.Models.Requests.Internal;
using Servicio.Hotel.Business.DTOs.Seguridad;
using Servicio.Hotel.Business.Interfaces.Seguridad;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Servicio.Hotel.API.Controllers.V1.Auth
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/internal/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO loginRequest)
        {
            var response = await _authService.LoginAsync(loginRequest);
            return Ok(response);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            var response = await _authService.RefreshTokenAsync(request.RefreshToken);
            return Ok(response);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            await _authService.LogoutAsync(request.RefreshToken);
            return NoContent();
        }

        [Authorize]
        [HttpPost("cambiar-password")]
        public async Task<IActionResult> CambiarPassword([FromBody] CambiarPasswordRequest request)
        {
            var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idValue, out var idUsuario))
                throw new UnauthorizedAccessException();

            var usuario = User.Identity?.Name ?? "Sistema";
            await _authService.CambiarPasswordAsync(idUsuario, request.PasswordActual, request.PasswordNuevo, usuario);
            return NoContent();
        }
    }
}
