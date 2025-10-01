using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.Repositories;

namespace PoultryFarmManager.Application;

public interface IUnitOfWork
{
    IBatchRepository Batches { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
