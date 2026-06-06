using Microsoft.Extensions.DependencyInjection;

namespace PaymentFlow.Messaging;

public static class MessagingServiceCollectionExtensions
{
    public static IServiceCollection AddPaymentFlowMessaging(this IServiceCollection services)
    {
        services.AddSingleton<IIntegrationEventTypeRegistry, IntegrationEventTypeRegistry>();
        return services;
    }
}
