using PaymentFlow.Payment.Api.Payments;

namespace PaymentFlow.Payment.Api.Validators;

public interface ICreatePaymentRequestValidator
{
    void Validate(CreatePaymentRequest request);
}

public sealed class CreatePaymentRequestValidator : ICreatePaymentRequestValidator
{
    public void Validate(CreatePaymentRequest request)
    {
        if (request.CustomerId == Guid.Empty)
        {
            throw new ArgumentException("CustomerId is required.", nameof(request));
        }

        if (request.Amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request), request.Amount, "Amount must be greater than zero.");
        }
    }
}
