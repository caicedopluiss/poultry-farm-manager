using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SharedLib.CQRS;

public interface IAppRequestHandler<in TRequest, TResult>
{
    Task<IAppRequestResult<TResult>> HandleAsync(IAppRequest<TRequest> request, CancellationToken cancellationToken = default);
}

public interface IAppRequestHandler<in TRequest>
{
    Task<IAppRequestResultBase> HandleAsync(IAppRequest<TRequest> request, CancellationToken cancellationToken = default);
}


public abstract class AppRequestHandler<TArgs, TResult> : IAppRequestHandler<TArgs, TResult>
{
    protected abstract Task<IEnumerable<(string field, string error)>> ValidateAsync(TArgs args, CancellationToken cancellationToken = default);
    protected abstract Task<TResult> ExecuteAsync(TArgs args, CancellationToken cancellationToken = default);

    public async Task<IAppRequestResult<TResult>> HandleAsync(IAppRequest<TArgs> request, CancellationToken cancellationToken = default)
    {
        var validationResult = await ValidateAsync(request.Args, cancellationToken);
        if (validationResult.Any()) return new AppRequestResult<TResult>(validationResult);
        var executionResult = await ExecuteAsync(request.Args, cancellationToken);
        return new AppRequestResult<TResult>(executionResult);
    }
}

public abstract class AppRequestHandler<TArgs> : IAppRequestHandler<TArgs>
{
    protected abstract Task<IEnumerable<(string field, string error)>> ValidateAsync(TArgs args, CancellationToken cancellationToken = default);
    protected abstract Task ExecuteAsync(TArgs args, CancellationToken cancellationToken = default);

    public async Task<IAppRequestResultBase> HandleAsync(IAppRequest<TArgs> request, CancellationToken cancellationToken = default)
    {
        var validationResult = await ValidateAsync(request.Args, cancellationToken);
        if (validationResult.Any()) return new AppRequestResult<IAppRequestResultBase>(validationResult);
        await ExecuteAsync(request.Args, cancellationToken);
        return new AppRequestResultBase(true);
    }
}