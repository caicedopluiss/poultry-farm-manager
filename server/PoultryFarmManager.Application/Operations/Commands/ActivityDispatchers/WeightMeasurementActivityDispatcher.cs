using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Core.Operations.Models;

namespace PoultryFarmManager.Application.Operations.Commands.ActivityDispatchers;

public sealed class WeightMeasurementActivityDispatcher : IActivityDispatcher
{
    public Task DispatchActivityAsync(Activity activity, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask; // No specific logic for weight measurement, just a placeholder
    }
}