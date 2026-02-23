using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application;
using PoultryFarmManager.Application.Repositories;

namespace PoultryFarmManager.Infrastructure;

public class UnitOfWork(
    AppDbContext context,
    IBatchesRepository batchRepository,
    IBatchActivitiesRepository batchActivitiesRepository,
    IAssetsRepository assetsRepository,
    IProductsRepository productsRepository,
    IProductVariantsRepository productVariantsRepository,
    ITransactionsRepository transactionsRepository,
    IVendorsRepository vendorsRepository,
    IPersonsRepository personsRepository) : IUnitOfWork
{
    public IBatchesRepository Batches => batchRepository;
    public IBatchActivitiesRepository BatchActivities => batchActivitiesRepository;
    public IAssetsRepository Assets => assetsRepository;
    public IProductsRepository Products => productsRepository;
    public IProductVariantsRepository ProductVariants => productVariantsRepository;
    public ITransactionsRepository Transactions => transactionsRepository;
    public IVendorsRepository Vendors => vendorsRepository;
    public IPersonsRepository Persons => personsRepository;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }
}
