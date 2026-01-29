using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetryBuilderOtlpExporterExtensions = OpenTelemetry.OpenTelemetryBuilderOtlpExporterExtensions;

namespace VertoBank.ServiceDefaults;

public static class Extensions
{
    private const string HealthEndpointPath = "/health";
    private const string AlivenessEndpointPath = "/alive";

    extension(WebApplication app)
    {
        public WebApplication UseExceptionHandler()
        {
            if (!app.Environment.IsProduction())
            {
                return app;
            }

            app.UseExceptionHandler(applicationBuilder => applicationBuilder.Run(async context =>
            {
                var exception = context.Features.Get<IExceptionHandlerPathFeature>()?.Error;

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                context.Response.ContentType = "application/json";

                if (exception is ValidationException validationException)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;

                    var errors = validationException.Errors
                        .GroupBy(error => error.PropertyName ?? string.Empty)
                        .ToDictionary(
                            group => group.Key,
                            group => group.Select(error => error.ErrorMessage).ToArray());

                    var validationPayload = JsonSerializer.Serialize(new
                    {
                        error = "Validation failed.",
                        details = errors
                    });

                    await context.Response.WriteAsync(validationPayload);
                    return;
                }

                var genericPayload = JsonSerializer.Serialize(new
                {
                    error = "An unexpected error occurred."
                });

                await context.Response.WriteAsync(genericPayload);
            }));

            return app;
        }

        public WebApplication MapDefaultEndpoints()
        {
            if (!app.Environment.IsDevelopment())
            {
                return app;
            }

            app.MapHealthChecks(HealthEndpointPath);

            app.MapHealthChecks(AlivenessEndpointPath, new HealthCheckOptions
            {
                Predicate = healthCheckRegistration => healthCheckRegistration.Tags.Contains("live")
            });

            return app;
        }
    }

    extension<TBuilder>(TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        public TBuilder AddServiceDefaults(string serviceName)
        {
            builder.ConfigureOpenTelemetry(serviceName);

            builder.AddDefaultHealthChecks();

            builder.Services.AddServiceDiscovery();

            builder.Services.ConfigureHttpClientDefaults(http =>
            {
                http.AddStandardResilienceHandler();

                http.AddServiceDiscovery();
            });

            return builder;
        }

        private TBuilder ConfigureOpenTelemetry(string serviceName)
        {
            builder.Logging.AddOpenTelemetry(loggerOptions =>
            {
                loggerOptions.IncludeFormattedMessage = true;
                loggerOptions.IncludeScopes = true;

                loggerOptions.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName));
            });

            builder.Services.AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService(serviceName))
                .WithMetrics(metrics => metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                )
                .WithTracing(tracing => tracing
                    .AddAspNetCoreInstrumentation(traceInstrumentationOptions =>
                        traceInstrumentationOptions.Filter = ShouldTrace)
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddSource("Wolverine")
                );

            builder.AddOpenTelemetryExporters();

            return builder;

            static bool ShouldTrace(HttpContext httpContext) =>
                !httpContext.Request.Path.StartsWithSegments(HealthEndpointPath, StringComparison.OrdinalIgnoreCase) &&
                !httpContext.Request.Path.StartsWithSegments(AlivenessEndpointPath, StringComparison.OrdinalIgnoreCase);
        }

        private TBuilder AddOpenTelemetryExporters()
        {
            var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

            if (useOtlpExporter)
            {
                OpenTelemetryBuilderOtlpExporterExtensions.UseOtlpExporter(builder.Services.AddOpenTelemetry());
            }

            return builder;
        }

        private TBuilder AddDefaultHealthChecks()
        {
            builder.Services.AddHealthChecks().AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

            return builder;
        }
    }
}
