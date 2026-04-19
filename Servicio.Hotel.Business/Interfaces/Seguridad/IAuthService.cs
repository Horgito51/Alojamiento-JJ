using System.Threading;
using System.Threading.Tasks;
using Servicio.Hotel.Business.DTOs.Seguridad;

namespace Servicio.Hotel.Business.Interfaces.Seguridad
{
    public interface IAuthService
    {
        Task<LoginResponseDTO> LoginAsync(LoginRequestDTO loginRequest, CancellationToken ct = default);
        Task LogoutAsync(string refreshToken, CancellationToken ct = default);
        Task<LoginResponseDTO> RefreshTokenAsync(string refreshToken, CancellationToken ct = default);
        Task<bool> ValidateTokenAsync(string token, CancellationToken ct = default);
    }
}