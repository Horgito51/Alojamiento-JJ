using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Servicio.Hotel.Business.Common;
using Servicio.Hotel.Business.DTOs.Alojamiento;
using Servicio.Hotel.Business.Exceptions;
using Servicio.Hotel.Business.Interfaces.Alojamiento;
using Servicio.Hotel.Business.Mappers.Alojamiento;
using Servicio.Hotel.Business.Validators.Alojamiento;
using Servicio.Hotel.DataManagement.Alojamiento.Interfaces;

namespace Servicio.Hotel.Business.Services.Alojamiento
{
    public class HabitacionService : IHabitacionService
    {
        private readonly IHabitacionDataService _habitacionDataService;

        public HabitacionService(IHabitacionDataService habitacionDataService)
        {
            _habitacionDataService = habitacionDataService;
        }

        public async Task<HabitacionDTO> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var dataModel = await _habitacionDataService.GetByIdAsync(id, ct);
            if (dataModel == null)
                throw new NotFoundException("HAB-001", $"No se encontró la habitación con ID {id}.");
            return dataModel.ToDto();
        }

        public async Task<HabitacionDTO> GetByGuidAsync(Guid guid, CancellationToken ct = default)
        {
            var dataModel = await _habitacionDataService.GetByGuidAsync(guid, ct);
            if (dataModel == null)
                throw new NotFoundException("HAB-002", $"No se encontró la habitación con GUID {guid}.");
            return dataModel.ToDto();
        }

        public async Task<IEnumerable<HabitacionDTO>> GetAllAsync(CancellationToken ct = default)
        {
            var pagedResult = await _habitacionDataService.GetAllPagedAsync(1, int.MaxValue, ct);
            return pagedResult.Items.ToDtoList();
        }

        public async Task<HabitacionDTO> CreateAsync(HabitacionCreateDTO habitacionCreateDto, CancellationToken ct = default)
        {
            var habitacionDto = new HabitacionDTO
            {
                IdSucursal = habitacionCreateDto.IdSucursal,
                IdTipoHabitacion = habitacionCreateDto.IdTipoHabitacion,
                NumeroHabitacion = habitacionCreateDto.NumeroHabitacion,
                Piso = habitacionCreateDto.Piso,
                CapacidadHabitacion = habitacionCreateDto.CapacidadHabitacion,
                PrecioBase = habitacionCreateDto.PrecioBase,
                DescripcionHabitacion = habitacionCreateDto.DescripcionHabitacion ?? string.Empty,
                EstadoHabitacion = habitacionCreateDto.EstadoHabitacion
            };

            HabitacionValidator.Validate(habitacionDto);
            var dataModel = habitacionDto.ToDataModel();
            var created = await _habitacionDataService.AddAsync(dataModel, ct);
            return created.ToDto();
        }

        public async Task UpdateAsync(HabitacionUpdateDTO habitacionUpdateDto, CancellationToken ct = default)
        {
            var habitacionDto = new HabitacionDTO
            {
                IdHabitacion = habitacionUpdateDto.IdHabitacion,
                IdTipoHabitacion = habitacionUpdateDto.IdTipoHabitacion,
                NumeroHabitacion = habitacionUpdateDto.NumeroHabitacion,
                Piso = habitacionUpdateDto.Piso,
                CapacidadHabitacion = habitacionUpdateDto.CapacidadHabitacion,
                PrecioBase = habitacionUpdateDto.PrecioBase,
                DescripcionHabitacion = habitacionUpdateDto.DescripcionHabitacion ?? string.Empty,
                EstadoHabitacion = habitacionUpdateDto.EstadoHabitacion
            };

            HabitacionValidator.Validate(habitacionDto);
            var existing = await _habitacionDataService.GetByIdAsync(habitacionUpdateDto.IdHabitacion, ct);
            if (existing == null)
                throw new NotFoundException("HAB-003", $"No se encontró la habitación con ID {habitacionUpdateDto.IdHabitacion}.");

            // Solo actualizamos los campos permitidos en la actualización
            existing.IdTipoHabitacion = habitacionDto.IdTipoHabitacion;
            existing.NumeroHabitacion = habitacionDto.NumeroHabitacion;
            existing.Piso = habitacionDto.Piso;
            existing.CapacidadHabitacion = habitacionDto.CapacidadHabitacion;
            existing.PrecioBase = habitacionDto.PrecioBase;
            existing.DescripcionHabitacion = habitacionDto.DescripcionHabitacion;
            existing.EstadoHabitacion = habitacionDto.EstadoHabitacion;

            await _habitacionDataService.UpdateAsync(existing, ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var existing = await _habitacionDataService.GetByIdAsync(id, ct);
            if (existing == null)
                throw new NotFoundException("HAB-004", $"No se encontró la habitación con ID {id}.");
            await _habitacionDataService.DeleteAsync(id, ct);
        }

        public async Task<IEnumerable<HabitacionDTO>> GetBySucursalAsync(int idSucursal, CancellationToken ct = default)
        {
            var dataModels = await _habitacionDataService.GetBySucursalAsync(idSucursal, ct);
            return dataModels.ToDtoList();
        }

        public async Task<IEnumerable<HabitacionDTO>> GetByTipoHabitacionAsync(int idTipoHabitacion, CancellationToken ct = default)
        {
            var dataModels = await _habitacionDataService.GetByTipoHabitacionAsync(idTipoHabitacion, ct);
            return dataModels.ToDtoList();
        }

        public async Task UpdateEstadoAsync(int id, string nuevoEstado, string usuario, CancellationToken ct = default)
        {
            var existing = await _habitacionDataService.GetByIdAsync(id, ct);
            if (existing == null)
                throw new NotFoundException("HAB-005", $"No se encontró la habitación con ID {id}.");
            await _habitacionDataService.UpdateEstadoAsync(id, nuevoEstado, usuario, ct);
        }
    }
}