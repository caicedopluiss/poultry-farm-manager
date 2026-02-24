using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PoultryFarmManager.Application.Repositories;
using PoultryFarmManager.Core.Models.Finance;

namespace PoultryFarmManager.Infrastructure.Repositories;

public sealed class PersonsRepository(AppDbContext context) : IPersonsRepository
{
    public Task<Person> CreateAsync(Person person, CancellationToken cancellationToken = default)
    {
        var createdPerson = context.Persons.Add(person).Entity;
        return Task.FromResult(createdPerson);
    }

    public async Task<IReadOnlyCollection<Person>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var persons = await context.Persons
            .AsNoTracking()
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .ToListAsync(cancellationToken);
        return persons;
    }

    public async Task<Person?> GetByIdAsync(Guid id, bool track = false, CancellationToken cancellationToken = default)
    {
        var query = context.Persons.AsQueryable();

        if (!track)
        {
            query = query.AsNoTracking();
        }

        var person = await query.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        return person;
    }

    public Task<Person> UpdateAsync(Person person, CancellationToken cancellationToken = default)
    {
        var updatedPerson = context.Persons.Update(person).Entity;
        return Task.FromResult(updatedPerson);
    }
}
