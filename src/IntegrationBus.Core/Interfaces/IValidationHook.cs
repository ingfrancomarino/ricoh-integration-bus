using System.Text.Json;

namespace IntegrationBus.Core.Interfaces;

public interface IValidationHook
{
    Task<bool> ValidateAsync(JsonElement order);
}
