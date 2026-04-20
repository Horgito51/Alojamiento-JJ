using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Servicio.Hotel.Business.DTOs.Alojamiento;

namespace Servicio.Hotel.Business.Interfaces.Alojamiento
{
    public interface ISucursalService
    {
        Task<IEnumerable<SucursalDTO>> GetAllAsync(CancellationToken ct = default);
        Task<SucursalDTO> GetByIdAsync(int id, CancellationToken ct = default);
        Task<SucursalDTO> CreateAsync(SucursalCreateDTO dto, CancellationToken ct = default);
        Task UpdateAsync(SucursalUpdateDTO dto, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);
    }
}
