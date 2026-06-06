namespace PaymentFlow.Payment.Api.Middleware;

public record ErrorResponse(
    int StatusCode,
    string Message,
    string? Detail = null);
