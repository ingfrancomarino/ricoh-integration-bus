namespace IntegrationBus.Core.Interfaces;

public interface ITenantContext
{
    string? TenantId { get; set; }
}
