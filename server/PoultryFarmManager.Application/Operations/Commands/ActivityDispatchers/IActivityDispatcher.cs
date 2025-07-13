using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Core.Operations.Models;

namespace PoultryFarmManager.Application.Operations.Commands.ActivityDispatchers
{
    public interface IActivityDispatcher
    {
        Task DispatchActivityAsync(Activity activity, CancellationToken cancellationToken = default);
    }
}