using Microsoft.Extensions.DependencyInjection;
using Wolverine.Attributes;

[assembly: WolverineModule]

namespace Module.Application;

public static class ApplicationInstaller
{
    public static IServiceCollection Install(IServiceCollection services) => services;
}
