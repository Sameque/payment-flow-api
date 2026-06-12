using PaymentFlow.Contracts;
using PaymentFlow.SharedKernel;

namespace PaymentFlow.Payment.Api.Payments;

public static class PaymentTimelineBuilder
{
    private static readonly (string EventType, string Label)[] PipelineSteps =
    [
        (EventNames.PaymentCreated, "Payment Created"),
        (EventNames.FraudApproved, "Fraud Check"),
        (EventNames.PaymentApproved, "Processor"),
        (EventNames.NotificationSent, "Notification Sent")
    ];

    public static IReadOnlyList<PaymentTimelineStepResponse> Build(
        PaymentStatus paymentStatus,
        IReadOnlyList<Persistence.AuditLog> auditLogs)
    {
        Dictionary<string, Persistence.AuditLog> auditByType = auditLogs
            .GroupBy(log => log.EventType)
            .ToDictionary(group => group.Key, group => group.OrderBy(log => log.CreatedAtUtc).First());

        bool fraudRejected = auditByType.ContainsKey(EventNames.FraudRejected);
        bool processorRejected = auditByType.ContainsKey(EventNames.PaymentRejected);

        return PipelineSteps.Select((step, index) => MapStep(
            index,
            step.EventType,
            step.Label,
            paymentStatus,
            auditByType,
            fraudRejected,
            processorRejected)).ToList();
    }

    private static PaymentTimelineStepResponse MapStep(
        int index,
        string defaultEventType,
        string defaultLabel,
        PaymentStatus paymentStatus,
        Dictionary<string, Persistence.AuditLog> auditByType,
        bool fraudRejected,
        bool processorRejected)
    {
        string label = defaultLabel;
        string eventType = defaultEventType;
        string status;
        DateTimeOffset? timestamp = null;

        switch (defaultEventType)
        {
            case EventNames.PaymentCreated:
                if (auditByType.TryGetValue(EventNames.PaymentCreated, out Persistence.AuditLog? created))
                {
                    status = "completed";
                    timestamp = created.CreatedAtUtc;
                }
                else
                {
                    status = "pending";
                }

                break;

            case EventNames.FraudApproved:
                if (fraudRejected)
                {
                    label = "Fraud Rejected";
                    eventType = EventNames.FraudRejected;
                    status = "failed";
                    timestamp = auditByType[EventNames.FraudRejected].CreatedAtUtc;
                }
                else if (auditByType.TryGetValue(EventNames.FraudApproved, out Persistence.AuditLog? fraudApproved))
                {
                    status = "completed";
                    timestamp = fraudApproved.CreatedAtUtc;
                }
                else
                {
                    status = "pending";
                }

                break;

            case EventNames.PaymentApproved:
                if (fraudRejected)
                {
                    status = "skipped";
                }
                else if (processorRejected)
                {
                    label = "Processor Rejected";
                    eventType = EventNames.PaymentRejected;
                    status = "failed";
                    timestamp = auditByType[EventNames.PaymentRejected].CreatedAtUtc;
                }
                else if (auditByType.TryGetValue(EventNames.PaymentApproved, out Persistence.AuditLog? approved))
                {
                    status = "completed";
                    timestamp = approved.CreatedAtUtc;
                }
                else if (paymentStatus == PaymentStatus.Approved)
                {
                    status = "completed";
                }
                else if (paymentStatus is PaymentStatus.Rejected or PaymentStatus.Failed)
                {
                    status = "failed";
                }
                else
                {
                    status = "pending";
                }

                break;

            case EventNames.NotificationSent:
                if (fraudRejected || processorRejected)
                {
                    status = "skipped";
                }
                else if (auditByType.TryGetValue(EventNames.NotificationSent, out Persistence.AuditLog? notification))
                {
                    status = "completed";
                    timestamp = notification.CreatedAtUtc;
                }
                else
                {
                    status = paymentStatus == PaymentStatus.Approved ? "pending" : "pending";
                }

                break;

            default:
                status = "pending";
                break;
        }

        return new PaymentTimelineStepResponse(
            (index + 1).ToString(),
            label,
            eventType,
            status,
            timestamp,
            null);
    }
}
