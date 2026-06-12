using Microsoft.AspNetCore.Mvc;
using PaymentFlow.Payment.Api.Common;
using PaymentFlow.SharedKernel;

namespace PaymentFlow.Payment.Api.Payments;

[ApiController]
[Route("payments")]
public sealed class PaymentsController(IPaymentApplicationService payments) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<PaymentResponse>> Create(
        [FromBody] CreatePaymentRequest request,
        CancellationToken cancellationToken)
    {
        PaymentResponse response = await payments.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = response.Id }, response);
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<PaymentListItemResponse>>> Search(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] PaymentStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        PaginatedResponse<PaymentListItemResponse> response = await payments.SearchAsync(
            page,
            pageSize,
            search,
            status,
            cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PaymentDetailsResponse>> Get(Guid id, CancellationToken cancellationToken)
    {
        PaymentDetailsResponse? response = await payments.GetDetailsAsync(id, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }
}
