using System;

namespace PoultryFarmManager.Core;

public interface IDbEntity
{
    Guid Id { get; set; }
}
