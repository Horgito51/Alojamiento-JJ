using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Servicio.Hotel.DataAccess.Repositories.Interfaces.Alojamiento;
using Servicio.Hotel.DataManagement.Alojamiento.Interfaces;
using Servicio.Hotel.DataManagement.Alojamiento.Mappers;
using Servicio.Hotel.DataManagement.Alojamiento.Models;
using Servicio.Hotel.DataManagement.UnitOfWork;

namespace Servicio.Hotel.DataManagement.Alojamiento.Services
{
    public class CatalogoServicioDataService : ICatalogoServicioDataService
    {
        private readonly ICatalogoServicioRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public CatalogoServicioDataService(ICatalogoServicioRepository repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<CatalogoServicioDataModel?> GetByIdAsync(int id, CancellationToken ct = default)
            => (await _repository.GetByIdAsync(id, ct)).ToModel();

        public async Task<CatalogoServicioDataModel?> GetByGuidAsync(Guid guid, CancellationToken ct = default)
            => (await _repository.GetByGuidAsync(guid, ct)).ToModel();

        public async Task<IEnumerable<CatalogoServicioDataModel>> GetAllAsync(CancellationToken ct = default)
            => (await _repository.GetAllAsync(ct)).ToModelList();

        public async Task<CatalogoServicioDataModel> AddAsync(CatalogoServicioDataModel model, CancellationToken ct = default)
        {
            var entity = await _repository.AddAsync(model.ToEntity()!, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return entity.ToModel()!;
        }

        public async Task UpdateAsync(CatalogoServicioDataModel model, CancellationToken ct = default)
        {
            await _repository.UpdateAsync(model.ToEntity()!, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            await _repository.DeleteAsync(id, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }
    }
}
