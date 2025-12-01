using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application;
using PoultryFarmManager.Application.Repositories;

namespace PoultryFarmManager.Infrastructure;

public class UnitOfWork(
    AppDbContext context, 
    IBatchesRepository batchRepository,
    IBatchActivitiesRepository batchActivitiesRepository) : IUnitOfWork
{
    public IBatchesRepository Batches => batchRepository;
    public IBatchActivitiesRepository BatchActivities => batchActivitiesRepository;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }
}
