using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace PoultryFarmManager.Application.Shared.CQRS;

public interface IAppRequestsMediator
{
    Task<IAppRequestResultBase> SendAsync<TArgs>(IAppRequest<TArgs> request, CancellationToken cancellationToken = default);
    Task<IAppRequestResult<TResult>> SendAsync<TArgs, TResult>(IAppRequest<TArgs> request, CancellationToken cancellationToken = default);
}

public class AppRequestsMediator(IServiceProvider serviceProvider) : IAppRequestsMediator
{
    public async Task<IAppRequestResultBase> SendAsync<TArgs>(IAppRequest<TArgs> request, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IAppRequestHandler<TArgs>>();
        return await handler.HandleAsync(request, cancellationToken);
    }

    public async Task<IAppRequestResult<TResult>> SendAsync<TArgs, TResult>(IAppRequest<TArgs> request, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IAppRequestHandler<TArgs, TResult>>();
        return await handler.HandleAsync(request, cancellationToken);
    }
}
