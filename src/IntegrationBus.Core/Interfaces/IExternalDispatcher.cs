using IntegrationBus.Core.Models;

namespace IntegrationBus.Core.Interfaces;

public interface IExternalDispatcher
{
    Task DispatchAsync(Order order, CancellationToken cancellationToken = default);
}
