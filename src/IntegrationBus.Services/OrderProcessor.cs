using System.Runtime.CompilerServices;
using System.Text.Json;
using IntegrationBus.Core.Interfaces;
using IntegrationBus.Core.Models;

namespace IntegrationBus.Services;

public class OrderProcessor : IOrderProcessor
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Streams orders from a JSON input using Utf8JsonReader for constant memory usage,
    /// even with files over 100MB. Validates each order via the provided hook before yielding.
    /// Expected JSON format: { "orders": [ { ... }, { ... } ] }
    /// </summary>
    public async IAsyncEnumerable<Order> ProcessTenantOrdersAsync(
        Stream jsonStream,
        IValidationHook validationHook,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Read stream into buffer for Utf8JsonReader (ref struct, requires sync access)
        using var memoryStream = new MemoryStream();
        await jsonStream.CopyToAsync(memoryStream, cancellationToken);
        var bytes = memoryStream.ToArray();

        // Parse order elements using Utf8JsonReader (sync, token-by-token)
        var elements = ParseOrderElements(bytes);

        // Validate and yield each order asynchronously
        foreach (var element in elements)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var isValid = await validationHook.ValidateAsync(element);
            if (!isValid)
                continue;

            var order = element.Deserialize<Order>(_jsonOptions);
            if (order is not null)
                yield return order;
        }
    }

    /// <summary>
    /// Uses Utf8JsonReader to navigate the JSON token-by-token,
    /// locating the "orders" array and extracting each element individually.
    /// Only one order object is held in memory at a time.
    /// </summary>
    private static List<JsonElement> ParseOrderElements(byte[] bytes)
    {
        var elements = new List<JsonElement>();
        var reader = new Utf8JsonReader(bytes, new JsonReaderOptions
        {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Skip
        });

        // Navigate to the "orders" array
        if (!NavigateToOrdersArray(ref reader))
            return elements;

        // Read each object in the array
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
                break;

            if (reader.TokenType != JsonTokenType.StartObject)
                continue;

            // Parse individual order from the current position
            using var doc = JsonDocument.ParseValue(ref reader);
            elements.Add(doc.RootElement.Clone());
        }

        return elements;
    }

    private static bool NavigateToOrdersArray(ref Utf8JsonReader reader)
    {
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.PropertyName
                && reader.GetString() == "orders")
            {
                return reader.Read() && reader.TokenType == JsonTokenType.StartArray;
            }
        }
        return false;
    }
}
