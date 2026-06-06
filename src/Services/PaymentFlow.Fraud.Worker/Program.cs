using PaymentFlow.Contracts;
using PaymentFlow.Fraud.Worker;
using PaymentFlow.Fraud.Worker.Services;
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
        services.AddScoped<IFraudAnalysisService, FraudAnalysisService>();
        services.AddScoped<IIntegrationEventHandler, PaymentCreatedHandler>();
        services.AddPaymentFlowRabbitMqConsumer(options =>
        {
            options.ConsumerName = "fraud-worker";
            options.QueueName = RabbitMqTopology.FraudQueue;
            options.DeadLetterQueueName = RabbitMqTopology.FraudDeadLetterQueue;
            options.Bindings = [RoutingKeys.PaymentTransactionCreated];
        });
    })
    .Build();

await host.RunAsync();
