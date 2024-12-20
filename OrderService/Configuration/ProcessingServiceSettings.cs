namespace OrderService.Configuration;

public class ProcessingServiceSettings
{
    public const string SectionName = "ProcessingService";
    public string BaseUrl { get; set; } = default!;
}