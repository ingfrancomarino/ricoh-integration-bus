namespace IntegrationBus.Core.Interfaces;

public interface ITenantService
{
    Task<IReadOnlyList<string>> GetFakeUserNamesByTenantAsync(string tenantId);
}
