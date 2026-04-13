using System.Text.Json;
using IntegrationBus.Core.Interfaces;

namespace IntegrationBus.Services;

public class DefaultValidationHook : IValidationHook
{
    public Task<bool> ValidateAsync(JsonElement order)
    {
        if (!order.TryGetProperty("id", out var id) || string.IsNullOrWhiteSpace(id.GetString()))
            return Task.FromResult(false);

        if (!order.TryGetProperty("status", out var status) || string.IsNullOrWhiteSpace(status.GetString()))
            return Task.FromResult(false);

        if (!order.TryGetProperty("amount", out var amount) || amount.GetDecimal() <= 0)
            return Task.FromResult(false);

        return Task.FromResult(true);
    }
}
