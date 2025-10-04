using PoultryFarmManager.Core;

namespace PoultryFarmManager.Tests.Integration;

internal interface IEntityFactory<T> where T : IDbEntity
{
    T CreateRandom();
}
