using System;
using System.Collections.Generic;
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
    public class ReservaService : IReservaService
    {
        private readonly IReservaDataService _reservaDataService;

        public ReservaService(IReservaDataService reservaDataService)
        {
            _reservaDataService = reservaDataService;
        }

        public async Task<ReservaDTO> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var dataModel = await _reservaDataService.GetByIdAsync(id, ct);
            if (dataModel == null)
                throw new NotFoundException("RES-001", $"No se encontró la reserva con ID {id}.");
            return dataModel.ToDto();
        }

        public async Task<ReservaDTO> GetByGuidAsync(Guid guid, CancellationToken ct = default)
        {
            var dataModel = await _reservaDataService.GetByGuidAsync(guid, ct);
            if (dataModel == null)
                throw new NotFoundException("RES-002", $"No se encontró la reserva con GUID {guid}.");
            return dataModel.ToDto();
        }

        public async Task<ReservaDTO> GetByCodigoAsync(string codigo, CancellationToken ct = default)
        {
            var dataModel = await _reservaDataService.GetByCodigoAsync(codigo, ct);
            if (dataModel == null)
                throw new NotFoundException("RES-003", $"No se encontró la reserva con código {codigo}.");
            return dataModel.ToDto();
        }

        public async Task<PagedResult<ReservaDTO>> GetByFiltroAsync(ReservaFiltroDTO filtro, int pageNumber, int pageSize, CancellationToken ct = default)
        {
            var pagedData = await _reservaDataService.GetByFiltroAsync(filtro.ToDataModel(), pageNumber, pageSize, ct);
            return new PagedResult<ReservaDTO>
            {
                Items = pagedData.Items.ToDtoList(),
                TotalCount = pagedData.TotalCount,
                PageNumber = pagedData.PageNumber,
                PageSize = pagedData.PageSize
            };
        }

        public async Task<ReservaDTO> CreateAsync(ReservaCreateDTO reservaCreateDto, CancellationToken ct = default)
        {
            var reservaDto = new ReservaDTO
            {
                IdCliente = reservaCreateDto.IdCliente,
                IdSucursal = reservaCreateDto.IdSucursal,
                FechaInicio = reservaCreateDto.FechaInicio,
                FechaFin = reservaCreateDto.FechaFin,
                SubtotalReserva = reservaCreateDto.SubtotalReserva,
                ValorIva = reservaCreateDto.ValorIva,
                TotalReserva = reservaCreateDto.TotalReserva,
                DescuentoAplicado = reservaCreateDto.DescuentoAplicado,
                SaldoPendiente = reservaCreateDto.SaldoPendiente,
                OrigenCanalReserva = reservaCreateDto.OrigenCanalReserva,
                EstadoReserva = reservaCreateDto.EstadoReserva,
                Observaciones = reservaCreateDto.Observaciones ?? string.Empty,
                EsWalkin = reservaCreateDto.EsWalkin,
                Habitaciones = reservaCreateDto.Habitaciones
            };

            ReservaValidator.Validate(reservaDto);
            var dataModel = reservaDto.ToDataModel();
            var created = await _reservaDataService.AddAsync(dataModel, ct);
            return created.ToDto();
        }

        public async Task UpdateAsync(ReservaUpdateDTO reservaUpdateDto, CancellationToken ct = default)
        {
            var reservaDto = new ReservaDTO
            {
                IdReserva = reservaUpdateDto.IdReserva,
                FechaInicio = reservaUpdateDto.FechaInicio,
                FechaFin = reservaUpdateDto.FechaFin,
                SubtotalReserva = reservaUpdateDto.SubtotalReserva,
                ValorIva = reservaUpdateDto.ValorIva,
                TotalReserva = reservaUpdateDto.TotalReserva,
                DescuentoAplicado = reservaUpdateDto.DescuentoAplicado,
                SaldoPendiente = reservaUpdateDto.SaldoPendiente,
                EstadoReserva = reservaUpdateDto.EstadoReserva,
                Observaciones = reservaUpdateDto.Observaciones ?? string.Empty
            };

            var existing = await _reservaDataService.GetByIdAsync(reservaUpdateDto.IdReserva, ct);
            if (existing == null)
                throw new NotFoundException("RES-004", $"No se encontró la reserva con ID {reservaUpdateDto.IdReserva}.");

            // Poblamos el DTO con los datos existentes que no vienen en el request para pasar la validación
            reservaDto.IdCliente = existing.IdCliente;
            reservaDto.IdSucursal = existing.IdSucursal;
            reservaDto.OrigenCanalReserva = existing.OrigenCanalReserva;

            ReservaValidator.Validate(reservaDto);

            // Solo actualizamos los campos permitidos en la actualización
            existing.FechaInicio = reservaDto.FechaInicio;
            existing.FechaFin = reservaDto.FechaFin;
            existing.SubtotalReserva = reservaDto.SubtotalReserva;
            existing.ValorIva = reservaDto.ValorIva;
            existing.TotalReserva = reservaDto.TotalReserva;
            existing.DescuentoAplicado = reservaDto.DescuentoAplicado;
            existing.SaldoPendiente = reservaDto.SaldoPendiente;
            existing.EstadoReserva = reservaDto.EstadoReserva;
            existing.Observaciones = reservaDto.Observaciones;

            await _reservaDataService.UpdateAsync(existing, ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var existing = await _reservaDataService.GetByIdAsync(id, ct);
            if (existing == null)
                throw new NotFoundException("RES-005", $"No se encontró la reserva con ID {id}.");
            await _reservaDataService.DeleteAsync(id, ct);
        }

public async Task ConfirmarAsync(int idReserva, string usuario, CancellationToken ct = default)
        {
            var existing = await _reservaDataService.GetByIdAsync(idReserva, ct);
            if (existing == null)
                throw new NotFoundException("RES-006", $"No se encontró la reserva con ID {idReserva}.");

            if (existing.EstadoReserva == "CON")
                throw new ConflictException("La reserva ya está confirmada.");

            if (existing.EstadoReserva != "PEN")
                throw new ConflictException($"No se puede confirmar una reserva en estado '{existing.EstadoReserva}'.");

            if (existing.Habitaciones == null || existing.Habitaciones.Count == 0)
                throw new ValidationException("RES-010", "La reserva no tiene habitaciones asociadas.");

            var habitacionesConConflicto = new HashSet<int>();
            foreach (var detalle in existing.Habitaciones)
            {
                var solapa = await _reservaDataService.ExisteSolapamientoAsync(
                    detalle.IdHabitacion,
                    detalle.FechaInicio,
                    detalle.FechaFin,
                    excludeIdReserva: idReserva,
                    ct: ct);

                if (solapa)
                    habitacionesConConflicto.Add(detalle.IdHabitacion);
            }

            if (habitacionesConConflicto.Count > 0)
            {
                var ids = string.Join(", ", habitacionesConConflicto);
                throw new ConflictException($"No se puede confirmar la reserva: las habitaciones {ids} ya están ocupadas en el rango {existing.FechaInicio:yyyy-MM-dd} a {existing.FechaFin:yyyy-MM-dd}.");
            }

            await _reservaDataService.ConfirmarAsync(idReserva, usuario, ct);
        }

public async Task CancelarAsync(int idReserva, string motivo, string usuario, CancellationToken ct = default)
        {
            var existing = await _reservaDataService.GetByIdAsync(idReserva, ct);
            if (existing == null)
                throw new NotFoundException("RES-007", $"No se encontró la reserva con ID {idReserva}.");

            if (existing.EstadoReserva == "CAN")
                throw new ConflictException("La reserva ya está cancelada.");

            if (existing.EstadoReserva != "PEN" && existing.EstadoReserva != "CON")
                throw new ConflictException($"No se puede cancelar una reserva en estado '{existing.EstadoReserva}'.");

            await _reservaDataService.CancelarAsync(idReserva, motivo, usuario, ct);
        }

        public async Task FinalizarAsync(int idReserva, string usuario, CancellationToken ct = default)
        {
            var existing = await _reservaDataService.GetByIdAsync(idReserva, ct);
            if (existing == null)
                throw new NotFoundException("RES-008", $"No se encontró la reserva con ID {idReserva}.");
            await _reservaDataService.FinalizarAsync(idReserva, usuario, ct);
        }

        public async Task<bool> PuedeCancelarAsync(int idReserva, CancellationToken ct = default)
        {
            return await _reservaDataService.PuedeCancelarAsync(idReserva, ct);
        }

        public async Task<int> ConfirmarReservaHabitacionAsync(int idReserva, int idHabitacion, int? idTarifa, DateTime fechaInicio, DateTime fechaFin, int numAdultos, int numNinos, decimal precioNoche, string usuario, CancellationToken ct = default)
        {
            if (fechaFin <= fechaInicio)
                throw new ValidationException("RES-009", "La fecha de fin debe ser posterior a la fecha de inicio.");
            return await _reservaDataService.ConfirmarReservaHabitacionAsync(idReserva, idHabitacion, idTarifa, fechaInicio, fechaFin, numAdultos, numNinos, precioNoche, usuario, ct);
        }
    }
}
