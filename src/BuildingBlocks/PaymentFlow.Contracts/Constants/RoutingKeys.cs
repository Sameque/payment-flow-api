namespace PaymentFlow.Contracts;

public static class RoutingKeys
{
    public const string PaymentTransactionCreated = "payment.transaction.created";
    public const string PaymentFraudApproved = "payment.fraud.approved";
    public const string PaymentFraudRejected = "payment.fraud.rejected";
    public const string PaymentFraudFailed = "payment.fraud.failed";
    public const string PaymentProcessorStarted = "payment.processor.started";
    public const string PaymentProcessorApproved = "payment.processor.approved";
    public const string PaymentProcessorRejected = "payment.processor.rejected";
    public const string PaymentProcessorFailed = "payment.processor.failed";
    public const string PaymentNotificationSent = "payment.notification.sent";
    public const string PaymentNotificationFailed = "payment.notification.failed";
    public const string PaymentAuditLogged = "payment.audit.logged";
}
