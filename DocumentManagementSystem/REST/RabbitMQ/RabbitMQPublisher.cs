using RabbitMQ.Client;
using System.Text;

namespace REST.RabbitMQ;

public interface IMessagePublisher
{
    void Publish(string message, string routingKey);
}
public class RabbitMQPublisher : IMessagePublisher, IDisposable
{
    private readonly IModel _channel;
    private string _exchangeName = "dms_exchange";

    public RabbitMQPublisher(IModel channel)
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

