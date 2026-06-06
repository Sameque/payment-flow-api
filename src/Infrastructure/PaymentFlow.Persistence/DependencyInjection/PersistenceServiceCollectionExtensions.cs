using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PaymentFlow.Persistence;

public static class PersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddPaymentFlowPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        PersistenceOptions options = configuration.GetSection(PersistenceOptions.SectionName).Get<PersistenceOptions>()
            ?? throw new InvalidOperationException("Persistence configuration is required.");

        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            throw new InvalidOperationException("Persistence connection string is required.");
        }

        services.AddDbContext<PaymentDbContext>(builder => builder.UseNpgsql(options.ConnectionString));
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
