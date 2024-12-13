using System.Reflection;
using log4net;
using log4net.Config;
using DocumentWorkerService;
using Microsoft.Extensions.Options;
using Minio;
using RabbitMQ.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// Set up log4net configuration
try
{
    var loggerRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
    XmlConfigurator.Configure(loggerRepository, new FileInfo("log4net.config"));
}
catch (Exception ex)
{
    Console.WriteLine($"Error configuring log4net: {ex.Message}");
    throw;
}

// Add log4net to the logging pipeline
builder.Logging.ClearProviders(); // Remove default logging providers
builder.Logging.AddLog4Net();     // Add log4net as the logging provider

var logger = LogManager.GetLogger(typeof(Program));

// Register services
builder.Services.AddSingleton<RabbitMQConsumer>();
builder.Services.AddSingleton<OCRService>();
builder.Services.AddHostedService<DocumentWorker>();
builder.Services.AddHostedService<RabbitMQConsumer>();  // RabbitMQConsumer should implement IHostedService

builder.Services.AddSingleton(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var endpoint = configuration["Minio:Endpoint"];
    var accessKey = configuration["Minio:AccessKey"];
    var secretKey = configuration["Minio:SecretKey"];
    var useSSL = configuration.GetValue<bool>("Minio:UseSSL");

    return new MinioClient()
        .WithEndpoint(endpoint)
        .WithCredentials(accessKey, secretKey)
        .WithSSL(useSSL)
        .Build();
});

logger.Info("Trying to create the queue...");

// Register RabbitMQ connection as singleton
builder.Services.AddSingleton<IConnection>(sp =>
{
    var factory = new ConnectionFactory
    {
        HostName = "rabbitmq",
        Port = 5672,
        UserName = "user",
        Password = "password",
        VirtualHost = "/"
    };

    logger.Info("Attempting to create RabbitMQ connection...");

    IConnection connection = null;
    int retryCount = 20;
    int currentTry = 0;
    bool connected = false;

    while (currentTry < retryCount && !connected)
    {
        try
        {
            connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            var exchangeName = "ocr_exchange";
            var queueName = "ocr_queue";
            var routingKey = "ocr_routing_key";

            // Declare the exchange and queue
            channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Direct);
            channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false);
            channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: routingKey);

            logger.Info($"RabbitMQ connection successfully established with host: {factory.HostName}");
            connected = true;
        }
        catch (RabbitMQ.Client.Exceptions.BrokerUnreachableException ex)
        {
            currentTry++;
            logger.Error($"Error connecting to RabbitMQ. Attempt {currentTry}/{retryCount}. Exception: {ex.Message}");
            if (currentTry < retryCount)
            {
                // Wait for 5 seconds before retrying
                Task.Delay(5000).Wait();
            }
        }
    }

    if (!connected)
    {
        logger.Error("Unable to connect to RabbitMQ after multiple attempts.");
        throw new Exception("Failed to connect to RabbitMQ after retries.");
    }

    return connection;
});


// Register RabbitMQ channel as scoped
builder.Services.AddScoped<IModel>(sp =>
{
    var connection = sp.GetRequiredService<IConnection>();
    return connection.CreateModel();
});

// Register OCRRabbitMQPublisher as IMessagePublisher (make sure it is implemented correctly)
builder.Services.AddScoped<IMessagePublisher, OCRRabbitMQPublisher>();

// Log startup information
logger.Info("Starting up Document Worker Service...");

var host = builder.Build();
host.Run();
