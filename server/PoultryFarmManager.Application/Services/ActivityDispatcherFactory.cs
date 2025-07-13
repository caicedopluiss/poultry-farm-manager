using System;
using System.Collections.Generic;
using System.Linq;
using PoultryFarmManager.Application.Operations.Commands.ActivityDispatchers;
using PoultryFarmManager.Core.Operations;

namespace PoultryFarmManager.Application.Services;

public interface IActivityDispatcherFactory
{
    IActivityDispatcher CreateDispatcher(ActivityType activityType);
}

public sealed class ActivityDispatcherFactory(IEnumerable<IActivityDispatcher> dispatchers) : IActivityDispatcherFactory
{
    public IActivityDispatcher CreateDispatcher(ActivityType activityType)
    {
        return activityType switch
        {
            ActivityType.WeightMeasurement => dispatchers.OfType<WeightMeasurementActivityDispatcher>().First(),
            ActivityType.Mortality => dispatchers.OfType<MortalityActivityDispatcher>().First(),
            _ => throw new NotImplementedException($"No dispatcher implemented for activity type: {activityType}"),
        };
    }
}