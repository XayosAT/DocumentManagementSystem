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
using REST.RabbitMQ;
using SharedData.EntitiesDAL;


var builder = WebApplication.CreateBuilder(args);

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
    channel.ExchangeDeclare(
        exchange: exchangeName,
        type: ExchangeType.Direct);
    channel.QueueDeclare(
        queue: queueName,
        durable: false,
        exclusive: false,
        autoDelete: false);
    channel.QueueBind(
        queue: queueName,
        exchange: exchangeName,
        routingKey: routingKey);
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
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


// Register the DocumentRepository and IDocumentRepository for dependency injection
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();


// Register AutoMapper to map between DTOs and entities
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Configure Kestrel to listen on port 8081 for Docker
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8081); // Set the port to 8081
});

// Configure CORS to allow all origins, methods, and headers
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policyBuilder =>
    {
        policyBuilder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Add services to support API endpoint exploration (Swagger or OpenAPI)
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Apply CORS policy
app.UseCors();

// Apply authentication (optional, depending on your security setup)
app.UseAuthentication();

// Map the API controllers
app.MapControllers();

// Start the application
app.Run();
