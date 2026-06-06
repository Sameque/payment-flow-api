using PaymentFlow.Audit.Worker;
using PaymentFlow.Contracts;
using PaymentFlow.Messaging;
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
        services.AddPaymentFlowPersistence(context.Configuration);
        services.AddPaymentFlowRabbitMq(context.Configuration);
        services.AddPaymentFlowOutbox(context.Configuration);
        services.AddScoped<IIntegrationEventHandler>(provider => AuditIntegrationEventHandler.Create(EventNames.PaymentCreated, provider));
        services.AddScoped<IIntegrationEventHandler>(provider => AuditIntegrationEventHandler.Create(EventNames.FraudApproved, provider));
        services.AddScoped<IIntegrationEventHandler>(provider => AuditIntegrationEventHandler.Create(EventNames.FraudRejected, provider));
        services.AddScoped<IIntegrationEventHandler>(provider => AuditIntegrationEventHandler.Create(EventNames.PaymentApproved, provider));
        services.AddScoped<IIntegrationEventHandler>(provider => AuditIntegrationEventHandler.Create(EventNames.PaymentRejected, provider));
        services.AddScoped<IIntegrationEventHandler>(provider => AuditIntegrationEventHandler.Create(EventNames.NotificationSent, provider));
        services.AddPaymentFlowRabbitMqConsumer(options =>
        {
            options.ConsumerName = "audit-worker";
            options.QueueName = RabbitMqTopology.AuditQueue;
            options.DeadLetterQueueName = RabbitMqTopology.AuditDeadLetterQueue;
            options.Bindings = ["payment.#"];
        });
    })
    .Build();

await host.RunAsync();
