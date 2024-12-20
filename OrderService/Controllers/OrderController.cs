using Microsoft.AspNetCore.Mvc;
using OrderService.Models;
using OrderService.Services;

namespace OrderService.Controllers;

[ApiController]
[Route("[controller]")]
public class OrderController : ControllerBase
{
    private readonly IProcessingServiceClient _processingService;
    private readonly ILogger<OrderController> _logger;

    public OrderController(IProcessingServiceClient processingService, ILogger<OrderController> logger)
    {
        _processingService = processingService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] OrderRequest request)
    {
        var result = await _processingService.CreateOrderAsync(request.OrderId);
        if (!result)
            return BadRequest("Failed to create order");

        return Ok("Order created successfully");
    }

    [HttpGet("{orderId}")]
    public async Task<ActionResult<OrderResponse>> GetOrder(string orderId)
    {
        var order = await _processingService.GetOrderAsync(orderId);
        if (order == null)
            return NotFound();

        return OrderResponse.FromOrder(order);
    }


    [HttpGet("status/{orderId}")]
    public async Task<IActionResult> GetOrderStatus(string orderId)
    {
        string orderStatus = await _processingService.GetOrderStatus(orderId);
        if (orderStatus == null)
            return NotFound();

        return Ok(orderStatus);
    }
}