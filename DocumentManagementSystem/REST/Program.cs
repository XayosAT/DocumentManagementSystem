using System.Diagnostics.CodeAnalysis;
using SharedData;
using SharedData.DTOs;
using REST.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using DAL.Repositories;
using DAL.Data;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using DAL.RabbitMQ;
using SharedData.EntitiesDAL;
using log4net;
using System.Reflection;
using DAL.Services;
using Microsoft.Extensions.Logging;
using DAL.Services;
using Minio;

var builder = WebApplication.CreateBuilder(args);

// Set up log4net configuration
var loggerRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
log4net.Config.XmlConfigurator.Configure(loggerRepository, new FileInfo("log4net.config"));

// Create an instance of the log4net logger
var logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

// Add logging services to ASP.NET Core's built-in logger factory
builder.Logging.ClearProviders(); // Remove default logging providers (e.g., Console, Debug)
builder.Logging.AddLog4Net();     // Add log4net as the logging provider

// Log startup information
logger.Info("Starting up the application...");

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

builder.Services.AddScoped<DocumentService>();

// Bind RabbitMQSettings
builder.Services.Configure<RabbitMQSettings>(builder.Configuration.GetSection("RabbitMQSettings"));

// Register RabbitMQ connection as singleton
builder.Services.AddSingleton<IConnection>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<RabbitMQSettings>>().Value;
    var queueName = settings.QueueName;
    var exchangeName = settings.ExchangeName;
    var routingKey = settings.RoutingKey;

    var factory = new ConnectionFactory
    {
        HostName = settings.HostName,
        Port = settings.Port,
        UserName = settings.UserName,
        Password = settings.Password,
        VirtualHost = settings.VirtualHost
    };

    var connection = factory.CreateConnection();
    // Declare the exchange once the connection is established
    using var channel = connection.CreateModel();
    channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Direct);
    channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false);
    channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: routingKey);

    logger.Info($"RabbitMQ connection successfully established with host {settings.HostName}");

    return connection;
});

// Register RabbitMQ channel as scoped
builder.Services.AddScoped<IModel>(sp =>
{
    var connection = sp.GetRequiredService<IConnection>();
    return connection.CreateModel();
});

builder.Services.AddScoped<IMessagePublisher, RabbitMQPublisher>();

// Add services to the container.
builder.Services.AddControllers();

// Add FluentValidation services to the container.
builder.Services.AddControllers()
    .AddFluentValidation(config =>
    {
        config.RegisterValidatorsFromAssemblyContaining<DocumentDALValidator>();
    });

// Register FluentValidation manually if needed
builder.Services.AddScoped<IValidator<DocumentDAL>, DocumentDALValidator>();

// Configure the PostgreSQL database connection using Entity Framework Core
builder.Services.AddDbContext<DocumentContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    logger.Info("Configuring PostgreSQL with connection string: DefaultConnection");
});

// Register the DocumentRepository and IDocumentRepository for dependency injection
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();

// Register AutoMapper to map between DTOs and entities
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Configure Kestrel to listen on port 8081 for Docker
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8081); // Set the port to 8081
    logger.Info("Kestrel is configured to listen on port 8081");
});

// Configure CORS to allow all origins, methods, and headers
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policyBuilder =>
    {
        policyBuilder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
        logger.Info("CORS is configured to allow all origins, methods, and headers");
    });
});

// Add services to support API endpoint exploration (Swagger or OpenAPI)
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Apply CORS policy
app.UseCors();

// Apply authentication (optional, depending on your security setup)
app.UseAuthentication();

// Log application startup complete
logger.Info("Application startup complete. Running now...");

// Map the API controllers
app.MapControllers();

// Start the application
try
{
    logger.Info("Starting the web host...");
    app.Run();
}
catch (Exception ex)
{
    logger.Error("An unexpected error occurred while starting the web host.", ex);
    throw;
}
finally
{
    logger.Info("Application is shutting down.");
}

[ExcludeFromCodeCoverage] public partial class Program {}
