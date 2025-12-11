using Microsoft.EntityFrameworkCore;
using Module.Application.Services;

namespace Module.Infrastructure.Persistence.Services;

public class EfCoreRepository<TAggregate>(ModuleDbContext dbContext) : IRepository<TAggregate> where TAggregate : class
{
    protected ModuleDbContext Context { get; } = dbContext;
    protected DbSet<TAggregate> DbSet { get; } = dbContext.Set<TAggregate>();

    public async Task<TAggregate> InsertAsync(TAggregate entity)
    {
        await DbSet.AddAsync(entity);
        return entity;
    }

    public async Task InsertRangeAsync(IReadOnlyList<TAggregate> entity) => await DbSet.AddRangeAsync(entity);

    public void Update(TAggregate entity)
    {
        DbSet.Attach(entity);

        Context.Entry(entity).State = EntityState.Modified;
    }

    public async Task<bool> RemoveAsync(object id)
    {
        TAggregate? entity = await DbSet.FindAsync(id);

        if (entity is null)
        {
            return false;
        }

        DbSet.Remove(entity);

        return true;
    }

    public async Task CommitAsync() => await Context.SaveChangesAsync();
}
