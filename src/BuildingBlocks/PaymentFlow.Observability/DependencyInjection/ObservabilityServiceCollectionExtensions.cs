using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

namespace PaymentFlow.Observability;

public static class ObservabilityServiceCollectionExtensions
{
    public static IServiceCollection AddPaymentFlowObservability(this IServiceCollection services, IConfiguration configuration)
    {
        ObservabilityOptions options = configuration.GetSection(ObservabilityOptions.SectionName).Get<ObservabilityOptions>()
            ?? new ObservabilityOptions();

        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(options.ServiceName))
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource("PaymentFlow")
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();

                if (options.UseConsoleExporter)
                {
                    tracing.AddConsoleExporter();
                }
            });

        return services;
    }
}
