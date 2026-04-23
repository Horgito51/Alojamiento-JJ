using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Servicio.Hotel.DataAccess.Context;
using Servicio.Hotel.DataAccess.Entities.Facturacion;
using Servicio.Hotel.DataAccess.Repositories.Interfaces.Facturacion;

namespace Servicio.Hotel.DataAccess.Repositories.Facturacion
{
    public class FacturaRepository : RepositoryBase<FacturaEntity>, IFacturaRepository
    {
        public FacturaRepository(ServicioHotelDbContext context) : base(context) { }

        public async Task<FacturaEntity?> GetByIdAsync(int id, CancellationToken ct = default)
            => await base.GetByIdAsync(id, ct);

        public async Task<FacturaEntity?> GetByGuidAsync(Guid guid, CancellationToken ct = default)
            => await _dbSet.FirstOrDefaultAsync(f => f.GuidFactura == guid, ct);

        public async Task<IEnumerable<FacturaEntity>> GetAllAsync(CancellationToken ct = default)
            => await base.GetAllAsync(ct);

        public async Task<FacturaEntity> AddAsync(FacturaEntity entity, CancellationToken ct = default)
            => await base.AddAsync(entity, ct);

        public async Task UpdateAsync(FacturaEntity entity, CancellationToken ct = default)
            => await base.UpdateAsync(entity, ct);

        public async Task DeleteAsync(int id, CancellationToken ct = default)
            => await base.DeleteAsync(id, ct);

        public async Task UpdateSaldoPendienteAsync(int idFactura, decimal nuevoSaldo, CancellationToken ct = default)
        {
            var factura = await GetByIdAsync(idFactura, ct);
            if (factura != null)
            {
                factura.SaldoPendiente = nuevoSaldo;
                if (nuevoSaldo == 0) factura.Estado = "PAG";
                await UpdateAsync(factura, ct);
            }
        }

        public async Task AnularAsync(int idFactura, string motivo, string usuario, CancellationToken ct = default)
        {
            var factura = await GetByIdAsync(idFactura, ct);
            if (factura != null)
            {
                factura.Estado = "ANU";
                factura.MotivoInhabilitacion = motivo;
                factura.ModificadoPorUsuario = usuario;
                factura.FechaModificacionUtc = DateTime.UtcNow;
                await UpdateAsync(factura, ct);
            }
        }

        public async Task<bool> EstaPagadaAsync(int idFactura, CancellationToken ct = default)
        {
            var factura = await GetByIdAsync(idFactura, ct);
            return factura != null && factura.Estado == "PAG";
        }
        // Estos métodos ejecutan los stored procedures que ya tienes en la base de datos.
        // Ajusta los nombres de los SP según lo que tengas.

public async Task<int> GenerarFacturaReservaAsync(int idReserva, string usuario, CancellationToken ct = default)
        {
            var reserva = await _context.Reservas.AsNoTracking().FirstOrDefaultAsync(r => r.IdReserva == idReserva, ct);
            if (reserva == null)
                throw new KeyNotFoundException();

            var sql = "EXEC booking.SP_GENERAR_FACTURA_RESERVA @id_reserva = {0}, @usuario = {1}";
            var result = await _context.Database.SqlQueryRaw<int?>(sql, idReserva, usuario).ToListAsync(ct);
            return result.FirstOrDefault() ?? 0;
        }

        public async Task<int> GenerarFacturaFinalAsync(int idReserva, string usuario, CancellationToken ct = default)
        {
            var sql = "EXEC booking.SP_GENERAR_FACTURA_FINAL @id_reserva = {0}, @usuario = {1}";
            var result = await _context.Database.SqlQueryRaw<int?>(sql, idReserva, usuario).ToListAsync(ct);
            return result.FirstOrDefault() ?? 0;
        }
    }
}
