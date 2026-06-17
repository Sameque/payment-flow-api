using PaymentFlow.Contracts;
using PaymentFlow.Messaging;
using PaymentFlow.Notification.Worker;
using PaymentFlow.Observability;
using PaymentFlow.Outbox;
using PaymentFlow.Persistence;
using PaymentFlow.RabbitMq;
using Serilog;

IHost host = Host.CreateDefaultBuilder(args)
    .UseSerilog()
    .ConfigureServices((context, services) =>
    {
        services.AddPaymentFlowObservability(context.Configuration);
        services.AddPaymentFlowMessaging();
        services.AddPaymentFlowRabbitMq(context.Configuration);
        services.AddPaymentFlowOutboxWriter(context.Configuration);
        services.AddScoped<IIntegrationEventHandler, PaymentApprovedNotificationHandler>();
        services.AddScoped<IIntegrationEventHandler, PaymentRejectedNotificationHandler>();
        services.AddScoped<IIntegrationEventHandler, FraudRejectedNotificationHandler>();
        services.AddPaymentFlowRabbitMqConsumer(options =>
        {
            options.ConsumerName = "notification-worker";
            options.QueueName = RabbitMqTopology.NotificationQueue;
            options.DeadLetterQueueName = RabbitMqTopology.NotificationDeadLetterQueue;
            options.Bindings = [RoutingKeys.PaymentProcessorApproved, RoutingKeys.PaymentProcessorRejected, RoutingKeys.PaymentFraudRejected];
        });
    })
    .Build();

await host.RunAsync();
