using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Servicio.Hotel.DataManagement.Alojamiento.Models;

namespace Servicio.Hotel.DataManagement.Alojamiento.Interfaces
{
    public interface ISucursalDataService
    {
        Task<SucursalDataModel?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<SucursalDataModel?> GetByGuidAsync(Guid guid, CancellationToken ct = default);
        Task<IEnumerable<SucursalDataModel>> GetAllAsync(CancellationToken ct = default);
        Task<SucursalDataModel> AddAsync(SucursalDataModel model, CancellationToken ct = default);
        Task UpdateAsync(SucursalDataModel model, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);
    }
}
