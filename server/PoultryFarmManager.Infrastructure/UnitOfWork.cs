using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application;
using PoultryFarmManager.Application.Repositories;

namespace PoultryFarmManager.Infrastructure;

public class UnitOfWork(AppDbContext context, IBatchRepository batchRepository) : IUnitOfWork
{
    public IBatchRepository Batches => batchRepository;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }
}
