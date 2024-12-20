using System.Text.Json.Serialization;

namespace OrderService.Models;

public class Order
{
    [JsonPropertyName("order_id")]

    public string OrderId { get; set; } = default!;
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? ErrorMessage { get; set; }
}