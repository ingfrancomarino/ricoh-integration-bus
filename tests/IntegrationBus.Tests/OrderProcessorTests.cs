using System.Text;
using System.Text.Json;
using IntegrationBus.Core.Interfaces;
using IntegrationBus.Services;

namespace IntegrationBus.Tests;

[TestClass]
public class OrderProcessorTests
{
    private readonly OrderProcessor _sut = new();
    private readonly IValidationHook _hook = new DefaultValidationHook();

    private static Stream ToStream(string json)
        => new MemoryStream(Encoding.UTF8.GetBytes(json));

    [TestMethod]
    public async Task ProcessTenantOrdersAsync_ValidOrders_YieldsAll()
    {
        var json = """
            {
              "orders": [
                { "id": "1", "status": "Active", "amount": 100, "product": "Widget" },
                { "id": "2", "status": "Active", "amount": 200, "product": "Gadget" }
              ]
            }
            """;

        var results = await _sut.ProcessTenantOrdersAsync(ToStream(json), _hook).ToListAsync();

        Assert.AreEqual(2, results.Count);
    }

    [TestMethod]
    public async Task ProcessTenantOrdersAsync_InvalidOrder_IsSkipped()
    {
        var json = """
            {
              "orders": [
                { "id": "1", "status": "Active", "amount": 100, "product": "Widget" },
                { "id": "",  "status": "",        "amount": -1,  "product": "Bad"    }
              ]
            }
            """;

        var results = await _sut.ProcessTenantOrdersAsync(ToStream(json), _hook).ToListAsync();

        Assert.AreEqual(1, results.Count);
        Assert.AreEqual("1", results[0].Id);
    }

    [TestMethod]
    public async Task ProcessTenantOrdersAsync_EmptyArray_YieldsNothing()
    {
        var json = """{ "orders": [] }""";

        var results = await _sut.ProcessTenantOrdersAsync(ToStream(json), _hook).ToListAsync();

        Assert.AreEqual(0, results.Count);
    }
}

internal static class AsyncEnumerableExtensions
{
    public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source)
    {
        var list = new List<T>();
        await foreach (var item in source)
            list.Add(item);
        return list;
    }
}
