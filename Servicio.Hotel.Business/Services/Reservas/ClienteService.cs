using System;
using System.Threading;
using System.Threading.Tasks;
using Servicio.Hotel.Business.Common;
using Servicio.Hotel.Business.DTOs.Reservas;
using Servicio.Hotel.Business.Exceptions;
using Servicio.Hotel.Business.Interfaces.Reservas;
using Servicio.Hotel.Business.Mappers.Reservas;
using Servicio.Hotel.Business.Validators.Reservas;
using Servicio.Hotel.DataManagement.Reservas.Interfaces;

namespace Servicio.Hotel.Business.Services.Reservas
{
    public class ClienteService : IClienteService
    {
        private readonly IClienteDataService _clienteDataService;

        public ClienteService(IClienteDataService clienteDataService)
        {
            _clienteDataService = clienteDataService;
        }

        public async Task<ClienteDTO> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var dataModel = await _clienteDataService.GetByIdAsync(id, ct);
            if (dataModel == null)
                throw new NotFoundException("CLI-001", $"No se encontró el cliente con ID {id}.");
            return dataModel.ToDto();
        }

        public async Task<ClienteDTO> GetByGuidAsync(Guid guid, CancellationToken ct = default)
        {
            var dataModel = await _clienteDataService.GetByGuidAsync(guid, ct);
            if (dataModel == null)
                throw new NotFoundException("CLI-002", $"No se encontró el cliente con GUID {guid}.");
            return dataModel.ToDto();
        }

        public async Task<PagedResult<ClienteDTO>> GetAllPagedAsync(int pageNumber, int pageSize, CancellationToken ct = default)
        {
            var pagedData = await _clienteDataService.GetAllPagedAsync(pageNumber, pageSize, ct);
            return new PagedResult<ClienteDTO>
            {
                Items = pagedData.Items.ToDtoList(),
                TotalCount = pagedData.TotalCount,
                PageNumber = pagedData.PageNumber,
                PageSize = pagedData.PageSize
            };
        }

        public async Task<ClienteDTO> CreateAsync(ClienteCreateDTO clienteCreateDto, CancellationToken ct = default)
        {
            var dataModel = clienteCreateDto.ToDataModel();
            var created = await _clienteDataService.AddAsync(dataModel, ct);
            return created.ToDto();
        }

        public async Task UpdateAsync(ClienteUpdateDTO clienteUpdateDto, CancellationToken ct = default)
        {
            var existing = await _clienteDataService.GetByIdAsync(clienteUpdateDto.IdCliente, ct);
            if (existing == null)
                throw new NotFoundException("CLI-003", $"No se encontró el cliente con ID {clienteUpdateDto.IdCliente}.");

            existing.Nombres = clienteUpdateDto.Nombres;
            existing.Apellidos = clienteUpdateDto.Apellidos;
            existing.RazonSocial = clienteUpdateDto.RazonSocial ?? string.Empty;
            existing.Correo = clienteUpdateDto.Correo;
            existing.Telefono = clienteUpdateDto.Telefono ?? string.Empty;
            existing.Direccion = clienteUpdateDto.Direccion ?? string.Empty;
            existing.Estado = clienteUpdateDto.Estado;

            await _clienteDataService.UpdateAsync(existing, ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var existing = await _clienteDataService.GetByIdAsync(id, ct);
            if (existing == null)
                throw new NotFoundException("CLI-004", $"No se encontró el cliente con ID {id}.");
            await _clienteDataService.DeleteAsync(id, ct);
        }

        public async Task<ClienteDTO> GetByIdentificacionAsync(string tipo, string numero, CancellationToken ct = default)
        {
            var dataModel = await _clienteDataService.GetByIdentificacionAsync(tipo, numero, ct);
            if (dataModel == null)
                throw new NotFoundException("CLI-005", $"No se encontró cliente con identificación {tipo} {numero}.");
            return dataModel.ToDto();
        }

        public async Task<ClienteDTO> GetByCorreoAsync(string correo, CancellationToken ct = default)
        {
            var dataModel = await _clienteDataService.GetByCorreoAsync(correo, ct);
            if (dataModel == null)
                throw new NotFoundException("CLI-006", $"No se encontró cliente con correo {correo}.");
            return dataModel.ToDto();
        }

        public async Task InhabilitarAsync(int id, string motivo, string usuario, CancellationToken ct = default)
        {
            var existing = await _clienteDataService.GetByIdAsync(id, ct);
            if (existing == null)
                throw new NotFoundException("CLI-007", $"No se encontró el cliente con ID {id}.");
            await _clienteDataService.InhabilitarAsync(id, motivo, usuario, ct);
        }
    }
}