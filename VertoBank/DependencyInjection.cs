using Microsoft.OpenApi;
using Module;
using SharedKernel;
using Wolverine;
using Wolverine.FluentValidation;
using Wolverine.Http;

namespace VertoBank;

public static class DependencyInjection
{
    private static readonly IReadOnlyList<SwaggerModule> SwaggerModules =
    [
        new("module", "Module API", "v1") //TODO : Update module info
    ];

    public static IHostApplicationBuilder AddModules(this IHostApplicationBuilder hostApplicationBuilder)
    {
        IServiceCollection services = hostApplicationBuilder.Services;
        IConfigurationManager configuration = hostApplicationBuilder.Configuration;

        var modules = new List<IModule>
        {
            new ModuleInstaller() //TODO : Add additional modules
        };

        foreach (IModule module in modules)
        {
            module.InstallDomain(services);
            module.InstallApplication(services);

            var typeName = module.GetType().Name;

            const string installerSuffix = "Installer";
            const string connectionStringSuffix = "ConnectionString";

            var moduleName = typeName.EndsWith(installerSuffix, StringComparison.Ordinal)
                ? typeName[..^installerSuffix.Length]
                : typeName;

            var connectionStringName = $"{moduleName}{connectionStringSuffix}";

            var connectionString = configuration.GetConnectionString(connectionStringName)
                                   ?? throw new InvalidOperationException(
                                       $"Connection string '{connectionStringName}' for module '{moduleName}' not found");

            module.InstallInfrastructure(services, connectionString);

            module.InstallPresentation(services);
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

            foreach (SwaggerModule module in SwaggerModules)
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
        hostApplicationBuilder.UseWolverine(options => options.UseFluentValidation());

        hostApplicationBuilder.Services.AddWolverineHttp();

        return hostApplicationBuilder;
    }

    public static void UseSwaggerDocuments(this WebApplication app)
    {
        app.UseSwagger();

        app.UseSwaggerUI(options =>
        {
            foreach (SwaggerModule module in SwaggerModules)
            {
                options.SwaggerEndpoint(
                    $"/swagger/{module.Key}/swagger.json",
                    $"{module.Title} {module.Version}"
                );
            }
        });
    }

    private sealed record SwaggerModule(string Key, string Title, string Version);
}
