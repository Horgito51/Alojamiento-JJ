using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Servicio.Hotel.DataAccess.Repositories.Interfaces.Reservas;
using Servicio.Hotel.DataManagement.Reservas.Interfaces;
using Servicio.Hotel.DataManagement.Reservas.Models;
using Servicio.Hotel.DataManagement.Reservas.Mappers;
using Servicio.Hotel.DataManagement.Common;
using Servicio.Hotel.DataManagement.UnitOfWork;

namespace Servicio.Hotel.DataManagement.Reservas.Services
{
    public class ReservaDataService : IReservaDataService
    {
        private readonly IReservaRepository _reservaRepository;
        private readonly IReservaHabitacionRepository _reservaHabitacionRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ReservaDataService(IReservaRepository reservaRepository, IReservaHabitacionRepository reservaHabitacionRepository, IUnitOfWork unitOfWork)
        {
            _reservaRepository = reservaRepository;
            _reservaHabitacionRepository = reservaHabitacionRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ReservaDataModel> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var entity = await _reservaRepository.GetByIdAsync(id, ct);
            return entity?.ToModel();
        }

        public async Task<ReservaDataModel> GetByGuidAsync(Guid guid, CancellationToken ct = default)
        {
            var entity = await _reservaRepository.GetByGuidAsync(guid, ct);
            return entity?.ToModel();
        }

        public async Task<ReservaDataModel> GetByCodigoAsync(string codigo, CancellationToken ct = default)
        {
            var entity = await _reservaRepository.GetByCodigoAsync(codigo, ct);
            return entity?.ToModel();
        }

        public async Task<DataPagedResult<ReservaDataModel>> GetByFiltroAsync(ReservaFiltroDataModel filtro, int pageNumber, int pageSize, CancellationToken ct = default)
        {
            var all = await _reservaRepository.GetAllAsync(ct);
            var query = all.AsQueryable();

            if (filtro.IdCliente.HasValue)
                query = query.Where(r => r.IdCliente == filtro.IdCliente.Value);
            if (filtro.IdSucursal.HasValue)
                query = query.Where(r => r.IdSucursal == filtro.IdSucursal.Value);
            if (!string.IsNullOrEmpty(filtro.EstadoReserva))
                query = query.Where(r => r.EstadoReserva == filtro.EstadoReserva);
            if (filtro.FechaInicioDesde.HasValue)
                query = query.Where(r => r.FechaInicio >= filtro.FechaInicioDesde.Value);
            if (filtro.FechaInicioHasta.HasValue)
                query = query.Where(r => r.FechaInicio <= filtro.FechaInicioHasta.Value);
            if (!string.IsNullOrEmpty(filtro.CodigoReserva))
                query = query.Where(r => r.CodigoReserva.Contains(filtro.CodigoReserva));
            if (filtro.EsWalkin.HasValue)
                query = query.Where(r => r.EsWalkin == filtro.EsWalkin.Value);
            if (filtro.EsEliminado.HasValue)
                query = query.Where(r => r.EsEliminado == filtro.EsEliminado.Value);

            var totalCount = query.Count();
            var items = query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            return new DataPagedResult<ReservaDataModel>
            {
                Items = items.ToModelList(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<ReservaDataModel> AddAsync(ReservaDataModel model, CancellationToken ct = default)
        {
            var entity = model.ToEntity();
            if (entity.GuidReserva == Guid.Empty) entity.GuidReserva = Guid.NewGuid();
            if (string.IsNullOrWhiteSpace(entity.CreadoPorUsuario)) entity.CreadoPorUsuario = "Sistema";
            if (string.IsNullOrWhiteSpace(entity.ServicioOrigen)) entity.ServicioOrigen = "reservas-service";
            if (string.IsNullOrWhiteSpace(entity.CodigoReserva))
                entity.CodigoReserva = $"RES-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
            entity.FechaRegistroUtc = DateTime.UtcNow;
            entity.FechaReservaUtc = DateTime.UtcNow;
            if (entity.ReservasHabitaciones != null)
            {
                foreach (var rh in entity.ReservasHabitaciones)
                {
                    if (rh.ReservaHabitacionGuid == Guid.Empty) rh.ReservaHabitacionGuid = Guid.NewGuid();
                    if (string.IsNullOrWhiteSpace(rh.CreadoPorUsuario)) rh.CreadoPorUsuario = "Sistema";
                    if (string.IsNullOrWhiteSpace(rh.ServicioOrigen)) rh.ServicioOrigen = "reservas-service";
                    rh.FechaRegistroUtc = DateTime.UtcNow;
                }
            }
            var added = await _reservaRepository.AddAsync(entity, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return added.ToModel();
        }

        public async Task UpdateAsync(ReservaDataModel model, CancellationToken ct = default)
        {
            var entity = await _reservaRepository.GetByIdAsync(model.IdReserva, ct);
            if (entity == null) return;

            // Actualizamos solo los campos que pueden cambiar
            entity.FechaInicio = model.FechaInicio;
            entity.FechaFin = model.FechaFin;
            entity.SubtotalReserva = model.SubtotalReserva;
            entity.ValorIva = model.ValorIva;
            entity.TotalReserva = model.TotalReserva;
            entity.DescuentoAplicado = model.DescuentoAplicado;
            entity.SaldoPendiente = model.SaldoPendiente;
            entity.EstadoReserva = model.EstadoReserva;
            entity.Observaciones = model.Observaciones;
            entity.ModificadoPorUsuario = model.ModificadoPorUsuario ?? "Sistema";
            entity.FechaModificacionUtc = DateTime.UtcNow;

            await _reservaRepository.UpdateAsync(entity, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            await _reservaRepository.DeleteAsync(id, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }

        public async Task ConfirmarAsync(int idReserva, string usuario, CancellationToken ct = default)
        {
            await _reservaRepository.ConfirmarAsync(idReserva, usuario, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }

        public async Task CancelarAsync(int idReserva, string motivo, string usuario, CancellationToken ct = default)
        {
            await _reservaRepository.CancelarAsync(idReserva, motivo, usuario, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }

        public async Task FinalizarAsync(int idReserva, string usuario, CancellationToken ct = default)
        {
            await _reservaRepository.FinalizarAsync(idReserva, usuario, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }

        public async Task<bool> PuedeCancelarAsync(int idReserva, CancellationToken ct = default)
        {
            return await _reservaRepository.PuedeCancelarAsync(idReserva, ct);
        }

        public async Task<int> ConfirmarReservaHabitacionAsync(int idReserva, int idHabitacion, int? idTarifa, DateTime fechaInicio, DateTime fechaFin, int numAdultos, int numNinos, decimal precioNoche, string usuario, CancellationToken ct = default)
        {
            var result = await _reservaRepository.ConfirmarReservaHabitacionAsync(idReserva, idHabitacion, idTarifa, fechaInicio, fechaFin, numAdultos, numNinos, precioNoche, usuario, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return result;
        }
    }
}