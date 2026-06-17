using PaymentFlow.Messaging;
using PaymentFlow.Observability;
using PaymentFlow.Outbox;
using PaymentFlow.Payment.Api.Audit;
using PaymentFlow.Payment.Api.Dashboard;
using PaymentFlow.Payment.Api.Payments;
using PaymentFlow.Payment.Api.Mappers;
using PaymentFlow.Payment.Api.Validators;
using PaymentFlow.Payment.Api.Middleware;
using PaymentFlow.Persistence;
using PaymentFlow.RabbitMq;
using Microsoft.EntityFrameworkCore;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(
                "http://localhost:5173",
                "http://127.0.0.1:5173",
                "http://localhost:4173")
            .AllowAnyHeader()
            .AllowAnyMethod());
});
builder.Services.AddPaymentFlowObservability(builder.Configuration);
builder.Host.UseSerilog();

builder.Services.AddPaymentFlowMessaging();
builder.Services.AddPaymentFlowPersistence(builder.Configuration);
builder.Services.AddPaymentFlowRabbitMq(builder.Configuration);
builder.Services.AddPaymentFlowOutboxDispatcher(builder.Configuration);
builder.Services.AddScoped<IPaymentApplicationService, PaymentApplicationService>();
builder.Services.AddScoped<IAuditQueryService, AuditQueryService>();
builder.Services.AddScoped<IDashboardQueryService, DashboardQueryService>();
builder.Services.AddScoped<IPaymentMapper, PaymentMapper>();
builder.Services.AddScoped<ICreatePaymentRequestValidator, CreatePaymentRequestValidator>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

WebApplication app = builder.Build();

if (args.Contains("--migrate"))
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Applying database migrations...");
        var dbContext = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
        await dbContext.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied successfully.");
        Environment.Exit(0);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while applying database migrations.");
        Environment.Exit(1);
    }
}

app.UseSerilogRequestLogging();
app.UseCors();
app.UseExceptionHandler();
app.MapControllers();

await app.RunAsync();
