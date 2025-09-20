using System;

namespace PoultryFarmManager.Core;

public abstract class DbEntity : IDbEntity
{
    public Guid Id { get; set; }
}
