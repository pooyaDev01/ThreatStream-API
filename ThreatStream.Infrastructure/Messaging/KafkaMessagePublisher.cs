using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System.Text.Json;
using ThreatStream.Contracts.Messaging;
using ThreatStream.Infrastructure.Configuration;

namespace ThreatStream.Infrastructure.Messaging
{
    public class KafkaMessagePublisher : IMessagePublisher, IDisposable
    {
        private readonly IProducer<string, string> _producer;

        public KafkaMessagePublisher(IOptions<KafkaOptions> options)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = options.Value.BootstrapServers
            };

            _producer = new ProducerBuilder<string, string>(config).Build();
        }

        public async Task PublishAsync<T>(T message, string topic, string? key = null)
        {
            var json = JsonSerializer.Serialize(message);

            await _producer.ProduceAsync(
                topic,
                new Message<string, string>
                {
                    Key = key!,
                    Value = json
                });
        }

        public void Dispose()
        {
            _producer.Dispose();
        }
    }
}
