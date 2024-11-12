using System.Text;
using log4net;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DocumentWorkerService;


public class RabbitMQConsumer : BackgroundService
{
    private static readonly ILog _logger = LogManager.GetLogger(typeof(RabbitMQConsumer));
    private readonly string _hostname;
    private readonly string _queueName;
    private readonly string _username;
    private readonly string _password;
    private IConnection _connection;
    private IModel _channel;
    private readonly OCRService _ocrService;
    private readonly IConfiguration _configuration;

    public RabbitMQConsumer(OCRService ocrService, IConfiguration configuration)
    {
        _ocrService = ocrService;
        _configuration = configuration;
        _hostname = configuration.GetValue<string>("RabbitMQSettings:HostName") ?? "rabbitmq";
        _queueName = configuration.GetValue<string>("RabbitMQSettings:QueueName") ?? "dms_queue";
        _username = configuration.GetValue<string>("RabbitMQSettings:UserName") ?? "user";
        _password = configuration.GetValue<string>("RabbitMQSettings:Password") ?? "password";

        InitializeRabbitMQListener();
    }

    private void InitializeRabbitMQListener()
    {
        var factory = new ConnectionFactory()
        {
            HostName = _hostname,
            UserName = _username,
            Password = _password
        };

        int retryCount = 10;
        int currentTry = 0;
        bool connected = false;

        while (currentTry < retryCount && !connected)
        {
            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                _channel.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
                _logger.Info("RabbitMQ Listener initialized and waiting for messages...");
                connected = true;
            }
            catch (RabbitMQ.Client.Exceptions.BrokerUnreachableException ex)
            {
                currentTry++;
                _logger.Error($"Error connecting to RabbitMQ. Attempt {currentTry}/{retryCount}. Exception: {ex.Message}");
                Task.Delay(5000).Wait();  // Wait 5 seconds before retrying
            }
        }

        if (!connected)
        {
            _logger.Error("Unable to connect to RabbitMQ after multiple attempts.");
            throw new Exception("Failed to connect to RabbitMQ after retries.");
        }
    }


    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        // Ensure that the consumer is properly initialized and message acknowledgment is manual
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (ch, ea) =>
        {
            var content = Encoding.UTF8.GetString(ea.Body.ToArray());
            _logger.Info($"Message received: {content}");

            try
            {
                // Call your processing method here
                ProcessMessage(content);
                // Acknowledge the message if processing is successful
                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                // Log the exception and do not acknowledge the message if an error occurs
                _logger.Error($"Error processing message: {ex.Message}");
                // Optionally, you can use BasicNack or reject the message here
                _channel.BasicNack(ea.DeliveryTag, false, true);
            }
        };

        // Set autoAck to false for manual message acknowledgment
        _channel.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);
    
        return Task.CompletedTask;
    }


    private async void ProcessMessage(string message)
    {
        try
        {
            // Your message processing logic here
            _logger.Info($"Processing message: {message}");
            var document = await _ocrService.PerformOCRAsync(message);
            _logger.Info($"OCR processing completed for document: {document}");
            // Example: Deserialize message and process document for OCR
            // DocumentDto document = JsonSerializer.Deserialize<DocumentDto>(message);
            // PerformOCR(document);

        }
        catch (Exception ex)
        {
            _logger.Error($"Error processing message: {ex.Message}");
            // Handle exceptions as necessary
        }
    }

    public override void Dispose()
    {
        _channel.Close();
        _connection.Close();
        base.Dispose();
    }
}