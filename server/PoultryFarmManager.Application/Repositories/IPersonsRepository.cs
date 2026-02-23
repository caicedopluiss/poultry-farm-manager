using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Core.Models.Finance;

namespace PoultryFarmManager.Application.Repositories;

public interface IPersonsRepository
{
    Task<IReadOnlyCollection<Person>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Person?> GetByIdAsync(Guid id, bool track = false, CancellationToken cancellationToken = default);
    Task<Person> CreateAsync(Person person, CancellationToken cancellationToken = default);
    Task<Person> UpdateAsync(Person person, CancellationToken cancellationToken = default);
}
