using OrderService.Models;

namespace OrderService.Services;

public interface IProcessingServiceClient
{
    Task<bool> CreateOrderAsync(string orderId, CancellationToken cancellationToken = default);
    Task<Order?> GetOrderAsync(string orderId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetAllOrdersAsync(CancellationToken cancellationToken = default);
    Task<string?> GetOrderStatus(string orderId);

}