using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using OrderService.Configuration;
using OrderService.Data;
using OrderService.Models;
using System.Threading;
using Microsoft.EntityFrameworkCore;

namespace OrderService.Services;

public class OrderStatusConsumer : IOrderStatusConsumer, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _queueName;
    private readonly IServiceScopeFactory _serviceScopeFactory;


    public OrderStatusConsumer(IOptions<RabbitMQSettings> settings, IServiceScopeFactory serviceScopeFactory)
    {
        var config = settings.Value;
        var factory = new ConnectionFactory
        {
            HostName = config.HostName,
            UserName = config.UserName,
            Password = config.Password,
            Port = 5672
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _queueName = config.QueueName;
        _serviceScopeFactory = serviceScopeFactory;

        _channel.QueueDeclare(
            queue: _queueName,
            durable: false,    
            exclusive: false,
            autoDelete: false,
            arguments: null);
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (_, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            Console.WriteLine($"Received status update: {message}");
            StoreOrderInDb(ConvertOrder(message));
        };

        _channel.BasicConsume(
            queue: _queueName,
            autoAck: true,
            consumer: consumer);

        return Task.CompletedTask;
    }

    private void StoreOrderInDb(Order order)
    {
        if (order == null)
            return;

        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
            dbContext.Orders.Add(order);
            dbContext.SaveChanges();
        }
        
    }

    private Order ConvertOrder(string message)
    {
        try
        {
            var order = JsonSerializer.Deserialize<Order>(message);
            return order;
            
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Error deserializing message: {ex.Message}");
            return null;
        }

    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}