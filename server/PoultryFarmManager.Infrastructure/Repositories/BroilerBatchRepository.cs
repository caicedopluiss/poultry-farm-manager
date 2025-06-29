using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.Operations.Repositories;
using PoultryFarmManager.Core.Operations.Models;

namespace PoultryFarmManager.Infrastructure.Repositories;

public class BroilerBatchRepository(ApplicationDbContext dbContext) : IBroilerBatchRepository
{
    public async Task<BroilerBatch> AddAsync(BroilerBatch batch, CancellationToken cancellationToken = default)
    {
        var result = (await dbContext.BroilerBatches.AddAsync(batch, cancellationToken)).Entity;

        return result;
    }
}