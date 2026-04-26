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
using Servicio.Hotel.Business.Common;
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

            var loginKey = loginRequest.Username.Trim();

            var credenciales = await _usuarioDataService.GetCredentialsByUsernameAsync(loginKey, ct);
            var usuario = await _usuarioDataService.GetByUsernameAsync(loginKey, ct);

            if (credenciales == null || usuario == null)
            {
                credenciales = await _usuarioDataService.GetCredentialsByCorreoAsync(loginKey, ct);
                usuario = await _usuarioDataService.GetByCorreoAsync(loginKey, ct);
            }

            if (credenciales == null || usuario == null)
                throw new UnauthorizedBusinessException("AUTH-001", "Usuario o contraseña incorrectos.");

            if (!PasswordHasher.Verify(loginRequest.Password, credenciales.PasswordHash, credenciales.PasswordSalt))
                throw new UnauthorizedBusinessException("AUTH-001", "Usuario o contraseña incorrectos.");

            var roles = usuario.Roles?.Select(r => r.NombreRol).ToList() ?? new List<string>();
            var token = GenerarTokenJWT(usuario, roles);
            var expirationMinutes = int.TryParse(_configuration["Jwt:ExpirationMinutes"], out var mins) ? mins : 60;

            var refreshToken = GenerarRefreshTokenJWT(usuario);

            return new LoginResponseDTO
            {
                AccessToken = token,
                RefreshToken = refreshToken,
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

        private string GenerarRefreshTokenJWT(UsuarioDataModel usuario)
        {
            var secret = _configuration["Jwt:Secret"] ?? "MiClaveSuperSecretaParaJWT1234567890!";
            var issuer = _configuration["Jwt:Issuer"] ?? "Microservicio.Clientes.API";
            var audience = _configuration["Jwt:Audience"] ?? "Microservicio.Clientes.API";
            var refreshDays = int.TryParse(_configuration["Jwt:RefreshExpirationDays"], out var days) ? days : 7;

            var key = Encoding.ASCII.GetBytes(secret);
            var tokenHandler = new JwtSecurityTokenHandler();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Name, usuario.Username),
                new Claim("token_type", "refresh")
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                NotBefore = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(refreshDays),
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

        public async Task<LoginResponseDTO> RefreshTokenAsync(string refreshToken, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                throw new UnauthorizedBusinessException("AUTH-002", "Refresh token inválido.");

            var principal = ValidateJwt(refreshToken, out var tokenType);
            if (principal == null || tokenType != "refresh")
                throw new UnauthorizedBusinessException("AUTH-002", "Refresh token inválido.");

            var idValue = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(idValue, out var idUsuario))
                throw new UnauthorizedBusinessException("AUTH-002", "Refresh token inválido.");

            var usuario = await _usuarioDataService.GetByIdAsync(idUsuario, ct);
            if (usuario == null)
                throw new UnauthorizedBusinessException("AUTH-002", "Refresh token inválido.");

            var roles = usuario.Roles?.Select(r => r.NombreRol).ToList() ?? new List<string>();
            var token = GenerarTokenJWT(usuario, roles);
            var newRefresh = GenerarRefreshTokenJWT(usuario);
            var expirationMinutes = int.TryParse(_configuration["Jwt:ExpirationMinutes"], out var mins) ? mins : 60;

            return new LoginResponseDTO
            {
                AccessToken = token,
                RefreshToken = newRefresh,
                ExpiresIn = expirationMinutes * 60,
                UsuarioGuid = usuario.UsuarioGuid,
                Username = usuario.Username,
                Correo = usuario.Correo,
                NombreCompleto = $"{usuario.Nombres} {usuario.Apellidos}",
                Roles = roles
            };
        }

        public async Task CambiarPasswordAsync(int idUsuario, string passwordActual, string passwordNuevo, string usuario, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(passwordActual) || string.IsNullOrWhiteSpace(passwordNuevo))
                throw new ValidationException("AUTH-003", "password_actual y password_nuevo son obligatorios.");

            var cred = await _usuarioDataService.GetCredentialsByIdAsync(idUsuario, ct);
            if (cred == null)
                throw new NotFoundException("AUTH-004", "Usuario no encontrado.");

            if (!PasswordHasher.Verify(passwordActual, cred.PasswordHash, cred.PasswordSalt))
                throw new UnauthorizedBusinessException("AUTH-005", "La contraseña actual no es correcta.");

            var (hash, salt) = PasswordHasher.HashPassword(passwordNuevo);
            await _usuarioDataService.CambiarPasswordAsync(idUsuario, hash, salt, usuario, ct);
        }

        private ClaimsPrincipal? ValidateJwt(string token, out string? tokenType)
        {
            tokenType = null;
            var secret = _configuration["Jwt:Secret"] ?? "MiClaveSuperSecretaParaJWT1234567890!";
            var issuer = _configuration["Jwt:Issuer"] ?? "Microservicio.Clientes.API";
            var audience = _configuration["Jwt:Audience"] ?? "Microservicio.Clientes.API";

            var key = Encoding.ASCII.GetBytes(secret);
            var handler = new JwtSecurityTokenHandler();

            try
            {
                var principal = handler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out _);

                tokenType = principal.FindFirst("token_type")?.Value;
                return principal;
            }
            catch
            {
                return null;
            }
        }

        public Task<bool> ValidateTokenAsync(string token, CancellationToken ct = default)
            => Task.FromResult(true);
    }
}
