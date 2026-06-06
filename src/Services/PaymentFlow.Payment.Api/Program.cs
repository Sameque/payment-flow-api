using PaymentFlow.Messaging;
using PaymentFlow.Observability;
using PaymentFlow.Outbox;
using PaymentFlow.Payment.Api.Payments;
using PaymentFlow.Payment.Api.Mappers;
using PaymentFlow.Payment.Api.Validators;
using PaymentFlow.Payment.Api.Middleware;
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
builder.Services.AddScoped<IPaymentApplicationService, PaymentApplicationService>();
builder.Services.AddScoped<IPaymentMapper, PaymentMapper>();
builder.Services.AddScoped<ICreatePaymentRequestValidator, CreatePaymentRequestValidator>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

WebApplication app = builder.Build();

app.UseSerilogRequestLogging();
app.UseExceptionHandler();
app.MapControllers();

await app.RunAsync();
