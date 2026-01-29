using Microsoft.OpenApi;
using Module;
using SharedKernel;
using Wolverine;
using Wolverine.FluentValidation;
using Wolverine.Http;
using Wolverine.Postgresql;

namespace VertoBank;

public static class DependencyInjection
{
    private static readonly IReadOnlyList<ModuleDescriptor> ModuleDescriptors =
    [
        new(new ModuleInstaller(), new SwaggerModule("Module", "Module API", "v1"))
    ];

    public static IHostApplicationBuilder AddModules(this IHostApplicationBuilder hostApplicationBuilder)
    {
        IServiceCollection services = hostApplicationBuilder.Services;

        IReadOnlyList<ModuleRegistration> moduleRegistrations =
            GetModuleRegistrations(hostApplicationBuilder.Configuration, ModuleDescriptors);

        foreach (var moduleRegistration in moduleRegistrations)
        {
            moduleRegistration.Module.InstallDomain(services);
            moduleRegistration.Module.InstallApplication(services);
            moduleRegistration.Module.InstallInfrastructure(services, moduleRegistration.ConnectionString);
            moduleRegistration.Module.InstallPresentation(services);
        }

        return hostApplicationBuilder;
    }

    public static IHostApplicationBuilder ConfigureSwaggerDocuments(this IHostApplicationBuilder applicationBuilder)
    {
        IServiceCollection services = applicationBuilder.Services;

        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(swaggerGenOptions =>
        {
            swaggerGenOptions.CustomSchemaIds(type =>
                type.FullName?.Replace('+', '.') ??
                throw new InvalidOperationException("Schema type FullName was null."));

            foreach (SwaggerModule module in ModuleDescriptors.Select(descriptor => descriptor.Swagger))
            {
                swaggerGenOptions.SwaggerDoc(module.Key, new OpenApiInfo
                {
                    Title = module.Title,
                    Version = module.Version
                });
            }

            swaggerGenOptions.DocInclusionPredicate((documentName, apiDescription) =>
            {
                IReadOnlyList<string> tags = apiDescription.ActionDescriptor.EndpointMetadata
                    .OfType<TagsAttribute>()
                    .SelectMany(tagsAttribute => tagsAttribute.Tags)
                    .ToList();

                return tags.Any(tag => tag.StartsWith(documentName, StringComparison.OrdinalIgnoreCase));
            });
        });

        return applicationBuilder;
    }

    public static IHostApplicationBuilder AddWolverineMessaging(this IHostApplicationBuilder hostApplicationBuilder)
    {
        IReadOnlyList<ModuleRegistration> moduleRegistrations =
            GetModuleRegistrations(hostApplicationBuilder.Configuration, ModuleDescriptors);

        hostApplicationBuilder.UseWolverine(options =>
        {
            options.UseFluentValidation();

            foreach (var moduleRegistration in moduleRegistrations)
            {
                options.PersistMessagesWithPostgresql(
                    moduleRegistration.ConnectionString,
                    GetWolverineSchemaName(moduleRegistration.ModuleName));
            }
        });

        hostApplicationBuilder.Services.AddWolverineHttp();

        return hostApplicationBuilder;
    }

    private static IReadOnlyList<ModuleRegistration> GetModuleRegistrations(
        IConfiguration configuration,
        IReadOnlyList<ModuleDescriptor> moduleDescriptors) =>
        moduleDescriptors.Select(descriptor =>
        {
            var typeName = descriptor.Module.GetType().Name;

            const string installerSuffix = "Installer";
            const string connectionStringSuffix = "ConnectionString";

            var moduleName = typeName.EndsWith(installerSuffix, StringComparison.Ordinal)
                ? typeName[..^installerSuffix.Length]
                : typeName;

            var connectionStringName = $"{moduleName}{connectionStringSuffix}";

            var connectionString = configuration.GetConnectionString(connectionStringName)
                                   ?? throw new InvalidOperationException(
                                       $"Connection string '{connectionStringName}' for module '{moduleName}' not found");

            return new ModuleRegistration(descriptor.Module, moduleName, connectionString);
        }).ToArray();

    private static string GetWolverineSchemaName(string moduleName) =>
        $"{moduleName.ToLowerInvariant()}_wolverine";

    public static void UseSwaggerDocuments(this WebApplication app)
    {
        app.UseSwagger();

        app.UseSwaggerUI(options =>
        {
            foreach (SwaggerModule module in ModuleDescriptors.Select(descriptor => descriptor.Swagger))
            {
                options.SwaggerEndpoint(
                    $"/swagger/{module.Key}/swagger.json",
                    $"{module.Title} {module.Version}"
                );
            }
        });
    }

    private sealed record ModuleDescriptor(IModule Module, SwaggerModule Swagger);

    private sealed record SwaggerModule(string Key, string Title, string Version);

    private sealed record ModuleRegistration(IModule Module, string ModuleName, string ConnectionString);
}
