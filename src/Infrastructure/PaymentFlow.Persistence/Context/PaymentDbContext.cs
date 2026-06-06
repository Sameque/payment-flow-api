using Microsoft.EntityFrameworkCore;
using PaymentFlow.SharedKernel;

namespace PaymentFlow.Persistence;

public sealed class PaymentDbContext(DbContextOptions<PaymentDbContext> options) : DbContext(options)
{
    public DbSet<Payment> Payments => Set<Payment>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public DbSet<ProcessedMessage> ProcessedMessages => Set<ProcessedMessage>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Payment>(builder =>
        {
            builder.ToTable("Payments");
            builder.HasKey(payment => payment.Id);
            builder.Property(payment => payment.CustomerId).IsRequired();
            builder.Property(payment => payment.Amount).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(payment => payment.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
            builder.Property(payment => payment.CreatedAtUtc).IsRequired();
            builder.Property(payment => payment.UpdatedAtUtc).IsRequired();
            builder.HasIndex(payment => payment.CustomerId);
        });

        modelBuilder.Entity<OutboxMessage>(builder =>
        {
            builder.ToTable("OutboxMessages");
            builder.HasKey(message => message.Id);
            builder.Property(message => message.EventType).HasMaxLength(256).IsRequired();
            builder.Property(message => message.RoutingKey).HasMaxLength(128).IsRequired();
            builder.Property(message => message.Payload).IsRequired();
            builder.Property(message => message.Processed).IsRequired();
            builder.Property(message => message.CreatedAtUtc).IsRequired();
            builder.Property(message => message.Error).HasMaxLength(2048);
            builder.HasIndex(message => new { message.Processed, message.CreatedAtUtc });
        });

        modelBuilder.Entity<ProcessedMessage>(builder =>
        {
            builder.ToTable("ProcessedMessages");
            builder.HasKey(message => new { message.MessageId, message.ConsumerName });
            builder.Property(message => message.ConsumerName).HasMaxLength(128).IsRequired();
            builder.Property(message => message.ProcessedAtUtc).IsRequired();
        });

        modelBuilder.Entity<AuditLog>(builder =>
        {
            builder.ToTable("AuditLogs");
            builder.HasKey(log => log.Id);
            builder.Property(log => log.EventType).HasMaxLength(256).IsRequired();
            builder.Property(log => log.RoutingKey).HasMaxLength(128).IsRequired();
            builder.Property(log => log.Payload).IsRequired();
            builder.Property(log => log.CreatedAtUtc).IsRequired();
            builder.HasIndex(log => log.EventId);
            builder.HasIndex(log => log.PaymentId);
        });
    }
}
