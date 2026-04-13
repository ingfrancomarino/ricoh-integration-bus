using System.Runtime.CompilerServices;
using System.Text.Json;
using IntegrationBus.Core.Interfaces;
using IntegrationBus.Core.Models;

namespace IntegrationBus.Services;

public class OrderProcessor : IOrderProcessor
{
    /// <summary>
    /// Streams orders from a JSON input, validating each one before yielding.
    /// Expected JSON format: { "orders": [ { ... }, { ... } ] }
    /// </summary>
    public async IAsyncEnumerable<Order> ProcessTenantOrdersAsync(
        Stream jsonStream,
        IValidationHook validationHook,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        using var document = await JsonDocument.ParseAsync(jsonStream, cancellationToken: cancellationToken);

        if (!document.RootElement.TryGetProperty("orders", out var ordersArray)
            || ordersArray.ValueKind != JsonValueKind.Array)
            yield break;

        foreach (var element in ordersArray.EnumerateArray())
        {
            cancellationToken.ThrowIfCancellationRequested();

            var isValid = await validationHook.ValidateAsync(element);
            if (!isValid)
                continue;

            var order = element.Deserialize<Order>(options);
            if (order is not null)
                yield return order;
        }
    }
}
