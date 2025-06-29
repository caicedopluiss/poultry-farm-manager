using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Core.Operations.Models;

namespace PoultryFarmManager.Application.Operations.Repositories;

public interface IBroilerBatchRepository
{
    Task<BroilerBatch> AddAsync(BroilerBatch batch, CancellationToken cancellationToken = default);
}