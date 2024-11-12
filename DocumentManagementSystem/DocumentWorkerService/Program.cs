using System.Reflection;
using log4net;
using log4net.Config;
using DocumentWorkerService;
using Minio;

var builder = Host.CreateApplicationBuilder(args);

// Register RabbitMQConsumer as a transient or singleton service
builder.Services.AddSingleton<RabbitMQConsumer>();
builder.Services.AddSingleton<OCRService>();
builder.Services.AddHostedService<DocumentWorker>();
builder.Services.AddHostedService<RabbitMQConsumer>();

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

// Log startup information
logger.Info("Starting up Document Worker Service...");

var host = builder.Build();
host.Run();