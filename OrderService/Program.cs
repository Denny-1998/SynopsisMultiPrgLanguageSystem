using Microsoft.EntityFrameworkCore;
using System.Runtime;
using OrderService.Configuration;
using OrderService.Data;
using OrderService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



// Configure settings
builder.Services.Configure<ProcessingServiceSettings>(
    builder.Configuration.GetSection(ProcessingServiceSettings.SectionName));
builder.Services.Configure<RabbitMQSettings>(
    builder.Configuration.GetSection(RabbitMQSettings.SectionName));

// Register services
builder.Services.AddHttpClient<IProcessingServiceClient, ProcessingServiceClient>();
builder.Services.AddSingleton<IOrderStatusConsumer, OrderStatusConsumer>();


// add db
DbSettings dbSettings = new DbSettings
{
    SqlConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
};
builder.Services.AddSingleton(dbSettings);

builder.Services.AddDbContext<OrderDbContext>(options => options.UseSqlServer(dbSettings.SqlConnectionString));



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    dbContext.Database.Migrate();  
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Start RabbitMQ consumer
var consumer = app.Services.GetRequiredService<IOrderStatusConsumer>();
await consumer.StartAsync();

app.Run();
