namespace TestProject1;

using Moq;
using RabbitMQ.Client;
using System.Text;
using Xunit;
using DAL.RabbitMQ;

public class RabbitMQPublisherTests
{
    private readonly Mock<IModel> _channelMock;
    private readonly RabbitMQPublisher _publisher;

    public RabbitMQPublisherTests()
    {
        // Set up the mock for IModel, which represents the RabbitMQ channel
        _channelMock = new Mock<IModel>();

        // Create an instance of RabbitMQPublisher using the mocked channel
        _publisher = new RabbitMQPublisher(_channelMock.Object);
    }

    // [Fact]
    // public void Publish_ShouldCallBasicPublish_WithExpectedArguments()
    // {
    //     // Arrange
    //     var message = "Test Message";
    //     var routingKey = "test.routing.key";
    //     var exchangeName = "dms_exchange";
    //     var body = Encoding.UTF8.GetBytes(message);
    //
    //     // Act
    //     _publisher.Publish(message, routingKey);
    //
    //     // Assert
    //     // Verify that BasicPublish was called with the expected parameters
    //     _channelMock.Verify(channel => channel.BasicPublish(
    //         exchange: exchangeName,
    //         routingKey: routingKey,
    //         basicProperties: null,
    //         body: It.Is<byte[]>(b => Encoding.UTF8.GetString(b) == message)),
    //         Times.Once,
    //         "BasicPublish should be called exactly once with the given parameters.");
    // }

    [Fact]
    public void Dispose_ShouldCloseAndDisposeTheChannel()
    {
        // Act
        _publisher.Dispose();

        // Assert
        // Verify that the Close method was called once
        _channelMock.Verify(channel => channel.Close(), Times.Once, "Close should be called exactly once.");

        // Verify that the Dispose method was called once
        _channelMock.Verify(channel => channel.Dispose(), Times.Once, "Dispose should be called exactly once.");
    }

    // [Fact]
    // public void Publish_ShouldThrowException_WhenChannelIsNull()
    // {
    //     // Arrange
    //     var nullPublisher = new RabbitMQPublisher(null);
    //
    //     // Act & Assert
    //     Assert.Throws<NullReferenceException>(() => nullPublisher.Publish("Test Message", "test.routing.key"));
    // }

    [Fact]
    public void Publish_ShouldNotThrow_WhenMessageIsEmpty()
    {
        // Arrange
        var emptyMessage = "";

        // Act & Assert
        var exception = Record.Exception(() => _publisher.Publish(emptyMessage, "test.routing.key"));
        Assert.Null(exception); // Ensure no exception is thrown when publishing an empty message
    }

    [Fact]
    public void Publish_ShouldNotThrow_WhenRoutingKeyIsEmpty()
    {
        // Arrange
        var routingKey = "";

        // Act & Assert
        var exception = Record.Exception(() => _publisher.Publish("Test Message", routingKey));
        Assert.Null(exception); // Ensure no exception is thrown when publishing with an empty routing key
    }
}
