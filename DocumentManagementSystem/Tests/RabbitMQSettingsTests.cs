namespace TestProject1;

using Xunit;

public class RabbitMQSettingsTests
{
    [Fact]
    public void RabbitMQSettings_ShouldSetAndGetProperties()
    {
        // Arrange
        var settings = new RabbitMQSettings();

        // Act
        settings.HostName = "localhost";
        settings.Port = 5672;
        settings.UserName = "guest";
        settings.Password = "guest";
        settings.VirtualHost = "/";
        settings.QueueName = "my-queue";
        settings.ExchangeName = "my-exchange";
        settings.RoutingKey = "my-routing-key";

        // Assert
        Assert.Equal("localhost", settings.HostName);
        Assert.Equal(5672, settings.Port);
        Assert.Equal("guest", settings.UserName);
        Assert.Equal("guest", settings.Password);
        Assert.Equal("/", settings.VirtualHost);
        Assert.Equal("my-queue", settings.QueueName);
        Assert.Equal("my-exchange", settings.ExchangeName);
        Assert.Equal("my-routing-key", settings.RoutingKey);
    }

    [Theory]
    [InlineData("localhost", 5672, "guest", "guest", "/", "my-queue", "my-exchange", "my-routing-key")]
    [InlineData("example.com", 5671, "user", "password", "my-vhost", "example-queue", "example-exchange", "example-key")]
    [InlineData("", 0, "", "", "", "", "", "")]
    public void RabbitMQSettings_ShouldSetAndGetVariousValues(
        string hostName,
        int port,
        string userName,
        string password,
        string virtualHost,
        string queueName,
        string exchangeName,
        string routingKey)
    {
        // Arrange
        var settings = new RabbitMQSettings
        {
            HostName = hostName,
            Port = port,
            UserName = userName,
            Password = password,
            VirtualHost = virtualHost,
            QueueName = queueName,
            ExchangeName = exchangeName,
            RoutingKey = routingKey
        };

        // Assert
        Assert.Equal(hostName, settings.HostName);
        Assert.Equal(port, settings.Port);
        Assert.Equal(userName, settings.UserName);
        Assert.Equal(password, settings.Password);
        Assert.Equal(virtualHost, settings.VirtualHost);
        Assert.Equal(queueName, settings.QueueName);
        Assert.Equal(exchangeName, settings.ExchangeName);
        Assert.Equal(routingKey, settings.RoutingKey);
    }
}
