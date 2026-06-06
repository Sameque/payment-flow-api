using PaymentFlow.SharedKernel;

namespace PaymentFlow.UnitTests;

public sealed class PaymentTests
{
    [Fact]
    public void Constructor_creates_pending_payment()
    {
        DateTimeOffset createdAtUtc = DateTimeOffset.UtcNow;

        var payment = new Payment(Guid.NewGuid(), Guid.NewGuid(), 120.45m, createdAtUtc);

        Assert.Equal(PaymentStatus.Pending, payment.Status);
        Assert.Equal(createdAtUtc, payment.CreatedAtUtc);
        Assert.Equal(createdAtUtc, payment.UpdatedAtUtc);
    }

    [Fact]
    public void Constructor_rejects_non_positive_amount()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Payment(Guid.NewGuid(), Guid.NewGuid(), 0, DateTimeOffset.UtcNow));
    }
}
