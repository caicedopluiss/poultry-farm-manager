using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.Repositories;

namespace PoultryFarmManager.Application;

public interface IUnitOfWork
{
    IBatchesRepository Batches { get; }
    IBatchActivitiesRepository BatchActivities { get; }
    IAssetsRepository Assets { get; }
    IProductsRepository Products { get; }
    IProductVariantsRepository ProductVariants { get; }
    ITransactionsRepository Transactions { get; }
    IVendorsRepository Vendors { get; }
    IPersonsRepository Persons { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
