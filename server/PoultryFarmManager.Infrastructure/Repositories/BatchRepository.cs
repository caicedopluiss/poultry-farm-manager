using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.Repositories;
using PoultryFarmManager.Core.Models;

namespace PoultryFarmManager.Infrastructure.Repositories;

public sealed class BatchRepository(AppDbContext context) : IBatchRepository
{
    public Task<Batch> CreateAsync(Batch batch, CancellationToken cancellationToken = default)
    {
        var createdBatch = context.Batches.Add(batch).Entity;
        return Task.FromResult(createdBatch);
    }
}
