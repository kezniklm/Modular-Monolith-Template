using Microsoft.Extensions.DependencyInjection;
using Wolverine.Attributes;

[assembly: WolverineModule]

namespace Module.Presentation;

public static class PresentationInstaller
{
    public static IServiceCollection Install(IServiceCollection serviceCollection) => serviceCollection;
}
