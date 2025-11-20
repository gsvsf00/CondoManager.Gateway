using CondoManager.Api.Config;
using CondoManager.Api.Services.Interfaces;
using CondoManager.Entity.Events;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace CondoManager.Api.Services
{
    public class RabbitMqConsumerService : BackgroundService
    {
        private readonly IConnection _connection;
        private readonly IChannel _channel;
        private readonly RabbitMqOptions _options;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RabbitMqConsumerService> _logger;

        public RabbitMqConsumerService(
            IOptions<RabbitMqOptions> options,
            IServiceProvider serviceProvider,
            ILogger<RabbitMqConsumerService> logger)
        {
            _options = options.Value;
            _serviceProvider = serviceProvider;
            _logger = logger;

            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password
            };

            _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
            _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();

            // Declare exchange
            _channel.ExchangeDeclareAsync(
                exchange: _options.ExchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false).GetAwaiter().GetResult();

            // Declare queue for message events
            _channel.QueueDeclareAsync(
                queue: "api.message.events",
                durable: true,
                exclusive: false,
                autoDelete: false).GetAwaiter().GetResult();

            // Bind queue to exchange with routing keys
            _channel.QueueBindAsync(
                queue: "api.message.events",
                exchange: _options.ExchangeName,
                routingKey: "message.received").GetAwaiter().GetResult();

            _channel.QueueBindAsync(
                queue: "api.message.events",
                exchange: _options.ExchangeName,
                routingKey: "message.read").GetAwaiter().GetResult();

            _channel.QueueBindAsync(
                queue: "api.message.events",
                exchange: _options.ExchangeName,
                routingKey: "message.delivered").GetAwaiter().GetResult();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("RabbitMQ Consumer Service started");

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var routingKey = ea.RoutingKey;

                    _logger.LogInformation($"Received message with routing key: {routingKey}");

                    using var scope = _serviceProvider.CreateScope();
                    var messageHandler = scope.ServiceProvider.GetRequiredService<IMessageEventHandler>();

                    switch (routingKey)
                    {
                        case "message.received":
                            var messageEvent = JsonSerializer.Deserialize<MessageReceivedEvent>(message);
                            if (messageEvent != null)
                            {
                                await messageHandler.HandleMessageReceivedAsync(messageEvent);
                            }
                            break;

                        case "message.read":
                            var readEvent = JsonSerializer.Deserialize<MessageReadEvent>(message);
                            if (readEvent != null)
                            {
                                await messageHandler.HandleMessageReadAsync(readEvent);
                            }
                            break;

                        case "message.delivered":
                            var deliveredEvent = JsonSerializer.Deserialize<MessageDeliveredEvent>(message);
                            if (deliveredEvent != null)
                            {
                                await messageHandler.HandleMessageDeliveredAsync(deliveredEvent);
                            }
                            break;

                        default:
                            _logger.LogWarning($"Unknown routing key: {routingKey}");
                            break;
                    }

                    // Acknowledge the message
                    await _channel.BasicAckAsync(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message");
                    // Reject and requeue the message
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, true);
                }
            };

            await _channel.BasicConsumeAsync(
                queue: "api.message.events",
                autoAck: false,
                consumer: consumer);

            // Keep the service running
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }

        public override void Dispose()
        {
            try
            {
                _channel?.Dispose();
                _connection?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing RabbitMQ consumer");
            }
            base.Dispose();
        }
    }
}