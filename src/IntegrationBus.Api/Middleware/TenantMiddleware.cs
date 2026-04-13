using IntegrationBus.Core.Interfaces;

namespace IntegrationBus.Api.Middleware;

public class TenantMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        if (context.Request.Headers.TryGetValue("X-Tenant-ID", out var tenantId))
        {
            tenantContext.TenantId = tenantId.ToString();
        }

        await next(context);
    }
}
