using PaymentFlow.Contracts;
using PaymentFlow.Outbox;
using PaymentFlow.Persistence;
using PaymentFlow.SharedKernel;
using PaymentFlow.Payment.Api.Mappers;
using PaymentFlow.Payment.Api.Validators;
using DomainPayment = PaymentFlow.SharedKernel.Payment;

namespace PaymentFlow.Payment.Api.Payments;

public sealed class PaymentApplicationService(
    IUnitOfWork unitOfWork,
    IPaymentRepository paymentRepository,
    IOutboxMessageWriter outboxMessageWriter,
    ICreatePaymentRequestValidator validator,
    IPaymentMapper mapper,
    ILogger<PaymentApplicationService> logger) : IPaymentApplicationService
{
    public async Task<PaymentResponse> CreateAsync(CreatePaymentRequest request, CancellationToken cancellationToken)
    {
        validator.Validate(request);

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

        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        await paymentRepository.AddAsync(payment, cancellationToken);
        outboxMessageWriter.Add(integrationEvent, RoutingKeys.PaymentTransactionCreated);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        logger.LogInformation(
            "Payment {PaymentId} created with correlation {CorrelationId} and event {EventId}.",
            payment.Id,
            correlationId,
            integrationEvent.EventId);

        return mapper.Map(payment);
    }

    public async Task<PaymentResponse?> GetAsync(Guid paymentId, CancellationToken cancellationToken)
    {
        DomainPayment? payment = await paymentRepository.GetByIdAsync(paymentId, cancellationToken);
        return payment is null ? null : mapper.Map(payment);
    }
}
