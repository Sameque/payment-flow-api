using PaymentFlow.Messaging;
using PaymentFlow.Observability;
using PaymentFlow.Outbox;
using PaymentFlow.Payment.Api.Payments;
using PaymentFlow.Persistence;
using PaymentFlow.RabbitMq;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddPaymentFlowObservability(builder.Configuration);
builder.Host.UseSerilog();

builder.Services.AddPaymentFlowMessaging();
builder.Services.AddPaymentFlowPersistence(builder.Configuration);
builder.Services.AddPaymentFlowRabbitMq(builder.Configuration);
builder.Services.AddPaymentFlowOutbox(builder.Configuration);
builder.Services.AddScoped<PaymentApplicationService>();

WebApplication app = builder.Build();

app.UseSerilogRequestLogging();
app.MapControllers();

await app.RunAsync();
