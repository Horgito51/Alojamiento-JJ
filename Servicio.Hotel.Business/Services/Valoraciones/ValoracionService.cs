using System;
using System.Threading;
using System.Threading.Tasks;
using Servicio.Hotel.Business.Common;
using Servicio.Hotel.Business.DTOs.Valoraciones;
using Servicio.Hotel.Business.Exceptions;
using Servicio.Hotel.Business.Interfaces.Valoraciones;
using Servicio.Hotel.Business.Mappers.Valoraciones;
using Servicio.Hotel.DataManagement.Valoraciones.Interfaces;

namespace Servicio.Hotel.Business.Services.Valoraciones
{
    public class ValoracionService : IValoracionService
    {
        private readonly IValoracionDataService _valoracionDataService;

        public ValoracionService(IValoracionDataService valoracionDataService)
        {
            _valoracionDataService = valoracionDataService;
        }

        public async Task<ValoracionDTO> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var dataModel = await _valoracionDataService.GetByIdAsync(id, ct);
            if (dataModel == null)
                throw new NotFoundException("VAL-001", $"No se encontró la valoración con ID {id}.");
            return dataModel.ToDto();
        }

        public async Task<ValoracionDTO> GetByGuidAsync(Guid guid, CancellationToken ct = default)
        {
            var dataModel = await _valoracionDataService.GetByGuidAsync(guid, ct);
            if (dataModel == null)
                throw new NotFoundException("VAL-002", $"No se encontró la valoración con GUID {guid}.");
            return dataModel.ToDto();
        }

        public async Task<PagedResult<ValoracionDTO>> GetByFiltroAsync(ValoracionFiltroDTO filtro, int pageNumber, int pageSize, CancellationToken ct = default)
        {
            var pagedData = await _valoracionDataService.GetByFiltroAsync(filtro.ToDataModel(), pageNumber, pageSize, ct);
            return new PagedResult<ValoracionDTO>
            {
                Items = pagedData.Items.ToDtoList(),
                TotalCount = pagedData.TotalCount,
                PageNumber = pagedData.PageNumber,
                PageSize = pagedData.PageSize
            };
        }

        public async Task<ValoracionDTO> CreateAsync(ValoracionDTO valoracionDto, CancellationToken ct = default)
        {
            if (valoracionDto.PuntuacionGeneral < 0 || valoracionDto.PuntuacionGeneral > 10)
                throw new ValidationException("VAL-003", "La puntuación general debe estar entre 0 y 10.");
            var dataModel = valoracionDto.ToDataModel();
            var created = await _valoracionDataService.AddAsync(dataModel, ct);
            return created.ToDto();
        }

        public async Task UpdateAsync(ValoracionDTO valoracionDto, CancellationToken ct = default)
        {
            var existing = await _valoracionDataService.GetByIdAsync(valoracionDto.IdValoracion, ct);
            if (existing == null)
                throw new NotFoundException("VAL-004", $"No se encontró la valoración con ID {valoracionDto.IdValoracion}.");
            var dataModel = valoracionDto.ToDataModel();
            await _valoracionDataService.UpdateAsync(dataModel, ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var existing = await _valoracionDataService.GetByIdAsync(id, ct);
            if (existing == null)
                throw new NotFoundException("VAL-005", $"No se encontró la valoración con ID {id}.");
            await _valoracionDataService.DeleteAsync(id, ct);
        }

        public async Task ModerarAsync(int idValoracion, string nuevoEstado, string motivo, string moderador, CancellationToken ct = default)
        {
            var existing = await _valoracionDataService.GetByIdAsync(idValoracion, ct);
            if (existing == null)
                throw new NotFoundException("VAL-006", $"No se encontró la valoración con ID {idValoracion}.");
            await _valoracionDataService.ModerarAsync(idValoracion, nuevoEstado, motivo, moderador, ct);
        }

        public async Task ResponderAsync(int idValoracion, string respuesta, string usuario, CancellationToken ct = default)
        {
            var existing = await _valoracionDataService.GetByIdAsync(idValoracion, ct);
            if (existing == null)
                throw new NotFoundException("VAL-007", $"No se encontró la valoración con ID {idValoracion}.");
            await _valoracionDataService.ResponderAsync(idValoracion, respuesta, usuario, ct);
        }

        public async Task<bool> ExistsByEstadiaAsync(int idEstadia, CancellationToken ct = default)
        {
            return await _valoracionDataService.ExistsByEstadiaAsync(idEstadia, ct);
        }
    }
}