using System;
using System.Threading;
using System.Threading.Tasks;
using Servicio.Hotel.Business.Common;
using Servicio.Hotel.Business.DTOs.Facturacion;
using Servicio.Hotel.Business.Exceptions;
using Servicio.Hotel.Business.Interfaces.Facturacion;
using Servicio.Hotel.Business.Interfaces.Reservas;
using Servicio.Hotel.Business.Mappers.Facturacion;
using Servicio.Hotel.Business.Validators.Facturacion;
using Servicio.Hotel.DataManagement.Facturacion.Interfaces;

namespace Servicio.Hotel.Business.Services.Facturacion
{
    public class PagoService : IPagoService
    {
        private readonly IPagoDataService _pagoDataService;
        private readonly IReservaService _reservaService;

        public PagoService(IPagoDataService pagoDataService, IReservaService reservaService)
        {
            _pagoDataService = pagoDataService;
            _reservaService = reservaService;
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

        public async Task<PagoSimuladoDTO> SimularPagoAsync(int idReserva, decimal? monto, string usuario, CancellationToken ct = default)
        {
            if (idReserva <= 0)
                throw new ValidationException("PAG-005", "El idReserva es obligatorio.");

            var reserva = await _reservaService.GetByIdAsync(idReserva, ct);
            var montoFinal = monto ?? reserva.SaldoPendiente;

            if (montoFinal <= 0)
                throw new ValidationException("PAG-006", "El monto del pago debe ser mayor a cero.");

            await _reservaService.UpdateAsync(new Servicio.Hotel.Business.DTOs.Reservas.ReservaUpdateDTO
            {
                IdReserva = reserva.IdReserva,
                FechaInicio = reserva.FechaInicio,
                FechaFin = reserva.FechaFin,
                SubtotalReserva = reserva.SubtotalReserva,
                ValorIva = reserva.ValorIva,
                TotalReserva = reserva.TotalReserva,
                DescuentoAplicado = reserva.DescuentoAplicado,
                SaldoPendiente = 0,
                EstadoReserva = "CON",
                Observaciones = reserva.Observaciones
            }, ct);

            return new PagoSimuladoDTO
            {
                IdReserva = reserva.IdReserva,
                CodigoReserva = reserva.CodigoReserva,
                Monto = montoFinal,
                EstadoPago = "APR",
                EstadoReserva = "CON",
                TransaccionExterna = $"SIM-{Guid.NewGuid():N}",
                CodigoAutorizacion = Guid.NewGuid().ToString("N")[..10].ToUpperInvariant(),
                Mensaje = "Pago realizado con exito.",
                FechaPagoUtc = DateTime.UtcNow
            };
        }
    }
}
