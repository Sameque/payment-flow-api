using PaymentFlow.Contracts;
using PaymentFlow.Messaging;
using PaymentFlow.Observability;
using PaymentFlow.Outbox;
using PaymentFlow.Persistence;
using PaymentFlow.Processor.Worker;
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
        services.AddScoped<IIntegrationEventHandler, FraudApprovedHandler>();
        services.AddPaymentFlowRabbitMqConsumer(options =>
        {
            options.ConsumerName = "processor-worker";
            options.QueueName = RabbitMqTopology.ProcessorQueue;
            options.DeadLetterQueueName = RabbitMqTopology.ProcessorDeadLetterQueue;
            options.Bindings = [RoutingKeys.PaymentFraudApproved];
        });
    })
    .Build();

await host.RunAsync();
