using IntegrationBus.Core.Models;
using IntegrationBus.Services;

namespace IntegrationBus.Tests;

[TestClass]
public class DynamicFilterServiceTests
{
    private readonly DynamicFilterService _sut = new();

    private static List<Order> SampleOrders() =>
    [
        new Order { Id = "1", Status = "Active",    Amount = 1000, Product = "Widget" },
        new Order { Id = "2", Status = "Cancelled", Amount = 200,  Product = "Gadget" },
        new Order { Id = "3", Status = "Active",    Amount = 500,  Product = "Widget" },
    ];

    [TestMethod]
    public void BuildFilter_ByStatus_ReturnsMatchingOrders()
    {
        var filter = _sut.BuildFilter(new() { ["Status"] = "Active" }).Compile();
        var result = SampleOrders().Where(filter).ToList();

        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public void BuildFilter_ByMinAmount_ReturnsMatchingOrders()
    {
        var filter = _sut.BuildFilter(new() { ["MinAmount"] = "500" }).Compile();
        var result = SampleOrders().Where(filter).ToList();

        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public void BuildFilter_CombinedFilters_ReturnsCorrectOrders()
    {
        var filter = _sut.BuildFilter(new()
        {
            ["Status"] = "Active",
            ["MinAmount"] = "600"
        }).Compile();
        var result = SampleOrders().Where(filter).ToList();

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("1", result[0].Id);
    }

    [TestMethod]
    public void BuildFilter_NoFilters_ReturnsAll()
    {
        var filter = _sut.BuildFilter(new()).Compile();
        var result = SampleOrders().Where(filter).ToList();

        Assert.AreEqual(3, result.Count);
    }

    [TestMethod]
    public void BuildFilter_UnknownKey_ThrowsArgumentException()
    {
        Assert.ThrowsException<ArgumentException>(
            () => _sut.BuildFilter(new() { ["Unknown"] = "value" }));
    }
}
