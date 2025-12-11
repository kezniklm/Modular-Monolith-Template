using Microsoft.EntityFrameworkCore;
using Module.Application.Services;

namespace Module.Infrastructure.Persistence.Services;

public class EfCoreQueryObject<TAggregate>(ModuleDbContext dbContext)
    : QueryObject<TAggregate>(dbContext.Set<TAggregate>().AsQueryable())
    where TAggregate : class
{
#pragma warning disable CA1823 // Avoid unused private fields
    private readonly ModuleDbContext _dbContext = dbContext;
#pragma warning restore CA1823 // Avoid unused private fields

    public override async Task<IEnumerable<TAggregate>> ExecuteAsync() => await Query.ToListAsync();
}
