using PaymentFlow.Contracts;
using PaymentFlow.Outbox;
using PaymentFlow.Payment.Api.Common;
using PaymentFlow.Payment.Api.Mappers;
using PaymentFlow.Payment.Api.Validators;
using PaymentFlow.Persistence;
using PaymentFlow.SharedKernel;
using DomainPayment = PaymentFlow.SharedKernel.Payment;

namespace PaymentFlow.Payment.Api.Payments;

public sealed class PaymentApplicationService(
    IUnitOfWork unitOfWork,
    IPaymentRepository paymentRepository,
    IAuditLogRepository auditLogRepository,
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

    public async Task<PaymentDetailsResponse?> GetDetailsAsync(Guid paymentId, CancellationToken cancellationToken)
    {
        DomainPayment? payment = await paymentRepository.GetByIdAsync(paymentId, cancellationToken);
        if (payment is null)
        {
            return null;
        }

        Guid? correlationId = await auditLogRepository.GetCorrelationIdForPaymentAsync(paymentId, cancellationToken);
        IReadOnlyList<AuditLog> auditLogs = await auditLogRepository.GetByPaymentIdAsync(paymentId, cancellationToken);

        return new PaymentDetailsResponse(
            payment.Id,
            payment.CustomerId,
            payment.Amount,
            payment.Status,
            correlationId,
            payment.CreatedAtUtc,
            payment.UpdatedAtUtc,
            PaymentTimelineBuilder.Build(payment.Status, auditLogs));
    }

    public async Task<PaginatedResponse<PaymentListItemResponse>> SearchAsync(
        int page,
        int pageSize,
        string? search,
        PaymentStatus? status,
        CancellationToken cancellationToken)
    {
        if (page < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(page), "Page must be greater than zero.");
        }

        if (pageSize is < 1 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be between 1 and 100.");
        }

        (IReadOnlyList<DomainPayment> items, int totalCount) = await paymentRepository.SearchAsync(
            page,
            pageSize,
            search,
            status,
            cancellationToken);

        var responses = new List<PaymentListItemResponse>(items.Count);
        foreach (DomainPayment payment in items)
        {
            Guid? correlationId = await auditLogRepository.GetCorrelationIdForPaymentAsync(payment.Id, cancellationToken);
            responses.Add(new PaymentListItemResponse(
                payment.Id,
                payment.CustomerId,
                payment.Amount,
                payment.Status,
                correlationId,
                payment.CreatedAtUtc,
                payment.UpdatedAtUtc));
        }

        int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        return new PaginatedResponse<PaymentListItemResponse>(responses, totalCount, page, pageSize, totalPages);
    }
}
