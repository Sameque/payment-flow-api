namespace PaymentFlow.Contracts;

public interface IPaymentEvent
{
    Guid PaymentId { get; }
}
