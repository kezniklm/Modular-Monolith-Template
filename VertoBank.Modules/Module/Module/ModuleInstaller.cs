using Microsoft.Extensions.DependencyInjection;
using Module.Application;
using Module.Domain;
using Module.Infrastructure;
using Module.Presentation;
using SharedKernel;

namespace Module;

public class ModuleInstaller : IModule
{
    public void InstallDomain(IServiceCollection serviceCollection) => DomainInstaller.Install(serviceCollection);

    public void InstallApplication(IServiceCollection serviceCollection) =>
        ApplicationInstaller.Install(serviceCollection);

    public void InstallInfrastructure(IServiceCollection serviceCollection, string connectionString) =>
        InfrastructureInstaller.Install(serviceCollection, connectionString);

    public void InstallPresentation(IServiceCollection serviceCollection) =>
        PresentationInstaller.Install(serviceCollection);
}
