using System.Linq.Expressions;

namespace Module.Application.Services;

public abstract class QueryObject<TAggregate>(IQueryable<TAggregate> query) : IQueryObject<TAggregate>
    where TAggregate : class
{
    protected IQueryable<TAggregate> Query { get; set; } = query ?? throw new ArgumentNullException(nameof(query));
#pragma warning disable CA1002
    protected List<(Expression<Func<TAggregate, object>> selector, bool ascending)> SortingCriteria { get; } = [];
#pragma warning restore CA1002

    public IQueryObject<TAggregate> Filter(Expression<Func<TAggregate, bool>> predicate)
    {
        Query = Query.Where(predicate);
        return this;
    }

    public IQueryObject<TAggregate> Page(int page, int pageSize)
    {
        Query = Query.Skip((page - 1) * pageSize).Take(pageSize);
        return this;
    }

    public IQueryObject<TAggregate> OrderBy(Expression<Func<TAggregate, object>> selector, bool ascending = true)
    {
        SortingCriteria.Add((selector, ascending));
        Query = ApplySorting();
        return this;
    }

    public abstract Task<IEnumerable<TAggregate>> ExecuteAsync();

    protected IQueryable<TAggregate> ApplySorting()
    {
        if (SortingCriteria.Count == 0)
        {
            return Query;
        }

        IOrderedQueryable<TAggregate>? orderedQuery = null;

        foreach ((Expression<Func<TAggregate, object>> selector, bool ascending) criteria in SortingCriteria)
        {
            orderedQuery = orderedQuery is null
                ? ApplyInitialOrdering(criteria)
                : ApplyThenOrdering(orderedQuery, criteria);
        }

        return orderedQuery!;
    }

    private IOrderedQueryable<TAggregate> ApplyInitialOrdering(
        (Expression<Func<TAggregate, object>> selector, bool ascending) criteria) =>
        criteria.ascending
            ? Query.OrderBy(criteria.selector)
            : Query.OrderByDescending(criteria.selector);

    private static IOrderedQueryable<TAggregate> ApplyThenOrdering(IOrderedQueryable<TAggregate> query,
        (Expression<Func<TAggregate, object>> selector, bool ascending) criteria) =>
        criteria.ascending
            ? query.ThenBy(criteria.selector)
            : query.ThenByDescending(criteria.selector);
}
