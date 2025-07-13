using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.Operations.DTOs;
using PoultryFarmManager.Application.Services;
using PoultryFarmManager.Core.Operations;
using SharedLib.CQRS;

namespace PoultryFarmManager.Application.Operations.Commands;

public class AddActivityCommand
{
    public record Args(NewActivityDto NewActivity);
    public record Result(ActivityDto Activity);

    public class Handler(IUnitOfWork unitOfWork, IActivityDispatcherFactory activityDispatcherFactory) : AppRequestHandler<Args, Result>
    {
        protected override async Task<Result> ExecuteAsync(Args args, CancellationToken cancellationToken = default)
        {
            var activity = args.NewActivity.ToCore();

            // Add activity to the unit of work
            var createdActivity = await unitOfWork.Activities.AddActivityAsync(activity, cancellationToken);
            // Trigger the appropriate dispatcher based on activity type
            // This will handle the specific logic for each activity type
            var dispatcher = activityDispatcherFactory.CreateDispatcher(createdActivity.Type);
            await dispatcher.DispatchActivityAsync(createdActivity, cancellationToken);
            // Save all changes to the database
            await unitOfWork.SaveChangesAsync(cancellationToken);
            var result = ActivityDto.FromCore(createdActivity);

            return new Result(result);
        }

        protected override async Task<IEnumerable<(string field, string error)>> ValidateAsync(Args args, CancellationToken cancellationToken = default)
        {
            var errors = new List<(string field, string error)>();

            if (args.NewActivity.BroilerBatchId == default)
            {
                errors.Add(("broilerBatchId", "BroilerBatchId is required."));
            }
            else
            {
                var batch = await unitOfWork.BroilerBatches.GetByIdAsync(args.NewActivity.BroilerBatchId, cancellationToken: cancellationToken);
                if (batch is null) errors.Add(("broilerBatchId", "Broiler Batch not found."));
            }

            if (string.IsNullOrWhiteSpace(args.NewActivity.Date) || !Utils.IsIso8601DateStringValid(args.NewActivity.Date))
            {
                errors.Add(("date", "Invalid date format."));
            }

            if (string.IsNullOrWhiteSpace(args.NewActivity.Type) || !Enum.TryParse<ActivityType>(args.NewActivity.Type, true, out var activityType) || !Enum.IsDefined(typeof(ActivityType), activityType))
            {
                errors.Add(("type", "Invalid activity type."));
            }

            if (!args.NewActivity.Value.HasValue && args.NewActivity.Unit is not null)
            {
                errors.Add(("value", "Value is required if Unit is provided."));
            }

            if (args.NewActivity.Value.HasValue && args.NewActivity.Value < 0)
            {
                errors.Add(("value", "Value cannot be negative."));
            }
            else
            {
                //Validate value size

                if (args.NewActivity.Value.HasValue && (args.NewActivity.Unit is null || string.IsNullOrWhiteSpace(args.NewActivity.Unit)))
                {
                    errors.Add(("unit", "Unit is required if Value is provided."));
                }
            }

            if (args.NewActivity.Unit?.Length > 10)
            {
                errors.Add(("unit", "Unit cannot exceed 10 characters."));
            }

            if (args.NewActivity.Description is not null && string.IsNullOrWhiteSpace(args.NewActivity.Description))
            {
                errors.Add(("description", "Description cannot be empty if provided."));
            }
            else if (args.NewActivity.Description?.Length > 500)
            {
                errors.Add(("description", "Description cannot exceed 500 characters."));
            }

            return errors;
        }
    }
}