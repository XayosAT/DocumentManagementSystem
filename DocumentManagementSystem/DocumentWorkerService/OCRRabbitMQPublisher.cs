using RabbitMQ.Client;
using System.Text;

namespace DocumentWorkerService;

public interface IMessagePublisher
{
    void Publish(string message, string routingKey);
}
public class OCRRabbitMQPublisher : IMessagePublisher, IDisposable
{
    private readonly IModel _channel;
    private string _exchangeName = "ocr_exchange";

    public OCRRabbitMQPublisher(IModel channel)
    {
        _channel = channel;
    }

    public void Publish(string message, string routingKey)
    {
        var body = Encoding.UTF8.GetBytes(message);
        
        _channel.BasicPublish(
            exchange: _exchangeName,
            routingKey: routingKey,
            basicProperties: null,
            body: body);
    }

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
    }
}

