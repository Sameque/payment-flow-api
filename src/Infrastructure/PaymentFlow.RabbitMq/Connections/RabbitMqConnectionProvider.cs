using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace PaymentFlow.RabbitMq;

public sealed class RabbitMqConnectionProvider(IOptions<RabbitMqOptions> options) : IRabbitMqConnectionProvider, IDisposable
{
    private readonly RabbitMqOptions options = options.Value;
    private readonly object gate = new();
    private IConnection? connection;

    public IModel CreateChannel()
    {
        lock (gate)
        {
            connection ??= CreateConnection();
            return connection.CreateModel();
        }
    }

    public void Dispose()
    {
        connection?.Dispose();
    }

    private IConnection CreateConnection()
    {
        var factory = new ConnectionFactory
        {
            HostName = options.HostName,
            Port = options.Port,
            UserName = options.UserName,
            Password = options.Password,
            VirtualHost = options.VirtualHost,
            DispatchConsumersAsync = true
        };

        return factory.CreateConnection("paymentflow");
    }
}
