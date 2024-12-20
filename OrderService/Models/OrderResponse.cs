namespace OrderService.Models;

public class OrderResponse
{
    public string OrderId { get; set; } = default!;
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? ErrorMessage { get; set; }

    public static OrderResponse FromOrder(Order order) =>
        new OrderResponse
        {
            OrderId = order.OrderId,
            Status = order.Status,
            CreatedAt = order.CreatedAt,
            ProcessedAt = order.ProcessedAt,
            ErrorMessage = order.ErrorMessage
        };
}