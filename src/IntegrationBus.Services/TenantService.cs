using IntegrationBus.Core.Interfaces;

namespace IntegrationBus.Services;

public class TenantService : ITenantService
{
    private static readonly Dictionary<string, List<string>> _fakeData = new()
    {
        ["tenant-a"] = ["Alice", "Bob", "Charlie"],
        ["tenant-b"] = ["Diana", "Eve", "Frank"],
    };

    /// <summary>
    /// Retrieves a simulated list of user names associated with the specified tenant.
    /// </summary>
    /// <param name="tenantId">
    /// The unique identifier of the tenant. Must not be <c>null</c> or empty.
    /// </param>
    /// <returns>
    /// A list of user names belonging to the tenant.
    /// Returns an empty list if the tenant is not found.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="tenantId"/> is <c>null</c> or an empty string.
    /// </exception>
    public Task<List<string>> GetFakeUserNamesByTenantAsync(string tenantId)
    {
        if (string.IsNullOrEmpty(tenantId))
            throw new ArgumentException("Tenant ID must not be null or empty.", nameof(tenantId));

        var result = _fakeData.TryGetValue(tenantId, out var users)
            ? new List<string>(users)
            : [];

        return Task.FromResult(result);
    }
}
