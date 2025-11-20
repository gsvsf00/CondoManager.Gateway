using RabbitMQ.Client;
using System.Text;

namespace CondoManager.Api.Services
{
    public class RabbitMqService
    {
        private readonly ConnectionFactory _factory;

        public RabbitMqService()
        {
            _factory = new ConnectionFactory()
            {
                HostName = "localhost", // or "rabbitmq" if running in Docker
                UserName = "guest",
                Password = "guest"
            };
        }

        public void Publish(string exchange, string routingKey, string message)
        {
            using (var connection = _factory.CreateConnectionAsync().GetAwaiter().GetResult())
            using (var channel = connection.CreateChannelAsync().GetAwaiter().GetResult())
            {
                channel.ExchangeDeclareAsync(exchange: exchange, type: ExchangeType.Direct, durable: true)
                       .GetAwaiter().GetResult();

                var body = Encoding.UTF8.GetBytes(message);

                var props = new BasicProperties
                {
                    Persistent = true,
                    ContentType = "text/plain"
                };

                channel.BasicPublishAsync(
                    exchange: exchange,
                    routingKey: routingKey,
                    mandatory: false,
                    basicProperties: props,
                    body: body
                ).GetAwaiter().GetResult();
            }
        }
    }
}