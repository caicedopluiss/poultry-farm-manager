using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.Repositories;

namespace PoultryFarmManager.Application;

public interface IUnitOfWork
{
    IBatchesRepository Batches { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
