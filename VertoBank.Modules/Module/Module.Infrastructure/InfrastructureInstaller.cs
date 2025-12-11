using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Module.Application.Services;
using Module.Infrastructure.Persistence;
using Wolverine.Attributes;

[assembly: WolverineModule]

namespace Module.Infrastructure;

public static class InfrastructureInstaller
{
    public static IServiceCollection Install(IServiceCollection serviceCollection, string moduleConnectionString)
    {
        RegisterQueryObjects(serviceCollection);

        RegisterRepositories(serviceCollection);

        serviceCollection.AddDbContext<ModuleDbContext>(options => options.UseNpgsql(moduleConnectionString));

        return serviceCollection;
    }

    private static void RegisterQueryObjects(IServiceCollection serviceCollection) =>
        serviceCollection.Scan(scan => scan
            .FromAssemblies(typeof(InfrastructureInstaller).Assembly)
            .AddClasses(filter => filter.AssignableTo(typeof(IQueryObject<>)), false)
            .AsImplementedInterfaces()
            .WithTransientLifetime());

    private static void RegisterRepositories(IServiceCollection serviceCollection) =>
        serviceCollection.Scan(scan => scan
            .FromAssemblies(typeof(InfrastructureInstaller).Assembly)
            .AddClasses(filter => filter.AssignableTo(typeof(IRepository<>)), false)
            .AsImplementedInterfaces()
            .WithTransientLifetime());
}
