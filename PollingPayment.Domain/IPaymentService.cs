using Polling.Shared;

namespace PollingPayment.Domain;

public interface IPaymentService
{
    Task<Result<PaymentResponse>> CreateAsync(CreatePaymentRequest request, CancellationToken cancellationToken);

    Task<Result<PaymentResponse>> GetAsync(Guid reference, CancellationToken cancellationToken);
}
