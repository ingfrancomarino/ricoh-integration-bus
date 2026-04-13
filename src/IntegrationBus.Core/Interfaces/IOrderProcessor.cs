using IntegrationBus.Core.Models;

namespace IntegrationBus.Core.Interfaces;

public interface IOrderProcessor
{
    IAsyncEnumerable<Order> ProcessTenantOrdersAsync(
        Stream jsonStream,
        IValidationHook validationHook,
        CancellationToken cancellationToken = default);
}
