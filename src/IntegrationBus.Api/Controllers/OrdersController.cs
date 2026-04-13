using IntegrationBus.Core.Interfaces;
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
    [HttpPost("process")]
    public async Task<IActionResult> ProcessOrders(
        [FromQuery] Dictionary<string, string> filters,
        CancellationToken cancellationToken)
    {
        var predicate = filterService.BuildFilter(filters).Compile();
        var results = new List<string>();

        await foreach (var order in orderProcessor.ProcessTenantOrdersAsync(
            Request.Body, validationHook, cancellationToken))
        {
            if (!predicate(order))
                continue;

            await dispatcher.DispatchAsync(order, cancellationToken);
            results.Add(order.Id);
        }

        return Ok(new { Dispatched = results.Count, OrderIds = results });
    }
}
