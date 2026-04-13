namespace IntegrationBus.Core.Models;

public class Order
{
    public string Id { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Product { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
