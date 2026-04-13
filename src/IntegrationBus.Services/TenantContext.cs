using IntegrationBus.Core.Interfaces;

namespace IntegrationBus.Services;

public class TenantContext : ITenantContext
{
    public string? TenantId { get; set; }
}
