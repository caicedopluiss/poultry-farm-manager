using System.Threading;
using System.Threading.Tasks;

namespace PoultryFarmManager.Application.Operations.Repositories;

public interface IUnitOfWork
{
    IBroilerBatchRepository BroilerBatches { get; }
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}