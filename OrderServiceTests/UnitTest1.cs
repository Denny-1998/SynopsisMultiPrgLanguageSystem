using Microsoft.Extensions.Logging;
using Moq;
using OrderService.Controllers;
using OrderService.Models;
using OrderService.Services;
using Microsoft.AspNetCore.Mvc;

namespace OrderService.Tests;

public class UnitTest1
{
    private readonly Mock<IProcessingServiceClient> _mockProcessingService;
    private readonly Mock<ILogger<OrderController>> _mockLogger;
    private readonly OrderController _controller;

    public UnitTest1()
    {
        _mockProcessingService = new Mock<IProcessingServiceClient>();
        _mockLogger = new Mock<ILogger<OrderController>>();
        _controller = new OrderController(_mockProcessingService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task CreateOrder_Success_ReturnsOkResult()
    {
        // Arrange
        var request = new OrderRequest { OrderId = "test-1" };
        _mockProcessingService.Setup(x => x.CreateOrderAsync(request.OrderId, default))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.CreateOrder(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Order created successfully", okResult.Value);
    }

    [Fact]
    public async Task GetOrder_ExistingOrder_ReturnsOrder()
    {
        // Arrange
        var orderId = "test-1";
        var order = new Order
        {
            OrderId = orderId,
            Status = OrderStatus.Completed,
            CreatedAt = DateTime.UtcNow
        };

        _mockProcessingService.Setup(x => x.GetOrderAsync(orderId, default))
            .ReturnsAsync(order);

        // Act
        var result = await _controller.GetOrder(orderId);

        // Assert
        var okResult = Assert.IsType<ActionResult<OrderResponse>>(result);
        var orderResponse = Assert.IsType<OrderResponse>(result.Value);
        Assert.Equal(orderId, orderResponse.OrderId);
        Assert.Equal(OrderStatus.Completed, orderResponse.Status);
    }

    [Fact]
    public async Task GetOrder_NonExistingOrder_ReturnsNotFound()
    {
        // Arrange
        var orderId = "non-existing";
        _mockProcessingService.Setup(x => x.GetOrderAsync(orderId, default))
            .ReturnsAsync((Order?)null);

        // Act
        var result = await _controller.GetOrder(orderId);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }
}