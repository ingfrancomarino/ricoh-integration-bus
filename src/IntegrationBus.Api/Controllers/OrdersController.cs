using System.Text;
using System.Text.Json;
using IntegrationBus.Core.Interfaces;
using IntegrationBus.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace IntegrationBus.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController(
    IOrderProcessor orderProcessor,
    IDynamicFilterService filterService,
    IExternalDispatcher dispatcher,
    IValidationHook validationHook) : ControllerBase
{
    /// <summary>
    /// Processes a batch of orders: validates via hook, applies dynamic filters, and dispatches to the external API.
    /// </summary>
    /// <remarks>
    /// Available filter keys:
    /// - **Status**: exact match (e.g. "Active", "Cancelled")
    /// - **Product**: exact match (e.g. "Widget", "Gadget")
    /// - **MinAmount**: minimum order amount, inclusive (e.g. "500")
    /// - **MaxAmount**: maximum order amount, inclusive (e.g. "10000")
    ///
    /// Filters are combined with AND logic. An empty filters object returns all valid orders.
    /// </remarks>
    [HttpPost("process")]
    public async Task<IActionResult> ProcessOrders(
        [FromBody] ProcessOrdersRequest request,
        CancellationToken cancellationToken)
    {
        // Serialize the orders array back into a stream for the streaming processor (camelCase)
        var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var json = JsonSerializer.Serialize(new { orders = request.Orders }, jsonOptions);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

        var predicate = filterService.BuildFilter(request.Filters).Compile();
        var results = new List<string>();

        await foreach (var order in orderProcessor.ProcessTenantOrdersAsync(
            stream, validationHook, cancellationToken))
        {
            if (!predicate(order))
                continue;

            await dispatcher.DispatchAsync(order, cancellationToken);
            results.Add(order.Id);
        }

        return Ok(new { Dispatched = results.Count, OrderIds = results });
    }
}
