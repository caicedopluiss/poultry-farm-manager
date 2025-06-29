using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.Operations.Repositories;

namespace PoultryFarmManager.Infrastructure.Repositories;

public class UnitOfWork(ApplicationDbContext dbContext, IBroilerBatchRepository broilerBatchRepository) : IUnitOfWork
{
    public IBroilerBatchRepository BroilerBatches => broilerBatchRepository;

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}