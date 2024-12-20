using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OrderService.Configuration;
using OrderService.Data;
using OrderService.Models;

namespace OrderService.Services;

public class ProcessingServiceClient : IProcessingServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly OrderDbContext _context;

    public ProcessingServiceClient(HttpClient httpClient, IOptions<ProcessingServiceSettings> settings, OrderDbContext dbContext)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(settings.Value.BaseUrl);
        _context = dbContext;
    }

    public async Task<bool> CreateOrderAsync(string orderId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsync($"/orders/{orderId}/process", null, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<Order?> GetOrderAsync(string orderId, CancellationToken cancellationToken = default)
    {
        // find order in DB first
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
        if (order != null) 
            return order;

        //get order from processing service
        var response = await _httpClient.GetAsync($"/orders/{orderId}", cancellationToken);
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<Order>(cancellationToken: cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetAllOrdersAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("/orders", cancellationToken);
        if (!response.IsSuccessStatusCode)
            return Array.Empty<Order>();

        return await response.Content.ReadFromJsonAsync<IEnumerable<Order>>(cancellationToken: cancellationToken)
               ?? Array.Empty<Order>();
    }

    public async Task<string?> GetOrderStatus (string orderId)
    {
        var dbOrder = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
        
        if (dbOrder != null)
            return dbOrder.Status.ToString();

        return null;

    }
}