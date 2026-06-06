using Microsoft.AspNetCore.Mvc;

namespace PaymentFlow.Payment.Api.Payments;

[ApiController]
[Route("payments")]
public sealed class PaymentsController(PaymentApplicationService payments) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<PaymentResponse>> Create(
        [FromBody] CreatePaymentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            PaymentResponse response = await payments.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = response.Id }, response);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PaymentResponse>> Get(Guid id, CancellationToken cancellationToken)
    {
        PaymentResponse? response = await payments.GetAsync(id, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }
}
