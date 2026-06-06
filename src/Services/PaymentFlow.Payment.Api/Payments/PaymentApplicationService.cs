using Microsoft.EntityFrameworkCore.Storage;
using PaymentFlow.Contracts;
using PaymentFlow.Outbox;
using PaymentFlow.Persistence;
using PaymentFlow.SharedKernel;
using DomainPayment = PaymentFlow.SharedKernel.Payment;

namespace PaymentFlow.Payment.Api.Payments;

public sealed class PaymentApplicationService(
    PaymentDbContext dbContext,
    IPaymentRepository paymentRepository,
    IOutboxMessageWriter outboxMessageWriter,
    ILogger<PaymentApplicationService> logger)
{
    public async Task<PaymentResponse> CreateAsync(CreatePaymentRequest request, CancellationToken cancellationToken)
    {
        if (request.CustomerId == Guid.Empty)
        {
            throw new ArgumentException("CustomerId is required.", nameof(request));
        }

        if (request.Amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request), request.Amount, "Amount must be greater than zero.");
        }

        Guid paymentId = Guid.NewGuid();
        Guid correlationId = request.CorrelationId.GetValueOrDefault(Guid.NewGuid());
        DateTimeOffset now = DateTimeOffset.UtcNow;

        var payment = new DomainPayment(paymentId, request.CustomerId, request.Amount, now);
        var integrationEvent = new PaymentCreatedEvent(
            Guid.NewGuid(),
            correlationId,
            now,
            payment.Id,
            payment.CustomerId,
            payment.Amount);

        await using IDbContextTransaction transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        await paymentRepository.AddAsync(payment, cancellationToken);
        outboxMessageWriter.Add(integrationEvent, RoutingKeys.PaymentTransactionCreated);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        logger.LogInformation(
            "Payment {PaymentId} created with correlation {CorrelationId} and event {EventId}.",
            payment.Id,
            correlationId,
            integrationEvent.EventId);

        return Map(payment);
    }

    public async Task<PaymentResponse?> GetAsync(Guid paymentId, CancellationToken cancellationToken)
    {
        DomainPayment? payment = await paymentRepository.GetByIdAsync(paymentId, cancellationToken);
        return payment is null ? null : Map(payment);
    }

    private static PaymentResponse Map(DomainPayment payment)
    {
        return new PaymentResponse(
            payment.Id,
            payment.CustomerId,
            payment.Amount,
            payment.Status,
            payment.CreatedAtUtc,
            payment.UpdatedAtUtc);
    }
}
