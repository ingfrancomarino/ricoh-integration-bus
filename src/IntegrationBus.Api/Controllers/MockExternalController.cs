using IntegrationBus.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace IntegrationBus.Api.Controllers;

/// <summary>
/// Simulates the external provider API for local development and demo purposes.
/// </summary>
[ApiController]
[Route("mock")]
public class MockExternalController : ControllerBase
{
    [HttpPost("orders")]
    public IActionResult ReceiveOrder([FromBody] Order order)
    {
        Console.WriteLine($"[Mock External API] Received order: Id={order.Id}, Status={order.Status}, Amount={order.Amount}");
        return Ok(new { received = true, orderId = order.Id });
    }
}
