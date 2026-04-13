namespace IntegrationBus.Core.Models;

public class ProcessOrdersRequest
{
    public Dictionary<string, string> Filters { get; set; } = new();
    public List<Order> Orders { get; set; } = new();
}
