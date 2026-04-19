using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Servicio.Hotel.Business.DTOs.Seguridad;
using Servicio.Hotel.Business.Interfaces.Seguridad;
using System.Threading.Tasks;

namespace Servicio.Hotel.API.Controllers.Internal
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/internal/[controller]")]
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
        public async Task<IActionResult> Refresh([FromBody] string refreshToken)
        {
            var response = await _authService.RefreshTokenAsync(refreshToken);
            return Ok(response);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] string refreshToken)
        {
            await _authService.LogoutAsync(refreshToken);
            return NoContent();
        }
    }
}
