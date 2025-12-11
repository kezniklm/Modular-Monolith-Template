using Microsoft.Extensions.DependencyInjection;

namespace SharedKernel;

public interface IModule
{
    void InstallDomain(IServiceCollection serviceCollection);
    void InstallApplication(IServiceCollection serviceCollection);
    void InstallInfrastructure(IServiceCollection serviceCollection, string connectionString);
    void InstallPresentation(IServiceCollection serviceCollection);
}
