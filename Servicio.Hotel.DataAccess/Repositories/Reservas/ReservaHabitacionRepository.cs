using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Servicio.Hotel.DataAccess.Context;
using Servicio.Hotel.DataAccess.Entities.Reservas;
using Servicio.Hotel.DataAccess.Repositories.Interfaces.Reservas;

namespace Servicio.Hotel.DataAccess.Repositories.Reservas
{
    public class ReservaHabitacionRepository : RepositoryBase<ReservaHabitacionEntity>, IReservaHabitacionRepository
    {
        public ReservaHabitacionRepository(ServicioHotelDbContext context) : base(context) { }

        public async Task<ReservaHabitacionEntity?> GetByIdAsync(int id, CancellationToken ct = default)
            => await base.GetByIdAsync(id, ct);

        public async Task<IEnumerable<ReservaHabitacionEntity>> GetAllAsync(CancellationToken ct = default)
            => await base.GetAllAsync(ct);

        public async Task<ReservaHabitacionEntity> AddAsync(ReservaHabitacionEntity entity, CancellationToken ct = default)
            => await base.AddAsync(entity, ct);

        public async Task UpdateAsync(ReservaHabitacionEntity entity, CancellationToken ct = default)
            => await base.UpdateAsync(entity, ct);

        public async Task DeleteAsync(int id, CancellationToken ct = default)
            => await base.DeleteAsync(id, ct);

        public async Task<ReservaHabitacionEntity?> GetByGuidAsync(Guid guid, CancellationToken ct = default)
            => await _dbSet.FirstOrDefaultAsync(rh => rh.ReservaHabitacionGuid == guid, ct);

        public async Task UpdateEstadoDetalleAsync(int idReservaHabitacion, string nuevoEstado, string usuario, CancellationToken ct = default)
        {
            var detalle = await GetByIdAsync(idReservaHabitacion, ct);
            if (detalle != null)
            {
                detalle.EstadoDetalle = nuevoEstado;
                detalle.ModificadoPorUsuario = usuario;
                detalle.FechaModificacionUtc = DateTime.UtcNow;
                await UpdateAsync(detalle, ct);
            }
        }
    }
}