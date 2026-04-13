using IntegrationBus.Services;

namespace IntegrationBus.Tests;

[TestClass]
public class TenantServiceTests
{
    private readonly TenantService _sut = new();

    [TestMethod]
    public async Task GetFakeUserNamesByTenantAsync_WithValidTenant_ReturnsList()
    {
        var result = await _sut.GetFakeUserNamesByTenantAsync("tenant-a");

        Assert.IsTrue(result.Count > 0);
        CollectionAssert.Contains(result.ToList(), "Alice");
    }

    [TestMethod]
    public async Task GetFakeUserNamesByTenantAsync_WithUnknownTenant_ReturnsEmptyList()
    {
        var result = await _sut.GetFakeUserNamesByTenantAsync("tenant-unknown");

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetFakeUserNamesByTenantAsync_WithNullTenant_ThrowsArgumentException()
    {
        await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _sut.GetFakeUserNamesByTenantAsync(null!));
    }

    [TestMethod]
    public async Task GetFakeUserNamesByTenantAsync_WithEmptyTenant_ThrowsArgumentException()
    {
        await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _sut.GetFakeUserNamesByTenantAsync(string.Empty));
    }
}
