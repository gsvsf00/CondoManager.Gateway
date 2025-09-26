using CondoManager.Api.Config;
// removed incorrect using that conflicted with RabbitMQ IModel
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace CondoManager.Api.Infrastructure
{
    public class RabbitMqPublisher : IDisposable
    {
        private readonly IConnection _connection;
        private readonly IChannel _channel;
        private readonly RabbitMqOptions _options;
        private readonly ILogger<RabbitMqPublisher> _logger;

        public RabbitMqPublisher(IOptions<RabbitMqOptions> options, ILogger<RabbitMqPublisher> logger)
        {
            _options = options.Value;
            _logger = logger;

            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password
            };

            // Create connection and channel (compatible with RabbitMQ.Client 7.x)
            _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
            _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();

            // Declare exchange
            _channel.ExchangeDeclareAsync(exchange: _options.ExchangeName, type: ExchangeType.Topic, durable: true, autoDelete: false)
                    .GetAwaiter().GetResult();
        }

        public async Task PublishAsync(object evt, string routingKey = "domain.event")
        {
            try
            {
                var json = JsonSerializer.Serialize(evt);
                var body = Encoding.UTF8.GetBytes(json);

                // v7: Create BasicProperties via constructor instead of channel.CreateBasicProperties()
                var props = new BasicProperties
                {
                    Persistent = true,
                    ContentType = "application/json"
                };

                await _channel.BasicPublishAsync(
                    exchange: _options.ExchangeName,
                    routingKey: routingKey,
                    mandatory: false,
                    basicProperties: props,
                    body: body
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish event to RabbitMQ");
                throw; // bubble up so callers can handle
            }
        }

        public void Dispose()
        {
            // In 7.x both IChannel and IConnection implement IDisposable
            try
            {
                _channel?.Dispose();
                _connection?.Dispose();
            }
            catch { /* ignore */ }
        }
    }
}
