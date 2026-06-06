namespace PaymentFlow.Contracts;

public static class EventNames
{
    public const string PaymentCreated = "PaymentCreatedEvent";
    public const string FraudApproved = "FraudApprovedEvent";
    public const string FraudRejected = "FraudRejectedEvent";
    public const string PaymentApproved = "PaymentApprovedEvent";
    public const string PaymentRejected = "PaymentRejectedEvent";
    public const string NotificationSent = "NotificationSentEvent";
}
