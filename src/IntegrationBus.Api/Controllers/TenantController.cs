using IntegrationBus.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IntegrationBus.Api.Controllers;

[ApiController]
[Route("api/tenants")]
public class TenantController(ITenantService tenantService) : ControllerBase
{
    [HttpGet("{tenantId}/users")]
    public async Task<IActionResult> GetUsers(string tenantId)
    {
        var users = await tenantService.GetFakeUserNamesByTenantAsync(tenantId);
        return Ok(users);
    }
}
