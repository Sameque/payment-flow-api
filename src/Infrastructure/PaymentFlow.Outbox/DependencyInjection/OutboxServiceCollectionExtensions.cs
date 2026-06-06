using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PaymentFlow.Outbox;

public static class OutboxServiceCollectionExtensions
{
    public static IServiceCollection AddPaymentFlowOutbox(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OutboxDispatcherOptions>(configuration.GetSection(OutboxDispatcherOptions.SectionName));
        services.AddScoped<IOutboxMessageWriter, OutboxMessageWriter>();
        services.AddHostedService<OutboxDispatcher>();

        return services;
    }
}
