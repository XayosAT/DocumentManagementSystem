using RabbitMQ.Client;
using System.Text;
using RabbitMQ.Client.Exceptions;

namespace DAL.RabbitMQ;

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
        try
        {
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(
                exchange: _exchangeName,
                routingKey: routingKey,
                basicProperties: null,
                body: body);

            Console.WriteLine($"Message published to exchange '{_exchangeName}' with routing key '{routingKey}'.");
        }
        catch (AlreadyClosedException ex)
        {
            Console.Error.WriteLine($"Failed to publish message because the channel is already closed: {ex.Message}");
            // You might want to retry or handle reconnection here
        }
        catch (BrokerUnreachableException ex)
        {
            Console.Error.WriteLine($"Failed to reach RabbitMQ broker: {ex.Message}");
            // Handle reconnection, logging, or alternate strategies
        }
        catch (OperationInterruptedException ex)
        {
            Console.Error.WriteLine($"Operation interrupted during message publishing: {ex.Message}");
            // This might indicate an issue with the RabbitMQ broker or channel closure
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"An unexpected error occurred while publishing a message: {ex.Message}");
            // Ensure any other unexpected errors are logged properly
        }
    }

    public void Dispose()
    {
        try
        {
            _channel?.Close();
            _channel?.Dispose();
        }
        catch (AlreadyClosedException ex)
        {
            Console.Error.WriteLine($"Channel is already closed during Dispose operation: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"An error occurred while disposing the RabbitMQ channel: {ex.Message}");
        }
    }
}

