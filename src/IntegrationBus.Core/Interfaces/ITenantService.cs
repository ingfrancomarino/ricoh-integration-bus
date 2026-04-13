namespace IntegrationBus.Core.Interfaces;

public interface ITenantService
{
    Task<List<string>> GetFakeUserNamesByTenantAsync(string tenantId);
}
