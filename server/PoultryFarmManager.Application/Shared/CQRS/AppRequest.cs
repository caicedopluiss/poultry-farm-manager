namespace PoultryFarmManager.Application.Shared.CQRS;

public interface IAppRequest<out TArgs>
{
    TArgs Args { get; }
}

public record AppRequest<TArgs>(TArgs Args) : IAppRequest<TArgs>;
