namespace PaymentFlow.SharedKernel;

public sealed class Payment
{
    private Payment()
    {
    }

    public Payment(Guid id, Guid customerId, decimal amount, DateTimeOffset createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Payment id is required.", nameof(id));
        }

        if (customerId == Guid.Empty)
        {
            throw new ArgumentException("Customer id is required.", nameof(customerId));
        }

        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Payment amount must be greater than zero.");
        }

        Id = id;
        CustomerId = customerId;
        Amount = amount;
        Status = PaymentStatus.Pending;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid CustomerId { get; private set; }

    public decimal Amount { get; private set; }

    public PaymentStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public void MarkFraudAnalysis(DateTimeOffset updatedAtUtc)
    {
        Status = PaymentStatus.FraudAnalysis;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void MarkApproved(DateTimeOffset updatedAtUtc)
    {
        Status = PaymentStatus.Approved;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void MarkRejected(DateTimeOffset updatedAtUtc)
    {
        Status = PaymentStatus.Rejected;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void MarkFailed(DateTimeOffset updatedAtUtc)
    {
        Status = PaymentStatus.Failed;
        UpdatedAtUtc = updatedAtUtc;
    }
}
