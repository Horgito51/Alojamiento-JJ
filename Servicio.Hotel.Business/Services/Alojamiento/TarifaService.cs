using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Servicio.Hotel.Business.DTOs.Alojamiento;
using Servicio.Hotel.Business.Exceptions;
using Servicio.Hotel.Business.Interfaces.Alojamiento;
using Servicio.Hotel.Business.Mappers.Alojamiento;
using Servicio.Hotel.Business.Validators.Alojamiento; // Si existiera TarifaValidator, de lo contrario se puede omitir o crear básico
using Servicio.Hotel.DataManagement.Alojamiento.Interfaces;

namespace Servicio.Hotel.Business.Services.Alojamiento
{
    public class TarifaService : ITarifaService
    {
        private readonly ITarifaDataService _tarifaDataService;

        public TarifaService(ITarifaDataService tarifaDataService)
        {
            _tarifaDataService = tarifaDataService;
        }

        public async Task<TarifaDTO> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var dataModel = await _tarifaDataService.GetByIdAsync(id, ct);
            if (dataModel == null)
                throw new NotFoundException("TAR-001", $"No se encontró la tarifa con ID {id}.");
            return dataModel.ToDto();
        }

        public async Task<TarifaDTO> GetByGuidAsync(Guid guid, CancellationToken ct = default)
        {
            var dataModel = await _tarifaDataService.GetByGuidAsync(guid, ct);
            if (dataModel == null)
                throw new NotFoundException("TAR-002", $"No se encontró la tarifa con GUID {guid}.");
            return dataModel.ToDto();
        }

        public async Task<IEnumerable<TarifaDTO>> GetAllAsync(CancellationToken ct = default)
        {
            var pagedResult = await _tarifaDataService.GetAllPagedAsync(1, int.MaxValue, ct);
            return pagedResult.Items.ToDtoList();
        }

        public async Task<TarifaDTO> CreateAsync(TarifaCreateDTO tarifaCreateDto, CancellationToken ct = default)
        {
            if (tarifaCreateDto.PrecioPorNoche <= 0)
                throw new ValidationException("TAR-003", "El precio por noche debe ser mayor a cero.");
            var dataModel = tarifaCreateDto.ToDataModel();
            var created = await _tarifaDataService.AddAsync(dataModel, ct);
            return created.ToDto();
        }

        public async Task UpdateAsync(TarifaUpdateDTO tarifaUpdateDto, CancellationToken ct = default)
        {
            var existing = await _tarifaDataService.GetByIdAsync(tarifaUpdateDto.IdTarifa, ct);
            if (existing == null)
                throw new NotFoundException("TAR-004", $"No se encontró la tarifa con ID {tarifaUpdateDto.IdTarifa}.");
            var dataModel = tarifaUpdateDto.ToDataModel();
            await _tarifaDataService.UpdateAsync(dataModel, ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var existing = await _tarifaDataService.GetByIdAsync(id, ct);
            if (existing == null)
                throw new NotFoundException("TAR-005", $"No se encontró la tarifa con ID {id}.");
            await _tarifaDataService.DeleteAsync(id, ct);
        }

        public async Task<IEnumerable<TarifaDTO>> GetBySucursalAsync(int idSucursal, CancellationToken ct = default)
        {
            var dataModels = await _tarifaDataService.GetBySucursalAsync(idSucursal, ct);
            return dataModels.ToDtoList();
        }

        public async Task<TarifaDTO> GetTarifaVigenteAsync(int idSucursal, int idTipoHabitacion, DateTime fecha, CancellationToken ct = default)
        {
            var dataModel = await _tarifaDataService.GetTarifaVigenteAsync(idSucursal, idTipoHabitacion, fecha, ct);
            if (dataModel == null)
                throw new NotFoundException("TAR-006", "No hay tarifa vigente para los parámetros especificados.");
            return dataModel.ToDto();
        }

        public async Task DesactivarAsync(int id, string usuario, CancellationToken ct = default)
        {
            var existing = await _tarifaDataService.GetByIdAsync(id, ct);
            if (existing == null)
                throw new NotFoundException("TAR-007", $"No se encontró la tarifa con ID {id}.");
            await _tarifaDataService.DesactivarAsync(id, usuario, ct);
        }
    }
}