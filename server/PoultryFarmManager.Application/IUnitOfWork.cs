using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.Finances;
using PoultryFarmManager.Application.Operations.Repositories;

namespace PoultryFarmManager.Application;

public interface IUnitOfWork
{
    IBroilerBatchRepository BroilerBatches { get; }
    IFinancesRepository Finances { get; }
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}