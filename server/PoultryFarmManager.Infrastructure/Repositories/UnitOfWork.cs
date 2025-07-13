using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application;
using PoultryFarmManager.Application.Finances;
using PoultryFarmManager.Application.Operations.Repositories;

namespace PoultryFarmManager.Infrastructure.Repositories;

public class UnitOfWork(
    ApplicationDbContext dbContext,
    IBroilerBatchRepository broilerBatchRepository,
    IFinancesRepository financesRepository,
    IActivityRepository activityRepository) : IUnitOfWork
{
    public IBroilerBatchRepository BroilerBatches => broilerBatchRepository;
    public IFinancesRepository Finances => financesRepository;
    public IActivityRepository Activities => activityRepository;

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}