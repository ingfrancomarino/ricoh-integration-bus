using IntegrationBus.Api.Middleware;
using IntegrationBus.Services;
using Microsoft.AspNetCore.Http;

namespace IntegrationBus.Tests;

[TestClass]
public class TenantMiddlewareTests
{
    [TestMethod]
    public async Task InvokeAsync_WithTenantHeader_SetsTenantContext()
    {
        var tenantContext = new TenantContext();
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Tenant-ID"] = "tenant-a";

        var middleware = new TenantMiddleware(_ => Task.CompletedTask);
        await middleware.InvokeAsync(context, tenantContext);

        Assert.AreEqual("tenant-a", tenantContext.TenantId);
    }

    [TestMethod]
    public async Task InvokeAsync_WithoutTenantHeader_TenantContextRemainsNull()
    {
        var tenantContext = new TenantContext();
        var context = new DefaultHttpContext();

        var middleware = new TenantMiddleware(_ => Task.CompletedTask);
        await middleware.InvokeAsync(context, tenantContext);

        Assert.IsNull(tenantContext.TenantId);
    }

    [TestMethod]
    public async Task InvokeAsync_CallsNextMiddleware()
    {
        var tenantContext = new TenantContext();
        var context = new DefaultHttpContext();
        var nextCalled = false;

        var middleware = new TenantMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context, tenantContext);

        Assert.IsTrue(nextCalled);
    }
}
