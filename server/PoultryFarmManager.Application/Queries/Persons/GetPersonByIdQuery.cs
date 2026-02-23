using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;

namespace PoultryFarmManager.Application.Queries.Persons;

public sealed class GetPersonByIdQuery
{
    public record Args(Guid PersonId);
    public record Result(PersonDto? Person);

    public sealed class Handler(IUnitOfWork unitOfWork) : AppRequestHandler<Args, Result>
    {
        protected override async Task<Result> ExecuteAsync(Args args, CancellationToken cancellationToken = default)
        {
            var person = await unitOfWork.Persons.GetByIdAsync(args.PersonId, cancellationToken: cancellationToken);

            if (person == null) return new Result(null);

            var personDto = new PersonDto().Map(person);

            return new Result(personDto);
        }

        protected override Task<IEnumerable<(string field, string error)>> ValidateAsync(Args args, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Enumerable.Empty<(string field, string error)>());
        }
    }
}
