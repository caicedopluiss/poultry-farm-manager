using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Enums;

namespace PoultryFarmManager.Application.Commands.Batches;

public sealed class CreateBatchCommand
{
    public record Args(NewBatchDto NewBatch);
    public record Result(BatchDto CreatedBatch);

    public sealed class Handler(IUnitOfWork unitOfWork) : AppRequestHandler<Args, Result>
    {
        protected override async Task<Result> ExecuteAsync(Args args, CancellationToken cancellationToken = default)
        {
            var batch = args.NewBatch.Map(args.NewBatch);
            batch.InitialPopulation = batch.Population;
            batch.Status = batch.StartDate > DateTime.UtcNow ? BatchStatus.Planned : BatchStatus.Active;

            var createdBatch = await unitOfWork.Batches.CreateAsync(batch, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var batchDto = new BatchDto().Map(createdBatch);
            var result = new Result(batchDto);

            return result;
        }

        protected override Task<IEnumerable<(string field, string error)>> ValidateAsync(Args args, CancellationToken cancellationToken = default)
        {
            var errors = new List<(string field, string error)>();

            if (string.IsNullOrWhiteSpace(args.NewBatch.Name))
            {
                errors.Add(("name", "Name is required."));
            }
            else if (args.NewBatch.Name.Length > 100)
            {
                errors.Add(("name", "Name cannot exceed 100 characters."));
            }

            if (!string.IsNullOrWhiteSpace(args.NewBatch.Breed) && args.NewBatch.Breed.Length > 100)
            {
                errors.Add(("breed", "Breed cannot exceed 100 characters."));
            }

            if (!Utils.IsIso8601DateStringValid(args.NewBatch.StartClientDateIsoString))
            {
                errors.Add(("startClientDateIsoString", "Start date is not a valid ISO 8601 date string."));
            }

            if (args.NewBatch.MaleCount < 0)
            {
                errors.Add(("maleCount", "Male count cannot be negative."));
            }

            if (args.NewBatch.FemaleCount < 0)
            {
                errors.Add(("femaleCount", "Female count cannot be negative."));
            }

            if (args.NewBatch.UnsexedCount < 0)
            {
                errors.Add(("unsexedCount", "Unsexed count cannot be negative."));
            }

            if (args.NewBatch.MaleCount + args.NewBatch.FemaleCount + args.NewBatch.UnsexedCount <= 0)
            {
                errors.Add(("population", "Batch population must be greater than zero."));
            }

            return Task.FromResult<IEnumerable<(string field, string error)>>(errors);
        }
    }
}
