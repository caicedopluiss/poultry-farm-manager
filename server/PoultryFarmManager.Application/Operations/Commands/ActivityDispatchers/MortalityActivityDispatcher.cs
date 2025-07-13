using System;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Core.Operations.Models;

namespace PoultryFarmManager.Application.Operations.Commands.ActivityDispatchers;

public sealed class MortalityActivityDispatcher(IUnitOfWork unitOfWork) : IActivityDispatcher
{
    public async Task DispatchActivityAsync(Activity activity, CancellationToken cancellationToken = default)
    {
        var batch = await unitOfWork.BroilerBatches.GetByIdAsync(activity.BroilerBatchId, track: true, cancellationToken: cancellationToken);
        if (batch is null) return;

        // Update the current population of the batch based on the mortality activity
        batch.CurrentPopulation -= Convert.ToInt32(activity.Value ?? 0);
        // Ensure the current population does not go below zero
        if (batch.CurrentPopulation < 0) batch.CurrentPopulation = 0;

        // Not needed to call UpdateAsync here, as the entity is already tracked by context
    }
}