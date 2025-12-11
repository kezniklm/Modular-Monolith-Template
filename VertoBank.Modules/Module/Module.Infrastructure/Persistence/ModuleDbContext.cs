using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Domain.Common;
using Wolverine;

namespace Module.Infrastructure.Persistence;

public class ModuleDbContext : DbContext
{
    private readonly ILogger<ModuleDbContext> _logger;
    private readonly IMessageBus _sender;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public ModuleDbContext() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public ModuleDbContext(DbContextOptions<ModuleDbContext> options, IMessageBus sender,
        ILogger<ModuleDbContext> logger) : base(options)
    {
        _sender = sender;
        _logger = logger;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
    {
        IEnumerable<DomainEvent> domainEvents = ChangeTracker.Entries<BaseEntity>()
            .Select(entityEntry => entityEntry.Entity)
            .Where(baseEntity => baseEntity.DomainEvents.Count != 0)
            .SelectMany(baseEntity => baseEntity.DomainEvents);

        var result = await base.SaveChangesAsync(cancellationToken);

        foreach (DomainEvent domainEvent in domainEvents)
        {
            await _sender.PublishAsync(domainEvent);

            try
            {
                await _sender.PublishAsync(domainEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error publishing domain event {EventType}. Event: {@Event}",
                    domainEvent.GetType().Name,
                    domainEvent
                );
            }
        }

        return result;
    }
}
