using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Servicio.Hotel.Business.DTOs.Seguridad;
using Servicio.Hotel.Business.Exceptions;
using Servicio.Hotel.Business.Interfaces.Seguridad;
using Servicio.Hotel.Business.Validators.Seguridad;
using Servicio.Hotel.DataManagement.Seguridad.Interfaces;
using Servicio.Hotel.DataManagement.Seguridad.Models;

namespace Servicio.Hotel.Business.Services.Seguridad
{
    public class AuthService : IAuthService
    {
        private readonly IUsuarioDataService _usuarioDataService;
        private readonly IConfiguration _configuration;

        public AuthService(IUsuarioDataService usuarioDataService, IConfiguration configuration)
        {
            _usuarioDataService = usuarioDataService;
            _configuration = configuration;
        }

        public async Task<LoginResponseDTO> LoginAsync(LoginRequestDTO loginRequest, CancellationToken ct = default)
        {
            LoginValidator.Validate(loginRequest);

            var credenciales = await _usuarioDataService.GetCredentialsByUsernameAsync(loginRequest.Username, ct);
            if (credenciales == null)
                throw new UnauthorizedBusinessException("AUTH-001", "Usuario o contraseña incorrectos.");

            var usuario = await _usuarioDataService.GetByUsernameAsync(loginRequest.Username, ct);
            if (usuario == null)
                throw new UnauthorizedBusinessException("AUTH-001", "Usuario o contraseña incorrectos.");

            var roles = usuario.Roles?.Select(r => r.NombreRol).ToList() ?? new List<string>();
            var token = GenerarTokenJWT(usuario, roles);
            var expirationMinutes = int.TryParse(_configuration["Jwt:ExpirationMinutes"], out var mins) ? mins : 60;

            return new LoginResponseDTO
            {
                AccessToken = token,
                RefreshToken = Guid.NewGuid().ToString(),
                ExpiresIn = expirationMinutes * 60,
                UsuarioGuid = usuario.UsuarioGuid,
                Username = usuario.Username,
                Correo = usuario.Correo,
                NombreCompleto = $"{usuario.Nombres} {usuario.Apellidos}",
                Roles = roles
            };
        }

        private string GenerarTokenJWT(UsuarioDataModel usuario, List<string> roles)
        {
            // Leer exactamente los mismos valores que usa AuthenticationExtensions
            var secret   = _configuration["Jwt:Secret"]   ?? "MiClaveSuperSecretaParaJWT1234567890!";
            var issuer   = _configuration["Jwt:Issuer"]   ?? "Microservicio.Clientes.API";
            var audience = _configuration["Jwt:Audience"] ?? "Microservicio.Clientes.API";
            var expirationMinutes = int.TryParse(_configuration["Jwt:ExpirationMinutes"], out var mins) ? mins : 60;

            var key = Encoding.ASCII.GetBytes(secret);
            var tokenHandler = new JwtSecurityTokenHandler();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Name, usuario.Username),
                new Claim(ClaimTypes.Email, usuario.Correo),
                new Claim("nombres", usuario.Nombres ?? ""),
                new Claim("apellidos", usuario.Apellidos ?? "")
            };

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                NotBefore = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
        }

        public Task LogoutAsync(string refreshToken, CancellationToken ct = default)
            => Task.CompletedTask;

        public Task<LoginResponseDTO> RefreshTokenAsync(string refreshToken, CancellationToken ct = default)
            => throw new NotImplementedException();

        public Task<bool> ValidateTokenAsync(string token, CancellationToken ct = default)
            => Task.FromResult(true);
    }
}
