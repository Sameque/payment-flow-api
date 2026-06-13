using PaymentFlow.Persistence;
using PaymentFlow.SharedKernel;
using DomainPayment = PaymentFlow.SharedKernel.Payment;

namespace PaymentFlow.Payment.Api.Dashboard;

public sealed class DashboardQueryService(IPaymentRepository paymentRepository) : IDashboardQueryService
{
    public async Task<DashboardSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken)
    {
        int total = await paymentRepository.CountAsync(cancellationToken);
        int approved = await paymentRepository.CountByStatusAsync(PaymentStatus.Approved, cancellationToken);
        int rejected = await paymentRepository.CountByStatusAsync(PaymentStatus.Rejected, cancellationToken);
        int failed = await paymentRepository.CountByStatusAsync(PaymentStatus.Failed, cancellationToken);

        return new DashboardSummaryResponse(total, approved, rejected + failed, 0);
    }

    public async Task<IReadOnlyList<PaymentsPerHourPointResponse>> GetPaymentsPerHourAsync(
        CancellationToken cancellationToken)
    {
        DateTimeOffset since = DateTimeOffset.UtcNow.AddHours(-23).ToUniversalTime();
        IReadOnlyList<DomainPayment> payments = await paymentRepository.GetCreatedSinceAsync(since, cancellationToken);

        var buckets = Enumerable.Range(0, 24)
            .Select(offset => since.AddHours(offset))
            .ToDictionary(
                hour => hour.ToString("HH:00"),
                _ => 0);

        foreach (DomainPayment payment in payments)
        {
            string key = payment.CreatedAtUtc.ToString("HH:00");
            if (buckets.ContainsKey(key))
            {
                buckets[key]++;
            }
        }

        return buckets
            .OrderBy(entry => entry.Key)
            .Select(entry => new PaymentsPerHourPointResponse(entry.Key, entry.Value))
            .ToList();
    }

    public async Task<IReadOnlyList<ApprovalRatePointResponse>> GetApprovalRateAsync(
        CancellationToken cancellationToken)
    {
        DateTimeOffset utcNow = DateTimeOffset.UtcNow;
        DateTimeOffset since = new DateTimeOffset(utcNow.Year, utcNow.Month, utcNow.Day, 0, 0, 0, TimeSpan.Zero).AddDays(-6);
        IReadOnlyList<DomainPayment> payments = await paymentRepository.GetCreatedSinceAsync(since, cancellationToken);

        var results = new List<ApprovalRatePointResponse>();

        for (int dayOffset = 0; dayOffset < 7; dayOffset++)
        {
            DateTimeOffset day = since.AddDays(dayOffset);
            DateTimeOffset nextDay = day.AddDays(1).ToUniversalTime();

            IEnumerable<DomainPayment> dayPayments = payments.Where(payment =>
                payment.CreatedAtUtc >= day && payment.CreatedAtUtc < nextDay);

            int approved = dayPayments.Count(payment => payment.Status == PaymentStatus.Approved);
            int rejected = dayPayments.Count(payment =>
                payment.Status is PaymentStatus.Rejected or PaymentStatus.Failed);

            int total = approved + rejected;
            decimal rate = total == 0 ? 0 : Math.Round((decimal)approved / total * 100, 1);

            results.Add(new ApprovalRatePointResponse(day.ToString("dd MMM"), rate));
        }

        return results;
    }
}
