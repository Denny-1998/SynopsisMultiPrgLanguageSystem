namespace OrderService.Configuration;

public class RabbitMQSettings
{
    public const string SectionName = "RabbitMQ";
    public string HostName { get; set; } = default!;
    public string QueueName { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public string Password { get; set; } = default!;
}