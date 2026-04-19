using System;
using System.Threading;
using System.Threading.Tasks;
using Servicio.Hotel.Business.Common;
using Servicio.Hotel.Business.DTOs.Facturacion;
using Servicio.Hotel.Business.Exceptions;
using Servicio.Hotel.Business.Interfaces.Facturacion;
using Servicio.Hotel.Business.Mappers.Facturacion;
using Servicio.Hotel.Business.Validators.Facturacion;
using Servicio.Hotel.DataManagement.Facturacion.Interfaces;

namespace Servicio.Hotel.Business.Services.Facturacion
{
    public class PagoService : IPagoService
    {
        private readonly IPagoDataService _pagoDataService;

        public PagoService(IPagoDataService pagoDataService)
        {
            _pagoDataService = pagoDataService;
        }

        public async Task<PagoDTO> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var dataModel = await _pagoDataService.GetByIdAsync(id, ct);
            if (dataModel == null)
                throw new NotFoundException("PAG-001", $"No se encontró el pago con ID {id}.");
            return dataModel.ToDto();
        }

        public async Task<PagoDTO> GetByGuidAsync(Guid guid, CancellationToken ct = default)
        {
            var dataModel = await _pagoDataService.GetByGuidAsync(guid, ct);
            if (dataModel == null)
                throw new NotFoundException("PAG-002", $"No se encontró el pago con GUID {guid}.");
            return dataModel.ToDto();
        }

        public async Task<PagedResult<PagoDTO>> GetByFacturaAsync(int idFactura, int pageNumber, int pageSize, CancellationToken ct = default)
        {
            var pagedData = await _pagoDataService.GetByFacturaAsync(idFactura, pageNumber, pageSize, ct);
            return new PagedResult<PagoDTO>
            {
                Items = pagedData.Items.ToDtoList(),
                TotalCount = pagedData.TotalCount,
                PageNumber = pagedData.PageNumber,
                PageSize = pagedData.PageSize
            };
        }

        public async Task<PagoDTO> CreateAsync(PagoDTO pagoDto, CancellationToken ct = default)
        {
            PagoValidator.Validate(pagoDto);
            var dataModel = pagoDto.ToDataModel();
            var created = await _pagoDataService.AddAsync(dataModel, ct);
            return created.ToDto();
        }

        public async Task UpdateAsync(PagoDTO pagoDto, CancellationToken ct = default)
        {
            var existing = await _pagoDataService.GetByIdAsync(pagoDto.IdPago, ct);
            if (existing == null)
                throw new NotFoundException("PAG-003", $"No se encontró el pago con ID {pagoDto.IdPago}.");
            var dataModel = pagoDto.ToDataModel();
            await _pagoDataService.UpdateAsync(dataModel, ct);
        }

        public async Task UpdateEstadoAsync(int idPago, string nuevoEstado, string usuario, CancellationToken ct = default)
        {
            var existing = await _pagoDataService.GetByIdAsync(idPago, ct);
            if (existing == null)
                throw new NotFoundException("PAG-004", $"No se encontró el pago con ID {idPago}.");
            await _pagoDataService.UpdateEstadoAsync(idPago, nuevoEstado, usuario, ct);
        }

        public async Task<decimal> GetTotalPagadoPorFacturaAsync(int idFactura, CancellationToken ct = default)
        {
            return await _pagoDataService.GetTotalPagadoPorFacturaAsync(idFactura, ct);
        }
    }
}