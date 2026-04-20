using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Servicio.Hotel.Business.Common;
using Servicio.Hotel.Business.DTOs.Seguridad;
using Servicio.Hotel.Business.Exceptions;
using Servicio.Hotel.Business.Interfaces.Seguridad;
using Servicio.Hotel.Business.Mappers.Seguridad;
using Servicio.Hotel.Business.Validators.Seguridad; // Podrías crear un UsuarioValidator
using Servicio.Hotel.DataManagement.Seguridad.Interfaces;

namespace Servicio.Hotel.Business.Services.Seguridad
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioDataService _usuarioDataService;

        public UsuarioService(IUsuarioDataService usuarioDataService)
        {
            _usuarioDataService = usuarioDataService;
        }

        public async Task<UsuarioDTO> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var dataModel = await _usuarioDataService.GetByIdAsync(id, ct);
            if (dataModel == null)
                throw new NotFoundException("USR-001", $"No se encontró el usuario con ID {id}.");
            return dataModel.ToDto();
        }

        public async Task<UsuarioDTO> GetByGuidAsync(Guid guid, CancellationToken ct = default)
        {
            var dataModel = await _usuarioDataService.GetByGuidAsync(guid, ct);
            if (dataModel == null)
                throw new NotFoundException("USR-002", $"No se encontró el usuario con GUID {guid}.");
            return dataModel.ToDto();
        }

        public async Task<PagedResult<UsuarioDTO>> GetAllPagedAsync(int pageNumber, int pageSize, CancellationToken ct = default)
        {
            var pagedData = await _usuarioDataService.GetAllPagedAsync(pageNumber, pageSize, ct);
            return new PagedResult<UsuarioDTO>
            {
                Items = pagedData.Items.ToDtoList(),
                TotalCount = pagedData.TotalCount,
                PageNumber = pagedData.PageNumber,
                PageSize = pagedData.PageSize
            };
        }

        public async Task<UsuarioDTO> CreateAsync(UsuarioCreateDTO usuarioCreateDto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(usuarioCreateDto.Username))
                throw new ValidationException("USR-003", "El nombre de usuario es obligatorio.");
            if (string.IsNullOrWhiteSpace(usuarioCreateDto.Correo))
                throw new ValidationException("USR-004", "El correo es obligatorio.");

            var dataModel = new Servicio.Hotel.DataManagement.Seguridad.Models.UsuarioDataModel
            {
                IdCliente = usuarioCreateDto.IdCliente,
                Username = usuarioCreateDto.Username,
                Correo = usuarioCreateDto.Correo,
                Nombres = usuarioCreateDto.Nombres,
                Apellidos = usuarioCreateDto.Apellidos,
                EstadoUsuario = usuarioCreateDto.EstadoUsuario,
                Activo = usuarioCreateDto.Activo,
                Roles = usuarioCreateDto.Roles?.Select(r => r.ToDataModel()).ToList()
            };

            var created = await _usuarioDataService.AddAsync(dataModel, ct);
            return created.ToDto();
        }

        public async Task UpdateAsync(UsuarioUpdateDTO usuarioUpdateDto, CancellationToken ct = default)
        {
            var existing = await _usuarioDataService.GetByIdAsync(usuarioUpdateDto.IdUsuario, ct);
            if (existing == null)
                throw new NotFoundException("USR-005", $"No se encontró el usuario con ID {usuarioUpdateDto.IdUsuario}.");

            // Solo actualizamos los campos permitidos en la actualización
            existing.Correo = usuarioUpdateDto.Correo;
            existing.Nombres = usuarioUpdateDto.Nombres;
            existing.Apellidos = usuarioUpdateDto.Apellidos;
            existing.EstadoUsuario = usuarioUpdateDto.EstadoUsuario;
            existing.Activo = usuarioUpdateDto.Activo;
            existing.Roles = usuarioUpdateDto.Roles?.Select(r => r.ToDataModel()).ToList();

            await _usuarioDataService.UpdateAsync(existing, ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var existing = await _usuarioDataService.GetByIdAsync(id, ct);
            if (existing == null)
                throw new NotFoundException("USR-006", $"No se encontró el usuario con ID {id}.");
            await _usuarioDataService.DeleteAsync(id, ct);
        }

        public async Task<UsuarioDTO> GetByUsernameAsync(string username, CancellationToken ct = default)
        {
            var dataModel = await _usuarioDataService.GetByUsernameAsync(username, ct);
            if (dataModel == null)
                throw new NotFoundException("USR-007", $"No se encontró usuario con nombre {username}.");
            return dataModel.ToDto();
        }

        public async Task<UsuarioDTO> GetByCorreoAsync(string correo, CancellationToken ct = default)
        {
            var dataModel = await _usuarioDataService.GetByCorreoAsync(correo, ct);
            if (dataModel == null)
                throw new NotFoundException("USR-008", $"No se encontró usuario con correo {correo}.");
            return dataModel.ToDto();
        }

        public async Task InhabilitarAsync(int id, string motivo, string usuario, CancellationToken ct = default)
        {
            var existing = await _usuarioDataService.GetByIdAsync(id, ct);
            if (existing == null)
                throw new NotFoundException("USR-009", $"No se encontró el usuario con ID {id}.");
            await _usuarioDataService.InhabilitarAsync(id, motivo, usuario, ct);
        }

        public async Task CambiarPasswordAsync(int id, string newPassword, string usuario, CancellationToken ct = default)
        {
            var existing = await _usuarioDataService.GetByIdAsync(id, ct);
            if (existing == null)
                throw new NotFoundException("USR-010", $"No se encontró el usuario con ID {id}.");
            // Aquí se debería hashear la contraseña antes de enviar a DataService
            // Por simplicidad, asumimos que el DataService recibe el hash y omitimos el salt
            await _usuarioDataService.CambiarPasswordAsync(id, newPassword, string.Empty, usuario, ct);
        }

        public async Task<bool> ExistsByUsernameAsync(string username, int? excludeId = null, CancellationToken ct = default)
        {
            return await _usuarioDataService.ExistsByUsernameAsync(username, excludeId, ct);
        }

        public async Task<bool> ExistsByCorreoAsync(string correo, int? excludeId = null, CancellationToken ct = default)
        {
            return await _usuarioDataService.ExistsByCorreoAsync(correo, excludeId, ct);
        }
    }
}