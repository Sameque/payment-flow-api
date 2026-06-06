using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PaymentFlow.Messaging;

namespace PaymentFlow.RabbitMq;

public static class RabbitMqServiceCollectionExtensions
{
    public static IServiceCollection AddPaymentFlowRabbitMq(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        services.AddSingleton<IRabbitMqConnectionProvider, RabbitMqConnectionProvider>();
        services.AddScoped<IIntegrationEventPublisher, RabbitMqIntegrationEventPublisher>();

        return services;
    }

    public static IServiceCollection AddPaymentFlowRabbitMqConsumer(
        this IServiceCollection services,
        Action<RabbitMqConsumerOptions> configure)
    {
        services.Configure(configure);
        services.AddHostedService<RabbitMqConsumerBackgroundService>();

        return services;
    }
}
