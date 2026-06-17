using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PaymentFlow.Outbox;

public static class OutboxServiceCollectionExtensions
{
    public static IServiceCollection AddPaymentFlowOutboxWriter(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OutboxDispatcherOptions>(configuration.GetSection(OutboxDispatcherOptions.SectionName));
        services.AddScoped<IOutboxMessageWriter, OutboxMessageWriter>();

        return services;
    }

    public static IServiceCollection AddPaymentFlowOutboxDispatcher(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPaymentFlowOutboxWriter(configuration);
        services.AddHostedService<OutboxDispatcher>();

        return services;
    }
}
