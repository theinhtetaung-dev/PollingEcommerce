using Polling.Shared;
using PollingPayment.Domain.Models.Payment;

namespace PollingPayment.Domain.Features.Payment;

public interface IPaymentService
{
    Task<Result<PaymentResponse>> CreateAsync(CreatePaymentRequest request, CancellationToken cancellationToken);

    Task<Result<PaymentResponse>> GetAsync(Guid reference, CancellationToken cancellationToken);
}
